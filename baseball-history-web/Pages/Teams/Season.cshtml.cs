using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class SeasonModel(BaseballDbContext context) : PageModel
{
    public TeamSeasonViewModel? Team { get; set; }

    public async Task<IActionResult> OnGetAsync(string teamId, string lgId, short year)
    {
        if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(lgId))
        {
            return NotFound();
        }

        var team = await context.Teams
            .Where(t => t.TeamId == teamId && t.LgId == lgId && t.YearId == year)
            .Select(t => new TeamSeasonRecord(
                t.TeamId,
                t.Name,
                t.YearId,
                t.LgId,
                t.DivId,
                t.FranchId,
                t.Franchise != null ? t.Franchise.FranchName : null,
                t.W ?? 0,
                t.L ?? 0,
                t.Rank,
                t.DivWin == "Y",
                t.Wcwin == "Y",
                t.LgWin == "Y",
                t.Wswin == "Y",
                t.Park,
                t.Attendance,
                t.R,
                t.Ab,
                t.H,
                t._2b,
                t._3b,
                t.Hr,
                t.Bb,
                t.So,
                t.Sb,
                t.Ra,
                t.Er,
                t.Era,
                t.Cg,
                t.Sho,
                t.Sv,
                t.Ha,
                t.Hra,
                t.Bba,
                t.Soa))
            .FirstOrDefaultAsync();

        if (team == null)
        {
            return NotFound();
        }

        Team = TeamSeasonViewModel.FromRecord(team);

        // Get Hall of Fame player IDs
        var hofPlayerIds = await context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSetAsync();

        var batters = await context.Batting
            .Where(b => b.TeamId == teamId && b.LgId == lgId && b.YearId == year)
            .OrderByDescending(b => b.Ab ?? 0)
            .Select(b => new RosterPlayer
            {
                PlayerId = b.PlayerId,
                FullName = (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                Games = b.G ?? 0,
                AtBats = b.Ab ?? 0,
                Hits = b.H ?? 0,
                HomeRuns = b.Hr ?? 0,
                Rbi = b.Rbi ?? 0,
                BattingAverage = (b.Ab ?? 0) > 0 ? (double)(b.H ?? 0) / (b.Ab ?? 0) : 0
            })
            .ToListAsync();

        foreach (var batter in batters)
        {
            batter.IsInHallOfFame = hofPlayerIds.Contains(batter.PlayerId);
        }

        Team.Batters = batters;

        var pitchers = await context.Pitching
            .Where(p => p.TeamId == teamId && p.LgId == lgId && p.YearId == year)
            .OrderByDescending(p => p.Ipouts ?? 0)
            .Select(p => new RosterPlayer
            {
                PlayerId = p.PlayerId,
                FullName = (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                Games = p.G ?? 0,
                Wins = p.W ?? 0,
                Losses = p.L ?? 0,
                Saves = p.Sv ?? 0,
                Strikeouts = p.So ?? 0,
                Era = (p.Ipouts ?? 0) > 0 ? ((p.Er ?? 0) * 27.0) / (p.Ipouts ?? 0) : 0
            })
            .ToListAsync();

        foreach (var pitcher in pitchers)
        {
            pitcher.IsInHallOfFame = hofPlayerIds.Contains(pitcher.PlayerId);
        }

        Team.Pitchers = pitchers;

        var managers = await context.Managers
            .Where(m => m.TeamId == teamId && m.LgId == lgId && m.YearId == year)
            .OrderBy(m => m.Inseason)
            .Select(m => new ManagerInfo
            {
                PlayerId = m.PlayerId,
                FullName = (m.Player.NameFirst ?? "") + " " + (m.Player.NameLast ?? ""),
                Games = m.G ?? 0,
                Wins = m.W ?? 0,
                Losses = m.L ?? 0,
                Order = m.Inseason
            })
            .ToListAsync();

        foreach (var manager in managers)
        {
            manager.IsInHallOfFame = hofPlayerIds.Contains(manager.PlayerId);
        }

        Team.Managers = managers;

        if (!string.IsNullOrEmpty(Team.FranchiseId))
        {
            Team.AvailableYears = await context.Teams
                .Where(t => t.FranchId == Team.FranchiseId)
                .Select(t => t.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_TeamSeason", Team);
        }

        return Page();
    }
}
