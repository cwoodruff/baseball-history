using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages;

[ResponseCache(Duration = 3600)]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    // Database stats
    public int TotalPlayers { get; set; }
    public int TotalTeams { get; set; }
    public int TotalFranchises { get; set; }
    public int TotalSeasons { get; set; }
    public int HallOfFamers { get; set; }
    public int FirstYear { get; set; }
    public int LastYear { get; set; }

    // Recent Hall of Fame inductees
    public List<PlayerSummary> RecentHofInductees { get; set; } = new();

    // Top career leaders
    public List<BattingLeaderEntry> CareerHrLeaders { get; set; } = new();
    public List<PitchingLeaderEntry> CareerWinsLeaders { get; set; } = new();

    public async Task OnGetAsync()
    {
        var homeData = await cache.GetOrCreateAsync("home_page_data", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await LoadHomePageDataAsync();
        });

        if (homeData != null)
        {
            TotalPlayers = homeData.TotalPlayers;
            TotalFranchises = homeData.TotalFranchises;
            TotalTeams = homeData.TotalTeams;
            TotalSeasons = homeData.TotalSeasons;
            HallOfFamers = homeData.HallOfFamers;
            FirstYear = homeData.FirstYear;
            LastYear = homeData.LastYear;
            RecentHofInductees = homeData.RecentHofInductees;
            CareerHrLeaders = homeData.CareerHrLeaders;
            CareerWinsLeaders = homeData.CareerWinsLeaders;
        }
    }

    private async Task<HomePageData> LoadHomePageDataAsync()
    {
        var data = new HomePageData();

        // Get database stats
        data.TotalPlayers = await context.People.CountAsync();
        data.TotalFranchises = await context.TeamsFranchises.CountAsync();
        data.TotalTeams = await context.Teams.Select(t => t.TeamId).Distinct().CountAsync();
        data.TotalSeasons = await context.Teams.Select(t => t.YearId).Distinct().CountAsync();
        data.HallOfFamers = await context.HallOfFame.Where(h => h.Inducted == "Y").Select(h => h.PlayerId).Distinct()
            .CountAsync();

        var yearRange = await context.Teams
            .GroupBy(_ => 1)
            .Select(g => new { Min = g.Min(t => (int)t.YearId), Max = g.Max(t => (int)t.YearId) })
            .OrderBy(_ => 1)
            .FirstOrDefaultAsync();

        if (yearRange != null)
        {
            data.FirstYear = yearRange.Min;
            data.LastYear = yearRange.Max;
        }

        // Get HOF set once for both leader sections
        var allLeaderPlayerIds = new List<string>();

        // Get career HR leaders (top 5)
        var hrData = await context.Batting
            .GroupBy(b => b.PlayerId)
            .Select(g => new { PlayerId = g.Key, HR = g.Sum(b => b.Hr ?? 0) })
            .OrderByDescending(x => x.HR)
            .Take(5)
            .ToListAsync();

        allLeaderPlayerIds.AddRange(hrData.Select(h => h.PlayerId));

        // Get career wins leaders (top 5)
        var winsData = await context.Pitching
            .GroupBy(p => p.PlayerId)
            .Select(g => new { PlayerId = g.Key, W = g.Sum(p => p.W ?? 0) })
            .OrderByDescending(x => x.W)
            .Take(5)
            .ToListAsync();

        allLeaderPlayerIds.AddRange(winsData.Select(w => w.PlayerId));

        // Single query for all leader player names
        var playerNames = await context.People
            .Where(p => allLeaderPlayerIds.Contains(p.PlayerId))
            .ToDictionaryAsync(p => p.PlayerId, p => $"{p.NameFirst} {p.NameLast}");

        // Single HOF query for all leaders
        var hofSet = await context.HallOfFame
            .Where(h => h.Inducted == "Y" && allLeaderPlayerIds.Contains(h.PlayerId))
            .Select(h => h.PlayerId)
            .ToHashSetAsync();

        data.CareerHrLeaders = hrData.Select((h, i) => new BattingLeaderEntry
        {
            Rank = i + 1,
            PlayerId = h.PlayerId,
            PlayerName = playerNames.GetValueOrDefault(h.PlayerId, h.PlayerId),
            HomeRuns = h.HR,
            IsInHallOfFame = hofSet.Contains(h.PlayerId)
        }).ToList();

        data.CareerWinsLeaders = winsData.Select((w, i) => new PitchingLeaderEntry
        {
            Rank = i + 1,
            PlayerId = w.PlayerId,
            PlayerName = playerNames.GetValueOrDefault(w.PlayerId, w.PlayerId),
            Wins = w.W,
            IsInHallOfFame = hofSet.Contains(w.PlayerId)
        }).ToList();

        // Get recent HOF inductees
        var recentHof = await context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .OrderByDescending(h => h.Yearid)
            .Take(6)
            .Select(h => new { h.PlayerId, h.Player.NameFirst, h.Player.NameLast, h.Player.Debut, h.Player.FinalGame, h.Yearid })
            .ToListAsync();

        data.RecentHofInductees = recentHof.Select(h => new PlayerSummary
        {
            PlayerId = h.PlayerId,
            FirstName = h.NameFirst,
            LastName = h.NameLast,
            FullName = $"{h.NameFirst} {h.NameLast}".Trim(),
            DebutYear = h.Debut?.Year.ToString(),
            FinalYear = h.FinalGame?.Year.ToString(),
            IsInHallOfFame = true
        }).ToList();

        return data;
    }

    private sealed class HomePageData
    {
        public int TotalPlayers { get; set; }
        public int TotalTeams { get; set; }
        public int TotalFranchises { get; set; }
        public int TotalSeasons { get; set; }
        public int HallOfFamers { get; set; }
        public int FirstYear { get; set; }
        public int LastYear { get; set; }
        public List<PlayerSummary> RecentHofInductees { get; set; } = new();
        public List<BattingLeaderEntry> CareerHrLeaders { get; set; } = new();
        public List<PitchingLeaderEntry> CareerWinsLeaders { get; set; } = new();
    }
}