using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Stats;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class BattingModel(BaseballDbContext context, IMemoryCache cache) : PageModel
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
        ViewModel.StatLabel = LeaderboardStats.BattingStats.GetValueOrDefault(stat, "Home Runs");
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

        // Get Hall of Fame player IDs (cached)
        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = FilterCacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        // Build query
        var query = context.Batting
            .AsQueryable();

        // Apply year filter
        if (fromYear.HasValue)
            query = query.Where(b => b.YearId >= fromYear.Value);
        if (toYear.HasValue)
            query = query.Where(b => b.YearId <= toYear.Value);

        // Apply league filter
        if (!string.IsNullOrEmpty(league))
            query = query.Where(b => b.LgId == league);

        if (singleSeason)
        {
            // Single season leaders
            var seasonData = await query
                .Where(b => b.Ab >= minAb)
                .Select(b => new
                {
                    b.PlayerId,
                    PlayerName = (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                    b.YearId,
                    b.TeamId,
                    TeamName = b.Team.Name,
                    G = b.G ?? 0,
                    AB = b.Ab ?? 0,
                    R = b.R ?? 0,
                    H = b.H ?? 0,
                    Doubles = b._2b ?? 0,
                    Triples = b._3b ?? 0,
                    HR = b.Hr ?? 0,
                    RBI = b.Rbi ?? 0,
                    SB = b.Sb ?? 0,
                    BB = b.Bb ?? 0
                })
                .ToListAsync();

            var leaders = seasonData
                .Select(b => new BattingLeaderEntry
                {
                    PlayerId = b.PlayerId,
                    PlayerName = b.PlayerName,
                    Year = b.YearId,
                    TeamId = b.TeamId,
                    TeamName = b.TeamName,
                    Games = b.G,
                    AtBats = b.AB,
                    Runs = b.R,
                    Hits = b.H,
                    Doubles = b.Doubles,
                    Triples = b.Triples,
                    HomeRuns = b.HR,
                    Rbi = b.RBI,
                    StolenBases = b.SB,
                    Walks = b.BB,
                    IsInHallOfFame = hofPlayerIds.Contains(b.PlayerId)
                })
                .OrderByDescending(e => e.GetStatValue(stat))
                .ToList();

            ViewModel.TotalEntries = leaders.Count;
            ViewModel.TotalPages = (int)Math.Ceiling((double)leaders.Count / PageSize);
            ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

            ViewModel.BattingLeaders = leaders
                .Skip((ViewModel.CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select((e, i) =>
                {
                    e.Rank = (ViewModel.CurrentPage - 1) * PageSize + i + 1;
                    return e;
                })
                .ToList();
        }
        else
        {
            // Career leaders
            var careerData = await query
                .GroupBy(b => b.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    G = g.Sum(b => b.G ?? 0),
                    AB = g.Sum(b => b.Ab ?? 0),
                    R = g.Sum(b => b.R ?? 0),
                    H = g.Sum(b => b.H ?? 0),
                    Doubles = g.Sum(b => b._2b ?? 0),
                    Triples = g.Sum(b => b._3b ?? 0),
                    HR = g.Sum(b => b.Hr ?? 0),
                    RBI = g.Sum(b => b.Rbi ?? 0),
                    SB = g.Sum(b => b.Sb ?? 0),
                    BB = g.Sum(b => b.Bb ?? 0)
                })
                .Where(x => x.AB >= minAb)
                .ToListAsync();

            // Get player names
            var playerIds = careerData.Select(c => c.PlayerId).ToList();
            var players = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => (p.NameFirst ?? "") + " " + (p.NameLast ?? ""));

            var leaders = careerData
                .Select(c => new BattingLeaderEntry
                {
                    PlayerId = c.PlayerId,
                    PlayerName = players.GetValueOrDefault(c.PlayerId, c.PlayerId),
                    Games = c.G,
                    AtBats = c.AB,
                    Runs = c.R,
                    Hits = c.H,
                    Doubles = c.Doubles,
                    Triples = c.Triples,
                    HomeRuns = c.HR,
                    Rbi = c.RBI,
                    StolenBases = c.SB,
                    Walks = c.BB,
                    IsInHallOfFame = hofPlayerIds.Contains(c.PlayerId)
                })
                .OrderByDescending(e => e.GetStatValue(stat))
                .ToList();

            ViewModel.TotalEntries = leaders.Count;
            ViewModel.TotalPages = (int)Math.Ceiling((double)leaders.Count / PageSize);
            ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

            ViewModel.BattingLeaders = leaders
                .Skip((ViewModel.CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Select((e, i) =>
                {
                    e.Rank = (ViewModel.CurrentPage - 1) * PageSize + i + 1;
                    return e;
                })
                .ToList();
        }

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_BattingLeaders", ViewModel);
        }

        return Page();
    }
}