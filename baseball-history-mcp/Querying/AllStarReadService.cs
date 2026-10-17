using BaseballHistory.Data.Models;
using BaseballHistory.Data.Reference;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class AllStarReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService) : IAllStarReadService
{
    public async Task<IReadOnlyList<AllStarYearSummaryReadModel>> ListAllStarYearsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Game identity mixes Retrosheet game IDs (AL/NL) and squad codes
        // (East-West, North-South), so classify in memory
        var rows = await context.AllstarFull
            .Select(a => new { a.YearId, a.GameId, a.LgId, a.PlayerId })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(a => a.YearId)
            .Select(g => new AllStarYearSummaryReadModel(
                g.Key,
                g.Select(a => AllStarGames.GroupKey(a.LgId, a.GameId)).Distinct().Count(),
                g.Select(a => a.PlayerId).Distinct().Count(),
                g.Any(a => a.LgId is "EAS" or "WES")))
            .OrderByDescending(y => y.Year)
            .ToList();
    }

    public async Task<AllStarYearReadModel?> GetAllStarYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var seasonYear = McpInputValidation.ValidateYear(year);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var rows = await context.AllstarFull
            .Where(a => a.YearId == seasonYear)
            .Select(a => new
            {
                a.PlayerId,
                a.Player.NameFirst,
                a.Player.NameLast,
                a.GameId,
                a.GameNum,
                a.TeamId,
                a.LgId,
                a.Gp,
                a.StartingPos
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return null;
        }

        // The squad codes (EAS/WES/NOS/SAS) aren't season leagues, so clubs
        // resolve by teamID alone against that year's Teams rows
        var teamsThisYear = await context.Teams
            .Where(t => t.YearId == seasonYear)
            .Select(t => new { t.TeamId, t.LgId, t.Name })
            .ToListAsync(cancellationToken);
        var teamByIdAndLeague = teamsThisYear.ToDictionary(t => (t.TeamId, t.LgId));
        var teamById = teamsThisYear.GroupBy(t => t.TeamId).ToDictionary(g => g.Key, g => g.First());

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var games = rows
            .GroupBy(r => AllStarGames.GroupKey(r.LgId, r.GameId))
            .Select(g =>
            {
                var leagues = g.Select(r => r.LgId).Distinct().OrderBy(l => l).ToList();
                var rosters = g
                    .GroupBy(r => r.LgId)
                    .OrderBy(lg => lg.Key)
                    .Select(lg => new AllStarRosterReadModel(
                        lg.Key,
                        AllStarGames.SquadName(lg.Key),
                        lg.Select(r =>
                            {
                                var team = r.LgId is "AL" or "NL"
                                    ? teamByIdAndLeague.GetValueOrDefault((r.TeamId, r.LgId))
                                    : teamById.GetValueOrDefault(r.TeamId);
                                int? startingPos = int.TryParse(r.StartingPos, out var pos) && pos > 0 ? pos : null;
                                return new AllStarRosterPlayerReadModel(
                                    r.PlayerId,
                                    FormatName(r.NameFirst, r.NameLast, r.PlayerId),
                                    r.TeamId,
                                    team?.Name,
                                    team?.LgId,
                                    startingPos.HasValue,
                                    startingPos,
                                    r.Gp == 1,
                                    hallOfFamers.Contains(r.PlayerId));
                            })
                            .OrderByDescending(p => p.IsStarter)
                            .ThenBy(p => p.StartingPos ?? int.MaxValue)
                            .ThenBy(p => p.FullName)
                            .ToList()))
                    .ToList();

                return (GroupKey: g.Key, Game: new AllStarGameReadModel(
                    AllStarGames.TitleFor(g.Key, leagues),
                    AllStarGames.GameType(g.Key),
                    g.Max(r => int.TryParse(r.GameNum, out var num) ? num : 0),
                    AllStarGames.ParseGameDate(g.Select(r => r.GameId).FirstOrDefault(id => !string.IsNullOrEmpty(id))),
                    rosters));
            })
            .OrderBy(g => AllStarGames.TypeOrder(g.GroupKey))
            .ThenBy(g => g.Game.GameNum)
            .ThenBy(g => g.Game.GameDate)
            .Select(g => g.Game)
            .ToList();

        var availableYears = await context.AllstarFull
            .Select(a => a.YearId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new AllStarYearReadModel(
            seasonYear,
            availableYears.Where(y => y < seasonYear).Select(y => (short?)y).Max(),
            availableYears.Where(y => y > seasonYear).Select(y => (short?)y).Min(),
            games);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
