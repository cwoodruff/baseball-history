using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Services;

/// <summary>
/// The cached set of inducted Hall of Fame player IDs, shared under the
/// "hof_player_ids" key that the pages and API endpoints already use.
/// </summary>
public static class HallOfFameIds
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public static async Task<HashSet<string>> GetAsync(BaseballDbContext context, IMemoryCache cache) =>
        (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;
}
