using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Services;

/// <summary>
/// Loads the full player detail view model (bio, career stats, seasons, teams,
/// awards). Shared by the player modal and the full player page.
/// </summary>
public class PlayerDetailService(BaseballDbContext context)
{
    public async Task<PlayerDetailViewModel?> GetPlayerDetailAsync(string id)
    {
        var person = await context.People
            .FirstOrDefaultAsync(p => p.PlayerId == id);

        if (person == null)
        {
            return null;
        }

        var player = PlayerDetailViewModel.FromPeople(person);

        // Check Hall of Fame status
        var hof = await context.HallOfFame
            .Where(h => h.PlayerId == id && h.Inducted == "Y")
            .OrderBy(h => h.Yearid)
            .FirstOrDefaultAsync();

        if (hof != null)
        {
            player.IsInHallOfFame = true;
            player.HofInductionYear = hof.Yearid;
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
            player.BattingStats = new CareerBattingStats
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
            player.PitchingStats = new CareerPitchingStats
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

        // League-indexed career context and advanced season rows from the
        // shared query layer (docs/qualification_and_league_index.sql)
        var careerIndex = await context.CareerBattingSummaries
            .Where(c => c.PlayerId == id)
            .FirstOrDefaultAsync();

        if (careerIndex != null)
        {
            player.CareerOpsIndex = (int?)careerIndex.OpsIndex;
            player.CareerQualified = careerIndex.Qualified;
            player.CareerPctOfThreshold = (double?)careerIndex.PctOfThreshold;
        }

        player.AdvancedBattingSeasons = await context.PlayerSeasonRates
            .Where(r => r.PlayerId == id)
            .OrderByDescending(r => r.YearId)
            .Select(r => new AdvancedBattingSeason
            {
                Year = r.YearId,
                LgId = r.LgId,
                Pa = r.Pa ?? 0,
                Iso = r.Iso,
                Babip = r.Babip,
                BbPct = r.BbPct,
                KPct = r.KPct,
                OpsIndex = r.OpsIndex,
                HrPer162 = r.HrPer162,
                Qualified = r.Qualified ?? false
            })
            .ToListAsync();

        player.AdvancedPitchingSeasons = await context.PlayerSeasonPitchingAdvanced
            .Where(p => p.PlayerId == id)
            .OrderByDescending(p => p.YearId)
            .Select(p => new AdvancedPitchingSeason
            {
                Year = p.YearId,
                LgId = p.LgId,
                Ip = p.Ip,
                K9 = p.K9,
                Bb9 = p.Bb9,
                Whip = p.Whip,
                Qualified = p.Qualified ?? false
            })
            .ToListAsync();

        // Get season-by-season batting records
        player.BattingSeasons = await context.Batting
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
        player.PitchingSeasons = await context.Pitching
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

        // Get team names and franchise ids so team entries can link to franchise pages
        var teamIds = allTeams.Select(t => t.TeamId).Distinct().ToList();
        var teamInfo = await context.Teams
            .Where(t => teamIds.Contains(t.TeamId))
            .GroupBy(t => t.TeamId)
            .Select(g => new
            {
                TeamId = g.Key,
                Name = g.OrderByDescending(t => t.YearId).First().Name,
                FranchId = g.OrderByDescending(t => t.YearId).First().FranchId
            })
            .ToDictionaryAsync(t => t.TeamId, t => new { t.Name, t.FranchId });

        player.Teams = allTeams.Select(t => new TeamRecord
        {
            TeamId = t.TeamId,
            TeamName = teamInfo.GetValueOrDefault(t.TeamId)?.Name,
            FranchId = teamInfo.GetValueOrDefault(t.TeamId)?.FranchId,
            FirstYear = t.FirstYear,
            LastYear = t.LastYear,
            Seasons = t.Seasons
        }).ToList();

        // Get awards
        player.Awards = await context.AwardsPlayers
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

        // Merge in manager awards (Manager of the Year etc.) for players who managed
        var managerAwards = await context.AwardsManagers
            .Where(a => a.PlayerId == id)
            .Select(a => new AwardRecord
            {
                Year = a.YearId,
                AwardId = a.AwardId,
                LgId = a.LgId,
                Notes = a.Notes
            })
            .ToListAsync();

        if (managerAwards.Count > 0)
        {
            player.Awards = player.Awards
                .Concat(managerAwards)
                .OrderByDescending(a => a.Year)
                .ToList();
        }

        // Managerial career, for the Managing tab and the /Managers cross-link
        player.ManagerialSeasons = await context.Managers
            .Where(m => m.PlayerId == id)
            .OrderByDescending(m => m.YearId)
            .ThenBy(m => m.Inseason)
            .Select(m => new ManagerSeasonRow
            {
                Year = m.YearId,
                TeamId = m.TeamId,
                LgId = m.LgId,
                TeamName = m.Team.Name,
                Inseason = m.Inseason,
                Games = m.G,
                Wins = m.W,
                Losses = m.L,
                Rank = m.Rank,
                IsPlayerManager = m.PlyrMgr == "Y",
                WonPennant = m.Team.LgWin == "Y",
                WonWorldSeries = m.Team.Wswin == "Y"
            })
            .ToListAsync();

        // Get All-Star appearances
        var allStarData = await context.AllstarFull
            .Where(a => a.PlayerId == id)
            .OrderByDescending(a => a.YearId)
            .Select(a => new { a.YearId, a.LgId, a.TeamId, a.GameNum })
            .ToListAsync();

        player.AllStarAppearances = allStarData.Select(a => new AllStarRecord
        {
            Year = a.YearId,
            LgId = a.LgId,
            TeamId = a.TeamId,
            GameNum = int.TryParse(a.GameNum, out var gameNum) ? gameNum : 0
        }).ToList();

        // Get season-by-season fielding records; PO/A/E/DP are string columns
        // that can be empty, so pull raw values and parse in memory
        var fieldingRows = await context.Fielding
            .Where(f => f.PlayerId == id)
            .OrderByDescending(f => f.YearId)
            .ThenBy(f => f.Pos)
            .Select(f => new
            {
                f.YearId, f.TeamId, TeamName = f.Team.Name, f.LgId, f.Pos,
                Games = f.G ?? 0, f.Po, f.A, f.E, f.Dp
            })
            .ToListAsync();

        player.FieldingSeasons = fieldingRows.Select(f => new SeasonFieldingRecord
        {
            Year = f.YearId,
            TeamId = f.TeamId,
            TeamName = f.TeamName,
            LgId = f.LgId,
            Position = f.Pos,
            Games = f.Games,
            Putouts = LahmanNumbers.ParseIntOrZero(f.Po),
            Assists = LahmanNumbers.ParseIntOrZero(f.A),
            Errors = LahmanNumbers.ParseIntOrZero(f.E),
            DoublePlays = LahmanNumbers.ParseIntOrZero(f.Dp)
        }).ToList();

        // Get postseason batting lines; SO is a string column, and rounds sort
        // in playoff chronology (WC -> DS -> CS -> WS), not alphabetically
        var postBattingRows = await context.BattingPost
            .Where(b => b.PlayerId == id)
            .Select(b => new
            {
                b.YearId, b.Round, b.TeamId, TeamName = b.Team.Name, b.LgId,
                G = b.G ?? 0, Ab = b.Ab ?? 0, R = b.R ?? 0, H = b.H ?? 0,
                Doubles = b._2b ?? 0, Triples = b._3b ?? 0, Hr = b.Hr ?? 0,
                Rbi = b.Rbi ?? 0, Sb = b.Sb ?? 0, Bb = b.Bb ?? 0, b.So
            })
            .ToListAsync();

        player.PostseasonBattingSeasons = postBattingRows
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => PostseasonViewModel.RoundChronologicalRank(b.Round))
            .Select(b => new PostseasonBattingRecord
            {
                Year = b.YearId,
                Round = b.Round,
                TeamId = b.TeamId,
                TeamName = b.TeamName,
                LgId = b.LgId,
                Games = b.G,
                AtBats = b.Ab,
                Runs = b.R,
                Hits = b.H,
                Doubles = b.Doubles,
                Triples = b.Triples,
                HomeRuns = b.Hr,
                Rbi = b.Rbi,
                StolenBases = b.Sb,
                Walks = b.Bb,
                Strikeouts = LahmanNumbers.ParseIntOrZero(b.So)
            })
            .ToList();

        // Get postseason pitching lines
        var postPitchingRows = await context.PitchingPost
            .Where(p => p.PlayerId == id)
            .Select(p => new
            {
                p.YearId, p.Round, TeamId = p.TeamId ?? "", TeamName = p.Team != null ? p.Team.Name : null,
                LgId = p.LgId ?? "", G = p.G ?? 0, Gs = p.Gs ?? 0, W = p.W ?? 0, L = p.L ?? 0,
                Sv = p.Sv ?? 0, Ipouts = p.Ipouts ?? 0, H = p.H ?? 0, Er = p.Er ?? 0,
                Bb = p.Bb ?? 0, So = p.So ?? 0
            })
            .ToListAsync();

        player.PostseasonPitchingSeasons = postPitchingRows
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => PostseasonViewModel.RoundChronologicalRank(p.Round))
            .Select(p => new PostseasonPitchingRecord
            {
                Year = p.YearId,
                Round = p.Round,
                TeamId = p.TeamId,
                TeamName = p.TeamName,
                LgId = p.LgId,
                Games = p.G,
                GamesStarted = p.Gs,
                Wins = p.W,
                Losses = p.L,
                Saves = p.Sv,
                InningsPitched = p.Ipouts / 3.0,
                Hits = p.H,
                EarnedRuns = p.Er,
                Walks = p.Bb,
                Strikeouts = p.So
            })
            .ToList();

        return player;
    }
}
