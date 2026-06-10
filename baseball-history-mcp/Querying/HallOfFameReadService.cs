using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_mcp.Querying;

public interface IHallOfFameReadService
{
    Task<HashSet<string>> GetInductedPlayerIdsAsync(CancellationToken cancellationToken = default);
    Task<PagedReadResult<HallOfFameInducteeReadModel>> ListInducteesAsync(HallOfFameLookupRequest request, CancellationToken cancellationToken = default);
    Task<HallOfFameVotingHistoryReadModel?> GetVotingHistoryAsync(string playerId, CancellationToken cancellationToken = default);
}

public sealed class HallOfFameReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IMemoryCache cache,
    BaseballMcpRequestPolicy requestPolicy) : IHallOfFameReadService
{
    private const string CacheKey = "mcp_hof_player_ids";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<HashSet<string>> GetInductedPlayerIdsAsync(CancellationToken cancellationToken = default)
    {
        return (await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync(cancellationToken);
        }))!;
    }

    public async Task<PagedReadResult<HallOfFameInducteeReadModel>> ListInducteesAsync(
        HallOfFameLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.HallOfFame
            .Where(h => h.Inducted == "Y");

        if (normalizedRequest.Year.HasValue)
        {
            query = query.Where(h => h.Yearid == normalizedRequest.Year.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedRequest.Category))
        {
            query = query.Where(h => h.Category != null && EF.Functions.ILike(h.Category, normalizedRequest.Category));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageWindow = requestPolicy.CreateHallOfFameLookupWindow(normalizedRequest, totalCount);

        var rows = await query
            .OrderByDescending(h => h.Yearid)
            .ThenBy(h => h.Player.NameLast)
            .ThenBy(h => h.Player.NameFirst)
            .ThenBy(h => h.PlayerId)
            .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
            .Take(pageWindow.PageSize)
            .Select(h => new
            {
                h.PlayerId,
                h.Player.NameFirst,
                h.Player.NameLast,
                h.Yearid,
                h.Category,
                h.VotedBy,
                h.Votes,
                h.Ballots,
                DebutYear = h.Player.Debut,
                FinalYear = h.Player.FinalGame
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row =>
        {
            double? votePercentage = null;
            if (int.TryParse(row.Votes, out var votes) && int.TryParse(row.Ballots, out var ballots) && ballots > 0)
            {
                votePercentage = Math.Round((double)votes / ballots * 100, 1);
            }

            return new HallOfFameInducteeReadModel(
                row.PlayerId,
                FormatName(row.NameFirst, row.NameLast, row.PlayerId),
                row.Yearid,
                row.Category ?? "Player",
                row.VotedBy,
                votePercentage,
                row.DebutYear?.Year,
                row.FinalYear?.Year);
        }).ToList();

        return pageWindow.CreateResult(items, totalCount);
    }

    public async Task<HallOfFameVotingHistoryReadModel?> GetVotingHistoryAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);

        if (person is null)
        {
            return null;
        }

        var history = await context.HallOfFame
            .Where(h => h.PlayerId == playerId)
            .OrderBy(h => h.Yearid)
            .ThenBy(h => h.VotedBy)
            .Select(h => new { h.Yearid, h.VotedBy, h.Votes, h.Ballots, h.Inducted })
            .ToListAsync(cancellationToken);

        if (history.Count == 0)
        {
            return null;
        }

        return new HallOfFameVotingHistoryReadModel(
            playerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            history.Select(row =>
            {
                double? votePercentage = null;
                if (int.TryParse(row.Votes, out var votes) && int.TryParse(row.Ballots, out var ballots) && ballots > 0)
                {
                    votePercentage = Math.Round((double)votes / ballots * 100, 1);
                }

                return new HallOfFameVotingYearReadModel(
                    row.Yearid,
                    row.VotedBy,
                    row.Votes,
                    row.Ballots,
                    votePercentage,
                    row.Inducted == "Y");
            }).ToList());
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
