# EF Core Read-Only Pattern with Projection & Caching

## Context

Read-only, historical-data applications (e.g., sports statistics, analytics dashboards) that serve large datasets from a static schema without writes.

## Pattern

### 1. Global NoTracking Configuration

```csharp
builder.Services.AddDbContext<BaseballDbContext>(options =>
    options.UseSqlite(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

**Benefit:** Eliminates 15-20% memory overhead from change-tracking metadata. Safe for read-only workloads.

### 2. Early Projection in Queries

Always project only needed columns at the query stage:

```csharp
// ✅ Good: Project early, avoid full entity hydration
var players = await context.People
    .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"))
    .OrderBy(p => p.NameLast)
    .Select(p => new
    {
        p.PlayerId,
        p.NameFirst,
        p.NameLast,
        TotalGames = (p.Battings.Sum(b => (int?)b.G) ?? 0) + 
                     (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0),
        IsHOF = p.HallOfFames.Any(h => h.Inducted == "Y")
    })
    .ToListAsync();

// ❌ Avoid: Full entity load, then projection in LINQ-to-Objects
var players = await context.People
    .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"))
    .OrderBy(p => p.NameLast)
    .ToListAsync(); // ← Pulls entire People records + navigation
// var result = players.Select(p => new { ... }); ← LINQ-to-Objects, missed optimization
```

**Benefit:** Smaller result sets from DB, aggregations compile to SQL, no N+1 queries.

### 3. Aggregations in Select Projection

Aggregations (`.Sum()`, `.Count()`, `.Any()`, `.First()`) inside `.Select()` compile to SQL:

```csharp
// ✅ Single query with aggregations in SQL
.Select(p => new
{
    p.PlayerId,
    TotalGames = (p.Battings.Sum(b => (int?)b.G) ?? 0),
    LastTeam = p.Battings
        .OrderByDescending(b => b.YearId)
        .Select(b => b.TeamId)
        .FirstOrDefault()
})
.ToListAsync()
```

### 4. Strategic Includes for Navigation

Use `Include()` only when necessary to project or filter deeply. Always follow with projection:

```csharp
// ✅ Include used for deep projection
var franchise = await context.TeamsFranchises
    .Include(f => f.Teams)
    .Select(f => new
    {
        f.FranchiseId,
        f.FranchiseName,
        SeasonCount = f.Teams.Count,
        Years = f.Teams.Select(t => t.Year)
    })
    .FirstOrDefaultAsync(f => f.FranchiseId == id);
```

### 5. IMemoryCache for Filter Options & Computed Lookups

Cache expensive filter queries and computed lookups with 24-hour TTL:

```csharp
// Get filter options (e.g., available years for leaderboards)
var years = await cache.GetOrCreateAsync("batting_years", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
    return await context.Batting
        .Select(b => (int)b.YearId)
        .Distinct()
        .OrderByDescending(y => y)
        .ToListAsync();
});

// Cache lookup sets (e.g., Hall of Fame player IDs)
var hofIds = await cache.GetOrCreateAsync("hof_player_ids", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
    return await context.HallOfFame
        .Where(h => h.Inducted == "Y")
        .Select(h => h.PlayerId)
        .ToHashSetAsync();
});
```

**Benefit:** 24-hour refresh amortizes the cost of expensive distinct/grouping queries.

### 6. Background Cache Warmer for Hot Paths

Use `AddHostedService<>` to pre-populate cache for frequently accessed views:

```csharp
public class PlayerCacheService(IServiceProvider serviceProvider, IMemoryCache cache) : BackgroundService
{
    private const string CacheKey = "players_first_page";
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await BuildCache(stoppingToken); // Build immediately at startup
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshInterval, stoppingToken);
            await BuildCache(stoppingToken);
        }
    }

    private async Task BuildCache(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BaseballDbContext>();
        
        var result = await context.People
            .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"))
            .Select(/* projection */)
            .Take(48)
            .ToListAsync(ct);
        
        cache.Set(CacheKey, result, RefreshInterval.Add(TimeSpan.FromMinutes(5)));
    }
}
```

**Benefit:** Instant page renders for default/home views; users don't see DB query delay.

### 7. Response Cache for HTTP-Level Caching

Combine `[ResponseCache]` with htmx header awareness:

```csharp
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // ... load data
        if (Request.IsHtmxNonBoostedRequest())
            return Partial("_PlayersContent", ViewModel);
        return Page();
    }
}
```

**Benefit:** Browsers cache full pages for 1 hour; htmx partials cached separately. Reduces repeat API calls.

## Anti-Patterns to Avoid

- ❌ **Full entity load then LINQ-to-Objects filtering:** Always project in SQL before `.ToList()`.
- ❌ **Multiple queries per aggregation:** Use `.Sum()`, `.Count()`, `.Any()` in the same `.Select()`.
- ❌ **Forgetting to `.ToListAsync()` after complex queries:** Ensures materialization and respects cancellation.
- ❌ **Cache TTL too short (<1 hour):** Defeats amortization; too long (>24h) risks stale data.
- ❌ **No cache invalidation strategy:** Document when/how to clear cache on data updates.

## Performance Notes

- **Memory savings:** NoTracking + projection = 15-20% lower memory than full entity tracking.
- **Amortized cost:** First request to filters hits DB 3-4x; subsequent 24h requests hit cache (1 DB hit for data).
- **Startup:** Background cache warmer adds ~500ms at app startup, but serves instant first-page renders.
- **Scaling:** Stateless pattern works horizontally; each instance has own IMemoryCache (eventual consistency acceptable for static data).

## Files in Baseball History

- `Program.cs` — NoTracking global config, cache/compression setup.
- `PlayerCacheService.cs` — cache warmer example (24h refresh, startup build).
- `Pages/Players/Index.cshtml.cs` — typical page model (projection, cache lookups, response cache).
- `Pages/Stats/Batting.cshtml.cs` — complex leaderboard with multi-filter projection.
- `Api/Endpoints/LeaderEndpoints.cs` — API endpoint with projection and DTO mapping.

## When to Apply

- **Read-only databases** (stats, archives, catalogs).
- **Static schema** (no migrations, no writes).
- **High-volume queries** on large historical datasets.
- **Performance-critical first-page renders** (cache warmer for hot paths).
