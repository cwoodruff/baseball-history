using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Stats;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class PitchingModel(ILeaderboardQueryService leaderboardService, BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan FilterCacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 100;

    public LeaderboardViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string stat = "w",
        int? fromYear = null,
        int? toYear = null,
        string? league = null,
        int? minIp = null,
        bool singleSeason = false,
        [FromQuery] int page = 1)
    {
        ViewModel.Type = LeaderboardType.Pitching;
        ViewModel.StatColumn = stat;
        
        // Get stat definition from catalog
        var statDef = LeaderboardStatCatalog.GetPitchingStat(stat);
        ViewModel.StatLabel = statDef?.Label ?? LeaderboardStats.PitchingStats.GetValueOrDefault(stat, "Wins");
        ViewModel.Title = $"Pitching Leaders - {ViewModel.StatLabel}";
        ViewModel.FromYear = fromYear;
        ViewModel.ToYear = toYear;
        ViewModel.League = league;
        ViewModel.SingleSeason = singleSeason;
        ViewModel.CurrentPage = page;
        ViewModel.AvailableStats = LeaderboardStats.PitchingStats;
        
        // Default to "Qualified" for rate stats if no explicit minimum provided
        bool isRateStat = statDef?.IsRateStat ?? false;
        int effectiveMinIp = minIp ?? (isRateStat ? -1 : 0);
        
        ViewModel.MinimumInningsPitched = effectiveMinIp;
        ViewModel.IsQualified = effectiveMinIp == -1;

        // Get available years (cached)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("pitching_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = FilterCacheDuration;
            return await context.Pitching
                .Select(p => (int)p.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Get available leagues (cached)
        ViewModel.AvailableLeagues = (await cache.GetOrCreateAsync("pitching_leagues", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = FilterCacheDuration;
            return await context.Pitching
                .Select(p => p.LgId!)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }))!;

        // Build request
        var request = new LeaderboardRequest(
            Stat: stat,
            FromYear: fromYear,
            ToYear: toYear,
            League: league,
            SingleSeason: singleSeason,
            Qualified: effectiveMinIp == -1, // -1 means "Qualified" (season-relative)
            MinAtBats: null,
            MinInningsPitched: effectiveMinIp > 0 ? effectiveMinIp : null,
            Page: page,
            PageSize: PageSize);

        // Call service
        var result = await leaderboardService.GetPitchingLeadersAsync(request);

        // Map to view model
        ViewModel.TotalEntries = result.TotalCount;
        ViewModel.TotalPages = result.TotalPages;
        ViewModel.CurrentPage = result.Page;

        ViewModel.PitchingLeaders = result.Rows.Select(r => new PitchingLeaderEntry
        {
            Rank = r.Rank,
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            Year = r.YearId,
            TeamId = r.TeamId,
            TeamName = r.TeamName,
            IsInHallOfFame = r.IsHallOfFamer,
            Games = r.G,
            GamesStarted = r.GS,
            Wins = r.W,
            Losses = r.L,
            Saves = r.SV,
            CompleteGames = r.CG,
            Shutouts = r.SHO,
            InningsPitched = (double)r.IP,
            Hits = r.H,
            HomeRuns = r.HR,
            Walks = r.BB,
            Strikeouts = r.SO
        }).ToList();

        // Return appropriate view
        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_PitchingLeaders", ViewModel);
        }

        return Page();
    }
}
