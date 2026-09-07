using baseball_history_mcp.Configuration;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Querying;

public sealed class PostseasonReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IOptions<BaseballMcpOptions> options) : IPostseasonReadService
{
    public async Task<PlayerPostseasonReadModel?> GetPlayerPostseasonAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedPlayerId = McpInputValidation.NormalizeRequiredPlayerId(playerId);
        var maxRows = options.Value.Limits.PostseasonRowsPerCategoryMax;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .Where(p => p.PlayerId == normalizedPlayerId)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        var battingQuery = context.BattingPost
            .Where(b => b.PlayerId == normalizedPlayerId)
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => b.Round);

        var totalBattingRows = await battingQuery.CountAsync(cancellationToken);
        var battingRaw = await battingQuery
            .Take(maxRows)
            .Select(b => new
            {
                b.YearId,
                b.Round,
                b.TeamId,
                TeamName = b.Team.Name,
                b.LgId,
                b.G,
                b.Ab,
                b.R,
                b.H,
                Doubles = b._2b,
                Triples = b._3b,
                b.Hr,
                b.Rbi,
                b.Sb,
                b.Bb,
                b.So
            })
            .ToListAsync(cancellationToken);

        var pitchingQuery = context.PitchingPost
            .Where(p => p.PlayerId == normalizedPlayerId)
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => p.Round);

        var totalPitchingRows = await pitchingQuery.CountAsync(cancellationToken);
        var pitchingRaw = await pitchingQuery
            .Take(maxRows)
            .Select(p => new
            {
                p.YearId,
                p.Round,
                p.TeamId,
                TeamName = p.Team != null ? p.Team.Name : null,
                p.LgId,
                p.W,
                p.L,
                p.G,
                p.Gs,
                p.Cg,
                p.Sho,
                p.Sv,
                p.Ipouts,
                p.H,
                p.Er,
                p.Hr,
                p.Bb,
                p.So,
                p.Era
            })
            .ToListAsync(cancellationToken);

        // Rounds sort chronologically within a year (wild card -> division ->
        // championship -> World Series), not alphabetically
        var batting = battingRaw
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => RoundRank(b.Round))
            .Select(b =>
            {
                var atBats = b.Ab ?? 0;
                var hits = b.H ?? 0;
                return new PostseasonBattingRowReadModel(
                    b.YearId,
                    b.Round,
                    b.TeamId,
                    b.TeamName,
                    b.LgId,
                    b.G ?? 0,
                    atBats,
                    b.R ?? 0,
                    hits,
                    b.Doubles ?? 0,
                    b.Triples ?? 0,
                    b.Hr ?? 0,
                    b.Rbi ?? 0,
                    b.Sb ?? 0,
                    b.Bb ?? 0,
                    ParseIntOrZero(b.So),
                    atBats > 0 ? Math.Round((double)hits / atBats, 3) : 0);
            })
            .ToList();

        var pitching = pitchingRaw
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => RoundRank(p.Round))
            .Select(p => new PostseasonPitchingRowReadModel(
                p.YearId,
                p.Round,
                p.TeamId ?? "",
                p.TeamName,
                p.LgId,
                p.W ?? 0,
                p.L ?? 0,
                p.G ?? 0,
                p.Gs ?? 0,
                p.Cg ?? 0,
                p.Sho ?? 0,
                p.Sv ?? 0,
                Math.Round((p.Ipouts ?? 0) / 3.0, 1),
                p.H ?? 0,
                p.Er ?? 0,
                p.Hr ?? 0,
                p.Bb ?? 0,
                p.So ?? 0,
                double.TryParse(p.Era, out var era) ? era : null))
            .ToList();

        return new PlayerPostseasonReadModel(
            person.PlayerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            totalBattingRows,
            totalPitchingRows,
            maxRows,
            totalBattingRows > maxRows,
            totalPitchingRows > maxRows,
            batting,
            pitching);
    }

    // Chronological order of postseason rounds within a season
    private static int RoundRank(string round) => round switch
    {
        _ when round.Contains("WC", StringComparison.OrdinalIgnoreCase) => 0,
        _ when round.Contains("DIV", StringComparison.OrdinalIgnoreCase) => 1,
        "ALDS1" or "ALDS2" or "NLDS1" or "NLDS2" or "AEDIV" or "AWDIV" or "NEDIV" or "NWDIV" => 1,
        "ALCS" or "NLCS" => 2,
        "WS" or "CS" => 3,
        _ => 4
    };

    private static int ParseIntOrZero(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
