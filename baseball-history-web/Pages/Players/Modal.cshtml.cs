using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Players;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
public class ModalModel(BaseballDbContext context) : PageModel
{
    public PlayerDetailViewModel? Player { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var person = await context.People
            .FirstOrDefaultAsync(p => p.PlayerId == id);

        if (person == null)
        {
            return NotFound();
        }

        Player = PlayerDetailViewModel.FromPeople(person);

        // Check Hall of Fame status
        var hof = await context.HallOfFame
            .Where(h => h.PlayerId == id && h.Inducted == "Y")
            .OrderBy(h => h.Yearid)
            .FirstOrDefaultAsync();

        if (hof != null)
        {
            Player.IsInHallOfFame = true;
            Player.HofInductionYear = hof.Yearid;
        }

        // Get career batting stats - aggregate in database
        var battingAgg = await context.Batting
            .Where(b => b.PlayerId == id)
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                G = g.Sum(b => b.G ?? 0),
                AB = g.Sum(b => b.Ab ?? 0),
                R = g.Sum(b => b.R ?? 0),
                H = g.Sum(b => b.H ?? 0),
                Doubles = g.Sum(b => b._2b ?? 0),
                Triples = g.Sum(b => b._3b ?? 0),
                HR = g.Sum(b => b.Hr ?? 0),
                BB = g.Sum(b => b.Bb ?? 0),
                RBI = g.Sum(b => b.Rbi ?? 0),
                SB = g.Sum(b => b.Sb ?? 0),
                SO = g.Sum(b => b.So ?? 0)
            })
            .FirstOrDefaultAsync();

        if (battingAgg is { AB: > 0 })
        {
            Player.BattingStats = new CareerBattingStats
            {
                Games = battingAgg.G,
                AtBats = battingAgg.AB,
                Runs = battingAgg.R,
                Hits = battingAgg.H,
                Doubles = battingAgg.Doubles,
                Triples = battingAgg.Triples,
                HomeRuns = battingAgg.HR,
                Walks = battingAgg.BB,
                Rbi = battingAgg.RBI,
                StolenBases = battingAgg.SB,
                Strikeouts = battingAgg.SO
            };
        }

        // Get career pitching stats - aggregate in database
        var pitchingAgg = await context.Pitching
            .Where(p => p.PlayerId == id)
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
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
            .FirstOrDefaultAsync();

        if (pitchingAgg is { G: > 0 })
        {
            Player.PitchingStats = new CareerPitchingStats
            {
                Games = pitchingAgg.G,
                GamesStarted = pitchingAgg.GS,
                Wins = pitchingAgg.W,
                Losses = pitchingAgg.L,
                Saves = pitchingAgg.SV,
                CompleteGames = pitchingAgg.CG,
                Shutouts = pitchingAgg.SHO,
                InningsPitched = pitchingAgg.IPOuts / 3.0,
                Hits = pitchingAgg.H,
                EarnedRuns = pitchingAgg.ER,
                HomeRuns = pitchingAgg.HR,
                Walks = pitchingAgg.BB,
                Strikeouts = pitchingAgg.SO
            };
        }

        // Get season-by-season batting records
        Player.BattingSeasons = await context.Batting
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

        // Get season-by-season pitching records
        Player.PitchingSeasons = await context.Pitching
            .Where(p => p.PlayerId == id)
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => p.Stint)
            .Select(p => new SeasonPitchingRecord
            {
                Year = p.YearId,
                TeamId = p.TeamId ?? "",
                TeamName = p.Team != null ? p.Team.Name : null,
                LgId = p.LgId ?? "",
                Games = p.G ?? 0,
                GamesStarted = p.Gs ?? 0,
                Wins = p.W ?? 0,
                Losses = p.L ?? 0,
                Saves = p.Sv ?? 0,
                InningsPitched = (p.Ipouts ?? 0) / 3.0,
                Hits = p.H ?? 0,
                EarnedRuns = p.Er ?? 0,
                Strikeouts = p.So ?? 0,
                Walks = p.Bb ?? 0
            })
            .ToListAsync();

        // Get teams played for
        var teamGroups = await context.Batting
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
        var pitchingTeams = await context.Pitching
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
        var teamNames = await context.Teams
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
        Player.Awards = await context.AwardsPlayers
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
        var allStarData = await context.AllstarFull
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