using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Stats;

[ResponseCache(Duration = 3600, VaryByQueryKeys = ["stat", "fromYear", "toYear", "league", "minIp", "singleSeason", "page"])]
public class PitchingModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan FilterCacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 100;

    public LeaderboardViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string stat = "w",
        int? fromYear = null,
        int? toYear = null,
        string? league = null,
        int minIp = 0,
        bool singleSeason = false,
        int page = 1)
    {
        ViewModel.Type = LeaderboardType.Pitching;
        ViewModel.StatColumn = stat;
        ViewModel.StatLabel = LeaderboardStats.PitchingStats.GetValueOrDefault(stat, "Wins");
        ViewModel.Title = $"Pitching Leaders - {ViewModel.StatLabel}";
        ViewModel.FromYear = fromYear;
        ViewModel.ToYear = toYear;
        ViewModel.League = league;
        ViewModel.MinimumInningsPitched = minIp;
        ViewModel.SingleSeason = singleSeason;
        ViewModel.CurrentPage = page;
        ViewModel.AvailableStats = LeaderboardStats.PitchingStats;

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
                .Where(p => p.LgId != null)
                .Select(p => p.LgId!)
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
        var query = context.Pitching
            .AsQueryable();

        // Apply year filter
        if (fromYear.HasValue)
            query = query.Where(p => p.YearId >= fromYear.Value);
        if (toYear.HasValue)
            query = query.Where(p => p.YearId <= toYear.Value);

        // Apply league filter
        if (!string.IsNullOrEmpty(league))
            query = query.Where(p => p.LgId == league);

        // Convert minIp to outs (IP * 3)
        var minOuts = minIp * 3;

        if (singleSeason)
        {
            // Single season leaders
            var seasonData = await query
                .Where(p => (p.Ipouts ?? 0) >= minOuts)
                .Select(p => new
                {
                    p.PlayerId,
                    PlayerName = (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                    p.YearId,
                    p.TeamId,
                    TeamName = p.Team.Name,
                    G = p.G ?? 0,
                    GS = p.Gs ?? 0,
                    W = p.W ?? 0,
                    L = p.L ?? 0,
                    SV = p.Sv ?? 0,
                    CG = p.Cg ?? 0,
                    SHO = p.Sho ?? 0,
                    IPOuts = p.Ipouts ?? 0,
                    H = p.H ?? 0,
                    ER = p.Er ?? 0,
                    HR = p.Hr ?? 0,
                    BB = p.Bb ?? 0,
                    SO = p.So ?? 0
                })
                .ToListAsync();

            var leaders = seasonData
                .Select(p => new PitchingLeaderEntry
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    Year = p.YearId,
                    TeamId = p.TeamId,
                    TeamName = p.TeamName,
                    Games = p.G,
                    GamesStarted = p.GS,
                    Wins = p.W,
                    Losses = p.L,
                    Saves = p.SV,
                    CompleteGames = p.CG,
                    Shutouts = p.SHO,
                    InningsPitched = p.IPOuts / 3.0,
                    Hits = p.H,
                    EarnedRuns = p.ER,
                    HomeRuns = p.HR,
                    Walks = p.BB,
                    Strikeouts = p.SO,
                    IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId)
                })
                .OrderByDescending(e => e.GetStatValue(stat))
                .ToList();

            // For ERA, sort ascending (lower is better)
            if (stat.ToLower() == "era" || stat.ToLower() == "whip")
            {
                leaders = leaders.OrderBy(e => e.GetStatValue(stat)).ToList();
            }

            ViewModel.TotalEntries = leaders.Count;
            ViewModel.TotalPages = (int)Math.Ceiling((double)leaders.Count / PageSize);
            ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

            ViewModel.PitchingLeaders = leaders
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
            // Career leaders - aggregate in database
            var careerData = await query
                .GroupBy(p => p.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    G = g.Sum(p => p.G ?? 0),
                    GS = g.Sum(p => p.Gs ?? 0),
                    W = g.Sum(p => p.W ?? 0),
                    L = g.Sum(p => p.L ?? 0),
                    SV = g.Sum(p => p.Sv ?? 0),
                    CG = g.Sum(p => p.Cg ?? 0),
                    SHO = g.Sum(p => p.Sho ?? 0),
                    IPOuts = g.Sum(p => p.Ipouts ?? 0),
                    H = g.Sum(p => p.H ?? 0),
                    ER = g.Sum(p => p.Er ?? 0),
                    HR = g.Sum(p => p.Hr ?? 0),
                    BB = g.Sum(p => p.Bb ?? 0),
                    SO = g.Sum(p => p.So ?? 0)
                })
                .Where(x => x.IPOuts >= minOuts)
                .ToListAsync();

            // Get player names
            var playerIds = careerData.Select(c => c.PlayerId).ToList();
            var players = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => (p.NameFirst ?? "") + " " + (p.NameLast ?? ""));

            var leaders = careerData
                .Select(p => new PitchingLeaderEntry
                {
                    PlayerId = p.PlayerId,
                    PlayerName = players.GetValueOrDefault(p.PlayerId, p.PlayerId),
                    Games = p.G,
                    GamesStarted = p.GS,
                    Wins = p.W,
                    Losses = p.L,
                    Saves = p.SV,
                    CompleteGames = p.CG,
                    Shutouts = p.SHO,
                    InningsPitched = p.IPOuts / 3.0,
                    Hits = p.H,
                    EarnedRuns = p.ER,
                    HomeRuns = p.HR,
                    Walks = p.BB,
                    Strikeouts = p.SO,
                    IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId)
                })
                .OrderByDescending(e => e.GetStatValue(stat))
                .ToList();

            // For ERA/WHIP, sort ascending
            if (stat.ToLower() == "era" || stat.ToLower() == "whip")
            {
                leaders = leaders.OrderBy(e => e.GetStatValue(stat)).ToList();
            }

            ViewModel.TotalEntries = leaders.Count;
            ViewModel.TotalPages = (int)Math.Ceiling((double)leaders.Count / PageSize);
            ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

            ViewModel.PitchingLeaders = leaders
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
            return Partial("_PitchingLeaders", ViewModel);
        }

        return Page();
    }
}