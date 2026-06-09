using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_mcp.Querying;

public interface IHallOfFameReadService
{
    Task<HashSet<string>> GetInductedPlayerIdsAsync(CancellationToken cancellationToken = default);
}

public sealed class HallOfFameReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IMemoryCache cache) : IHallOfFameReadService
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
}
