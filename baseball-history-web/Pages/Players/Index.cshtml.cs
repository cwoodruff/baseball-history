using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Players;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 48;

    public PlayerListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? letter, [FromQuery] int page = 1,
        [FromQuery] string? q = null, [FromQuery] string? pos = null,
        [FromQuery] int? era = null, [FromQuery] string? sort = null)
    {
        var sortBy = PlayerListViewModel.SortOptions.Any(o => o.Value == sort) ? sort! : "name";
        var searchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        var position = PlayerListViewModel.PositionOptions.Any(o => o.Value == pos) ? pos : null;
        var eraDecade = era is >= 1870 and <= 2030 ? era - era % 10 : null;
        var hasFilters = searchQuery != null || position != null || eraDecade.HasValue || sortBy != "name";

        // Serve from pre-warmed cache for the default view (letter A, page 1, full page load)
        var isDefaultRequest = (letter == null || letter.Equals("A", StringComparison.OrdinalIgnoreCase)) &&
                               page <= 1 && !hasFilters;
        if (isDefaultRequest && !Request.IsHtmxNonBoostedRequest())
        {
            var cached = PlayerCacheService.GetCachedFirstPage(cache);
            if (cached != null)
            {
                ViewModel = cached;
                return Page();
            }
        }

        ViewModel.SearchQuery = searchQuery;
        ViewModel.Position = position;
        ViewModel.Era = eraDecade;
        ViewModel.SortBy = sortBy;

        // Get all available first letters (cached)
        ViewModel.AvailableLetters = (await cache.GetOrCreateAsync("player_letters", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var letters = await context.People
                .Where(p => p.NameLast != null && p.NameLast.Length > 0)
                .Select(p => p.NameLast!.Substring(0, 1).ToUpper())
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
            return letters
                .Where(l => l.Length == 1 && char.IsLetter(l[0]))
                .Select(l => l[0])
                .ToList();
        }))!;

        // Default to 'A' if no letter specified; a name search spans all letters
        var currentLetter = letter?.ToUpper().FirstOrDefault() ?? 'A';
        ViewModel.CurrentLetter = searchQuery == null ? currentLetter.ToString() : null;

        // Get Hall of Fame player IDs for highlighting (cached)
        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        // Name search spans all players; otherwise browse by last-name letter
        var query = searchQuery != null
            ? context.People.Where(p =>
                ((p.NameFirst ?? "") + " " + (p.NameLast ?? "")).ToLower().Contains(searchQuery.ToLower()))
            : context.People.Where(p =>
                p.NameLast != null && p.NameLast.ToUpper().StartsWith(currentLetter.ToString()));

        if (position != null)
        {
            query = position == "OF"
                ? query.Where(p => p.Fieldings.Any(f =>
                    f.Pos == "OF" || f.Pos == "LF" || f.Pos == "CF" || f.Pos == "RF"))
                : query.Where(p => p.Fieldings.Any(f => f.Pos == position));
        }

        if (eraDecade.HasValue)
        {
            // Active at any point during the decade
            var eraStart = new DateOnly(eraDecade.Value, 1, 1);
            var eraEnd = new DateOnly(eraDecade.Value + 9, 12, 31);
            query = query.Where(p => p.Debut != null && p.FinalGame != null &&
                                     p.Debut <= eraEnd && p.FinalGame >= eraStart);
        }

        query = sortBy switch
        {
            "hr" => query.OrderByDescending(p => p.Battings.Sum(b => (int?)b.Hr) ?? 0)
                .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
            "hits" => query.OrderByDescending(p => p.Battings.Sum(b => (int?)b.H) ?? 0)
                .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
            "games" => query.OrderByDescending(p =>
                    (p.Battings.Sum(b => (int?)b.G) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0))
                .ThenBy(p => p.NameLast).ThenBy(p => p.NameFirst),
            _ => query.OrderBy(p => p.NameLast).ThenBy(p => p.NameFirst)
        };

        // Get total count for pagination
        ViewModel.TotalPlayers = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalPlayers / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));
        ViewModel.PageSize = PageSize;

        // Get players for current page — project only needed columns
        var players = await query
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst,
                p.NameLast,
                p.BirthYear,
                DebutYear = p.Debut,
                FinalYear = p.FinalGame,
                TotalGames = (p.Battings.Sum(b => (int?)b.G) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0),
                TotalHits = p.Battings.Sum(b => (int?)b.H) ?? 0,
                TotalHR = p.Battings.Sum(b => (int?)b.Hr) ?? 0,
                LastTeam = p.Battings.OrderByDescending(b => b.YearId).Select(b => b.TeamId).FirstOrDefault()
                           ?? p.Pitchings.OrderByDescending(pi => pi.YearId).Select(pi => pi.TeamId).FirstOrDefault()
            })
            .ToListAsync();

        ViewModel.Players = players.Select(p => new PlayerSummary
        {
            PlayerId = p.PlayerId,
            FirstName = p.NameFirst,
            LastName = p.NameLast,
            FullName = $"{p.NameFirst} {p.NameLast}".Trim(),
            BirthYear = p.BirthYear,
            DebutYear = p.DebutYear?.Year.ToString(),
            FinalYear = p.FinalYear?.Year.ToString(),
            IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId),
            TotalGames = p.TotalGames > 0 ? p.TotalGames : null,
            TotalHits = p.TotalHits > 0 ? p.TotalHits : null,
            TotalHomeRuns = p.TotalHR > 0 ? p.TotalHR : null,
            LastTeamId = p.LastTeam
        }).ToList();

        // Return partial view for HTMX requests
        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_PlayersContent", ViewModel);
        }

        return Page();
    }
}
