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
public class BattingModel(ILeaderboardQueryService leaderboardService, BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan FilterCacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 100;

    public LeaderboardViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string stat = "hr",
        int? fromYear = null,
        int? toYear = null,
        string? league = null,
        int minAb = 0,
        bool singleSeason = false,
        [FromQuery] int page = 1)
    {
        ViewModel.Type = LeaderboardType.Batting;
        ViewModel.StatColumn = stat;
        
        // Get stat definition from catalog
        var statDef = LeaderboardStatCatalog.GetBattingStat(stat);
        ViewModel.StatLabel = statDef?.Label ?? LeaderboardStats.BattingStats.GetValueOrDefault(stat, "Home Runs");
        ViewModel.Title = $"Batting Leaders - {ViewModel.StatLabel}";
        ViewModel.FromYear = fromYear;
        ViewModel.ToYear = toYear;
        ViewModel.League = league;
        ViewModel.MinimumAtBats = minAb;
        ViewModel.SingleSeason = singleSeason;
        ViewModel.CurrentPage = page;
        ViewModel.AvailableStats = LeaderboardStats.BattingStats;

        // Get available years (cached)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("batting_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = FilterCacheDuration;
            return await context.Batting
                .Select(b => (int)b.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Get available leagues (cached)
        ViewModel.AvailableLeagues = (await cache.GetOrCreateAsync("batting_leagues", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = FilterCacheDuration;
            return await context.Batting
                .Select(b => b.LgId)
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
            Qualified: minAb == 0, // If explicit minAb is 0, use qualification; otherwise override
            MinAtBats: minAb > 0 ? minAb : null,
            MinInningsPitched: null,
            Page: page,
            PageSize: PageSize);

        // Call service
        var result = await leaderboardService.GetBattingLeadersAsync(request);

        // Map to view model
        ViewModel.TotalEntries = result.TotalCount;
        ViewModel.TotalPages = result.TotalPages;
        ViewModel.CurrentPage = result.Page;

        ViewModel.BattingLeaders = result.Rows.Select(r => new BattingLeaderEntry
        {
            Rank = r.Rank,
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            Year = r.YearId,
            TeamId = r.TeamId,
            TeamName = r.TeamName,
            IsInHallOfFame = r.IsHallOfFamer,
            Games = r.G,
            AtBats = r.AB,
            Runs = r.R,
            Hits = r.H,
            Doubles = r.Doubles,
            Triples = r.Triples,
            HomeRuns = r.HR,
            Rbi = r.RBI,
            StolenBases = r.SB,
            Walks = r.BB,
            Strikeouts = 0 // Not in service response
        }).ToList();

        // Return appropriate view
        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_BattingLeaders", ViewModel);
        }

        return Page();
    }
}
