using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace baseball_history_tests.Services;

public class PlayerCacheServiceTests
{
    [Fact]
    public async Task StartAsync_WarmsDefaultPlayersPageIntoCache()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var provider = CreateServiceProvider();
        using var service = new PlayerCacheService(provider, cache);

        var cachedPage = await WarmCacheAsync(service, cache);

        Assert.NotNull(PlayerCacheService.GetCachedFirstPage(cache));
        Assert.Equal("A", cachedPage.CurrentLetter);
        Assert.Equal(1, cachedPage.CurrentPage);
        Assert.Equal(48, cachedPage.PageSize);
        Assert.NotEmpty(cachedPage.Players);
    }

    [Fact]
    public async Task StartAsync_SetsHallOfFameAndPartialRecordFlagsFromUnderlyingData()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var provider = CreateServiceProvider();
        using var service = new PlayerCacheService(provider, cache);

        var cachedPage = await WarmCacheAsync(service, cache);
        var cachedPlayers = cachedPage.Players.ToDictionary(player => player.PlayerId);

        await using var context = TestDatabaseFactory.CreateContext();
        var expectedPlayers = await context.People
            .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"))
            .OrderBy(p => p.NameLast)
            .ThenBy(p => p.NameFirst)
            .Take(48)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst
            })
            .ToListAsync();
        var hallOfFameIds = await context.HallOfFame
            .Where(h => h.Inducted == "Y" && cachedPlayers.Keys.Contains(h.PlayerId))
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSetAsync();

        Assert.Equal(expectedPlayers.Count, cachedPlayers.Count);
        foreach (var expected in expectedPlayers)
        {
            Assert.True(cachedPlayers.TryGetValue(expected.PlayerId, out var cached));
            Assert.NotNull(cached);
            Assert.Equal(hallOfFameIds.Contains(expected.PlayerId), cached.IsInHallOfFame);
            Assert.Equal(PlayerRecordFacts.IsPartialName(expected.NameFirst), cached.IsPartialRecord);
        }
    }

    [Fact]
    public async Task StartAsync_BuildsAvailableLettersAndPaginationFromDatabase()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var provider = CreateServiceProvider();
        using var service = new PlayerCacheService(provider, cache);

        var cachedPage = await WarmCacheAsync(service, cache);

        await using var context = TestDatabaseFactory.CreateContext();
        var expectedLetters = await context.People
            .Where(p => p.NameLast != null && p.NameLast.Length > 0)
            .Select(p => p.NameLast!.Substring(0, 1).ToUpper())
            .Distinct()
            .OrderBy(letter => letter)
            .ToListAsync();
        var normalizedLetters = expectedLetters
            .Where(letter => letter.Length == 1 && char.IsLetter(letter[0]))
            .Select(letter => letter[0])
            .ToList();

        var totalPlayers = await context.People
            .CountAsync(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"));

        Assert.Equal(normalizedLetters, cachedPage.AvailableLetters);
        Assert.Equal(totalPlayers, cachedPage.TotalPlayers);
        Assert.Equal((int)Math.Ceiling(totalPlayers / 48.0), cachedPage.TotalPages);
        Assert.Contains('A', cachedPage.AvailableLetters);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => TestDatabaseFactory.CreateContext());
        return services.BuildServiceProvider();
    }

    private static async Task<PlayerListViewModel> WarmCacheAsync(PlayerCacheService service, IMemoryCache cache)
    {
        await service.StartAsync(CancellationToken.None);

        for (var attempt = 0; attempt < 50; attempt++)
        {
            var cached = PlayerCacheService.GetCachedFirstPage(cache);
            if (cached != null)
            {
                await service.StopAsync(CancellationToken.None);
                return cached;
            }

            await Task.Delay(100);
        }

        await service.StopAsync(CancellationToken.None);
        throw new Xunit.Sdk.XunitException("Player cache warmup did not complete within the expected time.");
    }
}
