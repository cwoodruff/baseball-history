using baseball_history_mcp.Configuration;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Querying;

public sealed class ManagerReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IOptions<BaseballMcpOptions> options) : IManagerReadService
{
    public async Task<ManagerReadModel?> GetManagerAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedPlayerId = McpInputValidation.NormalizeRequiredPlayerId(playerId);
        var maxSeasonRows = options.Value.Limits.ManagerSeasonRowsMax;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var allSeasons = await context.Managers
            .Where(m => m.PlayerId == normalizedPlayerId)
            .Select(m => new
            {
                m.YearId,
                m.TeamId,
                TeamName = m.Team.Name,
                m.LgId,
                m.Inseason,
                m.G,
                m.W,
                m.L,
                m.Rank,
                m.PlyrMgr,
                LgWin = m.Team.LgWin,
                WsWin = m.Team.Wswin
            })
            .ToListAsync(cancellationToken);

        if (allSeasons.Count == 0)
        {
            return null;
        }

        var person = await context.People
            .Where(p => p.PlayerId == normalizedPlayerId)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst,
                p.NameLast,
                WasPlayer = p.Battings.Any() || p.Pitchings.Any()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var isInHallOfFame = await context.HallOfFame
            .AnyAsync(h => h.PlayerId == normalizedPlayerId && h.Inducted == "Y", cancellationToken);

        var awards = await context.AwardsManagers
            .Where(a => a.PlayerId == normalizedPlayerId)
            .OrderByDescending(a => a.YearId)
            .Select(a => new ManagerAwardReadModel(a.YearId, a.AwardId, a.LgId))
            .ToListAsync(cancellationToken);

        var seasons = allSeasons
            .OrderByDescending(s => s.YearId)
            .ThenBy(s => s.Inseason)
            .Take(maxSeasonRows)
            .Select(s => new ManagerSeasonReadModel(
                s.YearId,
                s.TeamId,
                s.TeamName,
                s.LgId,
                s.Inseason,
                s.G ?? 0,
                s.W ?? 0,
                s.L ?? 0,
                s.Rank,
                s.PlyrMgr == "Y",
                s.LgWin == "Y",
                s.WsWin == "Y"))
            .ToList();

        var wins = allSeasons.Sum(s => (int)(s.W ?? 0));
        var losses = allSeasons.Sum(s => (int)(s.L ?? 0));

        return new ManagerReadModel(
            normalizedPlayerId,
            FormatName(person?.NameFirst, person?.NameLast, normalizedPlayerId),
            isInHallOfFame,
            person?.WasPlayer ?? false,
            allSeasons.Any(s => s.PlyrMgr == "Y"),
            allSeasons.Min(s => s.YearId),
            allSeasons.Max(s => s.YearId),
            allSeasons.Select(s => s.YearId).Distinct().Count(),
            allSeasons.Sum(s => (int)(s.G ?? 0)),
            wins,
            losses,
            wins + losses > 0 ? Math.Round((double)wins / (wins + losses), 3) : 0,
            allSeasons.Count(s => s.LgWin == "Y"),
            allSeasons.Count(s => s.WsWin == "Y"),
            allSeasons.Count,
            maxSeasonRows,
            allSeasons.Count > maxSeasonRows,
            awards,
            seasons);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
