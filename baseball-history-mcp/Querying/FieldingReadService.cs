using baseball_history_mcp.Configuration;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Querying;

public sealed class FieldingReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IOptions<BaseballMcpOptions> options) : IFieldingReadService
{
    public async Task<PlayerFieldingReadModel?> GetPlayerFieldingAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedPlayerId = McpInputValidation.NormalizeRequiredPlayerId(playerId);
        var maxRows = options.Value.Limits.FieldingSeasonRowsMax;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .Where(p => p.PlayerId == normalizedPlayerId)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        var fieldingQuery = context.Fielding
            .Where(f => f.PlayerId == normalizedPlayerId)
            .OrderByDescending(f => f.YearId)
            .ThenBy(f => f.Stint)
            .ThenBy(f => f.Pos);

        var totalRows = await fieldingQuery.CountAsync(cancellationToken);

        // GS/InnOuts/PO/A/E/DP are string columns in the Lahman data; pull raw
        // values and parse in memory, never cast inside the EF query
        var rowsRaw = await fieldingQuery
            .Take(maxRows)
            .Select(f => new
            {
                f.YearId,
                f.Stint,
                f.TeamId,
                TeamName = f.Team.Name,
                f.LgId,
                f.Pos,
                f.G,
                f.Gs,
                f.InnOuts,
                f.Po,
                f.A,
                f.E,
                f.Dp
            })
            .ToListAsync(cancellationToken);

        var seasons = rowsRaw
            .Select(f =>
            {
                var putouts = ParseIntOrZero(f.Po);
                var assists = ParseIntOrZero(f.A);
                var errors = ParseIntOrZero(f.E);
                var innOuts = ParseIntOrNull(f.InnOuts);
                return new FieldingSeasonRowReadModel(
                    f.YearId,
                    f.Stint,
                    f.TeamId,
                    f.TeamName,
                    f.LgId,
                    f.Pos,
                    f.G ?? 0,
                    ParseIntOrZero(f.Gs),
                    innOuts.HasValue ? Math.Round(innOuts.Value / 3.0, 1) : null,
                    putouts,
                    assists,
                    errors,
                    ParseIntOrZero(f.Dp),
                    FieldingPercentage(putouts, assists, errors));
            })
            .ToList();

        var careerByPosition = seasons
            .GroupBy(s => s.Position)
            .Select(g =>
            {
                var putouts = g.Sum(s => s.Putouts);
                var assists = g.Sum(s => s.Assists);
                var errors = g.Sum(s => s.Errors);
                return new FieldingCareerPositionReadModel(
                    g.Key,
                    g.Sum(s => s.Games),
                    putouts,
                    assists,
                    errors,
                    g.Sum(s => s.DoublePlays),
                    FieldingPercentage(putouts, assists, errors));
            })
            .OrderByDescending(p => p.Games)
            .ToList();

        return new PlayerFieldingReadModel(
            person.PlayerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            totalRows,
            maxRows,
            totalRows > maxRows,
            careerByPosition,
            seasons);
    }

    private static double? FieldingPercentage(int putouts, int assists, int errors)
    {
        var chances = putouts + assists + errors;
        return chances > 0 ? Math.Round((double)(putouts + assists) / chances, 3) : null;
    }

    private static int ParseIntOrZero(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;

    private static int? ParseIntOrNull(string? value) => int.TryParse(value, out var parsed) ? parsed : null;

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
