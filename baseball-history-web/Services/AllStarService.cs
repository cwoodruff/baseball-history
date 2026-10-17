using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using BaseballHistory.Data.Reference;

namespace baseball_history_web.Services;

/// <summary>
/// All-Star Game index and roster queries, shared by the /AllStar pages and
/// the /api/all-star endpoints.
/// </summary>
public class AllStarService(BaseballDbContext context, IMemoryCache cache)
{
    /// <summary>One row per All-Star season, most recent first</summary>
    public async Task<List<AllStarYearSummary>> GetYearSummariesAsync()
    {
        // Game identity mixes Retrosheet game IDs (AL/NL) and squad codes
        // (East-West, North-South), so classify in memory
        var rows = await context.AllstarFull
            .Select(a => new { a.YearId, a.GameId, a.LgId, a.PlayerId })
            .ToListAsync();

        return rows
            .GroupBy(a => a.YearId)
            .Select(g => new AllStarYearSummary
            {
                Year = g.Key,
                GameCount = g.Select(a => AllStarGames.GroupKey(a.LgId, a.GameId)).Distinct().Count(),
                Selections = g.Select(a => a.PlayerId).Distinct().Count(),
                HasEastWestGame = g.Any(a => a.LgId is "EAS" or "WES")
            })
            .OrderByDescending(y => y.Year)
            .ToList();
    }

    /// <summary>The season's games and rosters, or null when no All-Star data exists for the year</summary>
    public async Task<AllStarYearViewModel?> GetYearAsync(short year)
    {
        var rows = await context.AllstarFull
            .Where(a => a.YearId == year)
            .Select(a => new
            {
                a.PlayerId,
                FullName = (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                a.GameId,
                a.GameNum,
                a.TeamId,
                a.LgId,
                a.Gp,
                a.StartingPos
            })
            .ToListAsync();

        if (rows.Count == 0)
        {
            return null;
        }

        // The squad codes (EAS/WES/NOS/SAS) aren't season leagues, so club names
        // and links resolve by teamID alone against that year's Teams rows
        var teamsThisYear = await context.Teams
            .Where(t => t.YearId == year)
            .Select(t => new { t.TeamId, t.LgId, t.Name })
            .ToListAsync();
        var teamByIdAndLeague = teamsThisYear.ToDictionary(t => (t.TeamId, t.LgId), t => t);
        var teamById = teamsThisYear
            .GroupBy(t => t.TeamId)
            .ToDictionary(g => g.Key, g => g.First());

        var hofPlayerIds = await HallOfFameIds.GetAsync(context, cache);

        var games = rows
            .GroupBy(r => AllStarGames.GroupKey(r.LgId, r.GameId))
            .Select(g =>
            {
                var leagues = g.Select(r => r.LgId).Distinct().OrderBy(l => l).ToList();
                return new AllStarGameViewModel
                {
                    GroupKey = g.Key,
                    Title = AllStarGames.TitleFor(g.Key, leagues),
                    GameNum = g.Max(r => int.TryParse(r.GameNum, out var num) ? num : 0),
                    GameDate = AllStarGameViewModel.ParseGameDate(
                        g.Select(r => r.GameId).FirstOrDefault(id => !string.IsNullOrEmpty(id))),
                    Rosters = g
                        .GroupBy(r => r.LgId)
                        .OrderBy(lg => lg.Key)
                        .Select(lg => new AllStarRosterGroup
                        {
                            LgId = lg.Key,
                            Players = lg
                                .Select(r =>
                                {
                                    var isSeasonLeague = r.LgId is "AL" or "NL";
                                    var team = isSeasonLeague
                                        ? teamByIdAndLeague.GetValueOrDefault((r.TeamId, r.LgId))
                                        : teamById.GetValueOrDefault(r.TeamId);
                                    return new AllStarRosterRow
                                    {
                                        PlayerId = r.PlayerId,
                                        FullName = r.FullName.Trim(),
                                        TeamId = r.TeamId,
                                        TeamName = team?.Name,
                                        LinkLgId = team?.LgId,
                                        StartingPos = int.TryParse(r.StartingPos, out var pos) ? pos : null,
                                        Played = r.Gp == 1,
                                        IsInHallOfFame = hofPlayerIds.Contains(r.PlayerId)
                                    };
                                })
                                .OrderByDescending(p => p.IsStarter)
                                .ThenBy(p => p.StartingPos ?? int.MaxValue)
                                .ThenBy(p => p.FullName)
                                .ToList()
                        })
                        .ToList()
                };
            })
            .OrderBy(g => AllStarGames.TypeOrder(g.GroupKey))
            .ThenBy(g => g.GameNum)
            .ThenBy(g => g.GameDate)
            .ToList();

        return new AllStarYearViewModel
        {
            Year = year,
            Games = games,
            AvailableYears = await context.AllstarFull
                .Select(a => a.YearId)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync()
        };
    }
}
