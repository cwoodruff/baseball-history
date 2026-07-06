using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages;

public class SearchModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int DropdownPlayerLimit = 10;
    private const int DropdownTeamLimit = 5;
    private const int PageSize = 25;

    public SearchViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? q, [FromQuery] int page = 1)
    {
        // The navbar dropdown issues targeted htmx requests; boosted navigations
        // and plain GETs render the full results page. Pagination on that page
        // targets #search-page-content.
        var isPagePartial = Request.IsHtmxNonBoostedRequest() &&
                            Request.GetHtmxTarget() == "search-page-content";
        var isDropdown = Request.IsHtmxNonBoostedRequest() && !isPagePartial;

        ViewModel.Query = q?.Trim() ?? "";
        ViewModel.PageSize = PageSize;

        if (ViewModel.Query.Length >= 2)
        {
            await RunSearchAsync(isDropdown, page);
        }

        if (isDropdown) return Partial("_SearchResults", ViewModel);
        if (isPagePartial) return Partial("_SearchPageResults", ViewModel);
        return Page();
    }

    private async Task RunSearchAsync(bool isDropdown, int page)
    {
        // ILIKE keeps the filter index-friendly (a pg_trgm GIN index can serve
        // it — see scripts/add-search-trgm-indexes.sql) instead of wrapping
        // every column in ToLower(), which forces a full scan.
        var pattern = "%" + EscapeLikePattern(ViewModel.Query) + "%";

        var hofPlayerIds = await GetHallOfFamePlayerIdsAsync();
        var prominence = await GetPlayerProminenceAsync();

        var matches = await context.People
            .Where(p => EF.Functions.ILike(p.NameFirst!, pattern) ||
                        EF.Functions.ILike(p.NameLast!, pattern) ||
                        EF.Functions.ILike(p.NameFirst + " " + p.NameLast, pattern))
            .Select(p => new PlayerMatch(p.PlayerId, p.NameFirst, p.NameLast, p.Debut, p.FinalGame))
            .ToListAsync();

        // Rank by career prominence from the cached lookup rather than
        // recomputing Sum(HR)+Sum(W) subqueries per candidate in the database.
        var rankedPlayers = matches
            .OrderByDescending(p => prominence.GetValueOrDefault(p.PlayerId))
            .ThenBy(p => p.NameLast)
            .ToList();

        ViewModel.TotalPlayerCount = rankedPlayers.Count;

        List<PlayerMatch> playerSlice;
        if (isDropdown)
        {
            playerSlice = rankedPlayers.Take(DropdownPlayerLimit).ToList();
        }
        else
        {
            ViewModel.TotalPages = (int)Math.Ceiling((double)rankedPlayers.Count / PageSize);
            ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));
            playerSlice = rankedPlayers
                .Skip((ViewModel.CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        ViewModel.Players = playerSlice.Select(p => new SearchResult
        {
            Id = p.PlayerId,
            Title = $"{p.NameFirst} {p.NameLast}".Trim(),
            Subtitle = p.Debut.HasValue
                ? $"{p.Debut.Value.Year} - {p.FinalGame?.Year.ToString() ?? "Present"}"
                : null,
            Type = SearchResultType.Player,
            Initials = GetInitials(p.NameFirst, p.NameLast),
            IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId)
        }).ToList();

        var franchiseQuery = context.TeamsFranchises
            .Where(f => EF.Functions.ILike(f.FranchName!, pattern));

        ViewModel.TotalTeamCount = await franchiseQuery.CountAsync();

        var franchises = await franchiseQuery
            .OrderByDescending(f => f.Active == "Y")
            .ThenBy(f => f.FranchName)
            .Take(isDropdown ? DropdownTeamLimit : 50)
            .ToListAsync();

        // Get latest team ID for each franchise
        var franchiseIds = franchises.Select(f => f.FranchId).ToList();
        var latestTeams = await context.Teams
            .Where(t => t.FranchId != null && franchiseIds.Contains(t.FranchId))
            .GroupBy(t => t.FranchId)
            .Select(g => new { FranchId = g.Key, TeamId = g.OrderByDescending(t => t.YearId).First().TeamId })
            .ToDictionaryAsync(x => x.FranchId!, x => x.TeamId);

        ViewModel.Teams = franchises.Select(f => new SearchResult
        {
            Id = f.FranchId,
            Title = f.FranchName ?? f.FranchId,
            Subtitle = f.Active == "Y" ? "Active Franchise" : "Historical Franchise",
            Type = SearchResultType.Franchise,
            Initials = GetInitials(f.FranchName, null),
            TeamId = latestTeams.GetValueOrDefault(f.FranchId)
        }).ToList();
    }

    private async Task<HashSet<string>> GetHallOfFamePlayerIdsAsync()
    {
        return (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;
    }

    private async Task<Dictionary<string, int>> GetPlayerProminenceAsync()
    {
        return (await cache.GetOrCreateAsync("player_prominence", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var homeRuns = await context.Batting
                .GroupBy(b => b.PlayerId)
                .Select(g => new { g.Key, Value = g.Sum(b => (int?)b.Hr) ?? 0 })
                .ToListAsync();

            var wins = await context.Pitching
                .GroupBy(p => p.PlayerId)
                .Select(g => new { g.Key, Value = g.Sum(p => (int?)p.W) ?? 0 })
                .ToListAsync();

            var prominence = new Dictionary<string, int>();
            foreach (var entry2 in homeRuns) prominence[entry2.Key] = entry2.Value;
            foreach (var entry2 in wins) prominence[entry2.Key] = prominence.GetValueOrDefault(entry2.Key) + entry2.Value;
            return prominence;
        }))!;
    }

    private static string EscapeLikePattern(string term)
    {
        return term
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }

    private static string GetInitials(string? firstName, string? lastName)
    {
        var initials = "";
        if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
            initials += firstName[0];
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            initials += lastName[0];
        if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(firstName))
            initials = firstName.Length >= 2 ? firstName.Substring(0, 2) : firstName;
        return initials.ToUpper();
    }

    private sealed record PlayerMatch(string PlayerId, string? NameFirst, string? NameLast, DateOnly? Debut, DateOnly? FinalGame);
}
