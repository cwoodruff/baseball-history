using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages;

public class IndexModel(BaseballDbContext context) : PageModel
{
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
        // Get database stats
        TotalPlayers = await context.People.CountAsync();
        TotalFranchises = await context.TeamsFranchises.CountAsync();
        TotalTeams = await context.Teams.Select(t => t.TeamId).Distinct().CountAsync();
        TotalSeasons = await context.Teams.Select(t => t.YearId).Distinct().CountAsync();
        HallOfFamers = await context.HallOfFame.Where(h => h.Inducted == "Y").Select(h => h.PlayerId).Distinct()
            .CountAsync();

        var years = await context.Teams.Select(t => (int)t.YearId).Distinct().ToListAsync();
        if (years.Any())
        {
            FirstYear = years.Min();
            LastYear = years.Max();
        }

        // Get recent HOF inductees
        var recentHof = await context.HallOfFame
            .Include(h => h.Player)
            .Where(h => h.Inducted == "Y")
            .OrderByDescending(h => h.Yearid)
            .Take(6)
            .Select(h => new { h.PlayerId, h.Player, h.Yearid })
            .ToListAsync();

        RecentHofInductees = recentHof.Select(h => new PlayerSummary
        {
            PlayerId = h.PlayerId,
            FirstName = h.Player?.NameFirst,
            LastName = h.Player?.NameLast,
            FullName = $"{h.Player?.NameFirst} {h.Player?.NameLast}".Trim(),
            DebutYear = h.Player?.Debut?.Year.ToString(),
            FinalYear = h.Player?.FinalGame?.Year.ToString(),
            IsInHallOfFame = true
        }).ToList();

        // Get career HR leaders (top 5)
        var hrData = await context.Batting
            .GroupBy(b => b.PlayerId)
            .Select(g => new { PlayerId = g.Key, HR = g.Sum(b => b.Hr ?? 0) })
            .OrderByDescending(x => x.HR)
            .Take(5)
            .ToListAsync();

        var hrPlayerIds = hrData.Select(h => h.PlayerId).ToList();
        var hrPlayers = await context.People
            .Where(p => hrPlayerIds.Contains(p.PlayerId))
            .ToDictionaryAsync(p => p.PlayerId, p => $"{p.NameFirst} {p.NameLast}");

        var hofSet = await context.HallOfFame
            .Where(h => h.Inducted == "Y" && hrPlayerIds.Contains(h.PlayerId))
            .Select(h => h.PlayerId)
            .ToHashSetAsync();

        CareerHrLeaders = hrData.Select((h, i) => new BattingLeaderEntry
        {
            Rank = i + 1,
            PlayerId = h.PlayerId,
            PlayerName = hrPlayers.GetValueOrDefault(h.PlayerId, h.PlayerId),
            HomeRuns = h.HR,
            IsInHallOfFame = hofSet.Contains(h.PlayerId)
        }).ToList();

        // Get career wins leaders (top 5)
        var winsData = await context.Pitching
            .GroupBy(p => p.PlayerId)
            .Select(g => new { PlayerId = g.Key, W = g.Sum(p => p.W ?? 0) })
            .OrderByDescending(x => x.W)
            .Take(5)
            .ToListAsync();

        var winsPlayerIds = winsData.Select(w => w.PlayerId).ToList();
        var winsPlayers = await context.People
            .Where(p => winsPlayerIds.Contains(p.PlayerId))
            .ToDictionaryAsync(p => p.PlayerId, p => $"{p.NameFirst} {p.NameLast}");

        var winsHofSet = await context.HallOfFame
            .Where(h => h.Inducted == "Y" && winsPlayerIds.Contains(h.PlayerId))
            .Select(h => h.PlayerId)
            .ToHashSetAsync();

        CareerWinsLeaders = winsData.Select((w, i) => new PitchingLeaderEntry
        {
            Rank = i + 1,
            PlayerId = w.PlayerId,
            PlayerName = winsPlayers.GetValueOrDefault(w.PlayerId, w.PlayerId),
            Wins = w.W,
            IsInHallOfFame = winsHofSet.Contains(w.PlayerId)
        }).ToList();
    }
}