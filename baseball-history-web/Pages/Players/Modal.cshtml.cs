using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Players;

public class ModalModel : PageModel
{
    private readonly BaseballDbContext _context;

    public ModalModel(BaseballDbContext context)
    {
        _context = context;
    }

    public PlayerDetailViewModel? Player { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var person = await _context.People
            .FirstOrDefaultAsync(p => p.PlayerId == id);

        if (person == null)
        {
            return NotFound();
        }

        Player = PlayerDetailViewModel.FromPeople(person);

        // Check Hall of Fame status
        var hof = await _context.HallOfFame
            .Where(h => h.PlayerId == id && h.Inducted == "Y")
            .OrderBy(h => h.Yearid)
            .FirstOrDefaultAsync();

        if (hof != null)
        {
            Player.IsInHallOfFame = true;
            Player.HofInductionYear = hof.Yearid;
        }

        // Get career batting stats - fetch data first, then aggregate in memory for string fields
        var battingData = await _context.Batting
            .Where(b => b.PlayerId == id)
            .ToListAsync();

        if (battingData.Any())
        {
            var totalAtBats = battingData.Sum(b => b.Ab ?? 0);
            if (totalAtBats > 0)
            {
                Player.BattingStats = new CareerBattingStats
                {
                    Games = battingData.Sum(b => b.G ?? 0),
                    AtBats = totalAtBats,
                    Runs = battingData.Sum(b => b.R ?? 0),
                    Hits = battingData.Sum(b => b.H ?? 0),
                    Doubles = battingData.Sum(b => b._2b ?? 0),
                    Triples = battingData.Sum(b => b._3b ?? 0),
                    HomeRuns = battingData.Sum(b => b.Hr ?? 0),
                    Walks = battingData.Sum(b => b.Bb ?? 0),
                    Rbi = battingData.Sum(b => b.Rbi ?? 0),
                    StolenBases = battingData.Sum(b => b.Sb ?? 0),
                    Strikeouts = battingData.Sum(b => b.So ?? 0)
                };
            }
        }

        // Get career pitching stats
        var pitchingData = await _context.Pitching
            .Where(p => p.PlayerId == id)
            .ToListAsync();

        if (pitchingData.Any())
        {
            Player.PitchingStats = new CareerPitchingStats
            {
                Games = pitchingData.Sum(p => p.G ?? 0),
                GamesStarted = pitchingData.Sum(p => p.Gs ?? 0),
                Wins = pitchingData.Sum(p => p.W ?? 0),
                Losses = pitchingData.Sum(p => p.L ?? 0),
                Saves = pitchingData.Sum(p => p.Sv ?? 0),
                CompleteGames = pitchingData.Sum(p => p.Cg ?? 0),
                Shutouts = pitchingData.Sum(p => p.Sho ?? 0),
                Hits = pitchingData.Sum(p => p.H ?? 0),
                Walks = pitchingData.Sum(p => p.Bb ?? 0),
                Strikeouts = pitchingData.Sum(p => p.So ?? 0)
            };

            // Calculate innings pitched from outs
            var totalOuts = pitchingData.Sum(p => p.Ipouts ?? 0);
            Player.PitchingStats.InningsPitched = totalOuts / 3.0;

            // Parse earned runs
            var erSum = pitchingData.Sum(p => p.Er ?? 0);
            Player.PitchingStats.EarnedRuns = erSum;

            // Parse HR
            var hrSum = pitchingData.Sum(p => p.Hr ?? 0);
            Player.PitchingStats.HomeRuns = hrSum;
        }

        // Get season-by-season batting records
        Player.BattingSeasons = await _context.Batting
            .Where(b => b.PlayerId == id)
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => b.Stint)
            .Select(b => new SeasonBattingRecord
            {
                Year = b.YearId,
                TeamId = b.TeamId,
                TeamName = b.Team.Name,
                LgId = b.LgId,
                Games = b.G ?? 0,
                AtBats = b.Ab ?? 0,
                Runs = b.R ?? 0,
                Hits = b.H ?? 0,
                Doubles = b._2b ?? 0,
                Triples = b._3b ?? 0,
                HomeRuns = b.Hr ?? 0
            })
            .ToListAsync();

        // Get teams played for
        var teamGroups = await _context.Batting
            .Where(b => b.PlayerId == id)
            .GroupBy(b => b.TeamId)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(b => b.YearId),
                LastYear = g.Max(b => b.YearId),
                Seasons = g.Select(b => b.YearId).Distinct().Count()
            })
            .ToListAsync();

        var existingTeamIds = teamGroups.Select(t => t.TeamId).ToHashSet();
        var pitchingTeams = await _context.Pitching
            .Where(p => p.PlayerId == id && p.TeamId != null && !existingTeamIds.Contains(p.TeamId))
            .GroupBy(p => p.TeamId!)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(p => p.YearId),
                LastYear = g.Max(p => p.YearId),
                Seasons = g.Select(p => p.YearId).Distinct().Count()
            })
            .ToListAsync();

        var allTeams = teamGroups
            .Select(t => new { t.TeamId, t.FirstYear, t.LastYear, t.Seasons })
            .Concat(pitchingTeams.Select(t => new { t.TeamId, t.FirstYear, t.LastYear, t.Seasons }))
            .OrderBy(t => t.FirstYear)
            .ToList();

        // Get team names
        var teamIds = allTeams.Select(t => t.TeamId).Distinct().ToList();
        var teamNames = await _context.Teams
            .Where(t => teamIds.Contains(t.TeamId))
            .GroupBy(t => t.TeamId)
            .Select(g => new { TeamId = g.Key, Name = g.OrderByDescending(t => t.YearId).First().Name })
            .ToDictionaryAsync(t => t.TeamId, t => t.Name);

        Player.Teams = allTeams.Select(t => new TeamRecord
        {
            TeamId = t.TeamId,
            TeamName = teamNames.GetValueOrDefault(t.TeamId),
            FirstYear = t.FirstYear,
            LastYear = t.LastYear,
            Seasons = t.Seasons
        }).ToList();

        // Get awards
        Player.Awards = await _context.AwardsPlayers
            .Where(a => a.PlayerId == id)
            .OrderByDescending(a => a.YearId)
            .Select(a => new AwardRecord
            {
                Year = (short)a.YearId,
                AwardId = a.AwardId,
                LgId = a.LgId,
                Notes = a.Notes
            })
            .ToListAsync();

        // Get All-Star appearances
        var allStarData = await _context.AllstarFull
            .Where(a => a.PlayerId == id)
            .OrderByDescending(a => a.YearId)
            .Select(a => new { a.YearId, a.LgId, a.TeamId, a.GameNum })
            .ToListAsync();

        Player.AllStarAppearances = allStarData.Select(a => new AllStarRecord
        {
            Year = a.YearId,
            LgId = a.LgId,
            TeamId = a.TeamId,
            GameNum = int.TryParse(a.GameNum, out var gameNum) ? gameNum : 0
        }).ToList();

        return Partial("_PlayerModal", Player);
    }
}