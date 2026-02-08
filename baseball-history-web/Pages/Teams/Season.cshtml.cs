using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

public class SeasonModel : PageModel
{
    private readonly BaseballDbContext _context;

    public SeasonModel(BaseballDbContext context)
    {
        _context = context;
    }

    public TeamSeasonViewModel? Team { get; set; }

    public async Task<IActionResult> OnGetAsync(string teamId, string lgId, short year)
    {
        if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(lgId))
        {
            return NotFound();
        }

        var team = await _context.Teams
            .Include(t => t.Franchise)
            .FirstOrDefaultAsync(t => t.TeamId == teamId && t.LgId == lgId && t.YearId == year);

        if (team == null)
        {
            return NotFound();
        }

        Team = TeamSeasonViewModel.FromTeam(team);

        // Get Hall of Fame player IDs
        var hofPlayerIds = await _context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSetAsync();

        // Get batting roster
        var batters = await _context.Batting
            .Include(b => b.Player)
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
                BattingAverage = (b.Ab ?? 0) > 0 ? (double)(b.H ?? 0) / (b.Ab ?? 0) : 0
            })
            .ToListAsync();

        // Parse RBI from string
        var rbiData = await _context.Batting
            .Where(b => b.TeamId == teamId && b.LgId == lgId && b.YearId == year && b.Rbi != null)
            .Select(b => new { b.PlayerId, Rbi = b.Rbi })
            .ToListAsync();

        foreach (var batter in batters)
        {
            var rbi = rbiData.FirstOrDefault(r => r.PlayerId == batter.PlayerId);
            if (rbi != null)
            {
                batter.Rbi = rbi.Rbi ?? 0;
            }

            batter.IsInHallOfFame = hofPlayerIds.Contains(batter.PlayerId);
        }

        Team.Batters = batters;

        // Get pitching roster
        var pitchers = await _context.Pitching
            .Include(p => p.Player)
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

        // Get managers
        var managers = await _context.Managers
            .Include(m => m.Player)
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

        // Get available years for this franchise
        if (!string.IsNullOrEmpty(team.FranchId))
        {
            Team.AvailableYears = await _context.Teams
                .Where(t => t.FranchId == team.FranchId)
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