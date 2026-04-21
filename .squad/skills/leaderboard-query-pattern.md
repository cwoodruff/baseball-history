# Leaderboard Query Pattern

## Context
ASP.NET Core Razor Pages with Entity Framework Core, using dynamic expression trees for sortable leaderboard queries with both single-season and career aggregation modes.

## Pattern Overview
Two-stage materialization pattern for efficient leaderboard rendering with pagination, caching, and calculated stat ordering.

## When to Use
- Sortable data tables with 10+ stat columns
- Dual-mode queries (single-season vs career aggregation)
- Calculated stats that require zero-division guards (AVG, ERA, OPS, WHIP)
- Pagination with count-before-skip pattern
- Response caching with htmx partial support

## Structure

### 1. Response Cache with htmx Separation
```csharp
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class LeaderboardModel(BaseballDbContext context, IMemoryCache cache) : PageModel
```

**Why:**
- Separates full-page and htmx partial responses in cache
- 1-hour TTL for leaderboard queries (matches filter cache behavior)
- Client-side caching reduces server load

### 2. Filter Cache (24h TTL)
```csharp
var availableYears = await cache.GetOrCreateAsync("leaderboard_years", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
    return await context.Stats
        .Select(s => (int)s.YearId)
        .Distinct()
        .OrderByDescending(y => y)
        .ToListAsync();
});
```

**Why:**
- Filter options rarely change (Lahman data is static)
- Reduces DB hits from 3-5 per request to 0-1 per request
- Consistent with established caching strategy

### 3. Single-Season Query Path
```csharp
var seasonQuery = query
    .Where(s => s.MinThreshold >= minValue)
    .Select(s => new
    {
        s.PlayerId,
        PlayerName = (s.Player.NameFirst ?? "") + " " + (s.Player.NameLast ?? ""),
        s.YearId,
        s.TeamId,
        TeamName = s.Team.Name,
        Stat1 = s.Stat1 ?? 0,
        Stat2 = s.Stat2 ?? 0
        // ... all stats needed for sorting/display
    });

ViewModel.TotalEntries = await seasonQuery.CountAsync();
ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalEntries / PageSize);
ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

var orderedQuery = ApplyOrdering(seasonQuery, statColumn);

var seasonData = await orderedQuery
    .Skip((ViewModel.CurrentPage - 1) * PageSize)
    .Take(PageSize)
    .ToListAsync();
```

**Why:**
- Early projection reduces data transfer
- Count before pagination (consistent page totals)
- Clamp prevents out-of-bounds pages
- Skip/Take in database (efficient pagination)

### 4. Career Aggregation Query Path
```csharp
var careerQuery = query
    .GroupBy(s => s.PlayerId)
    .Select(g => new
    {
        PlayerId = g.Key,
        Stat1 = g.Sum(s => s.Stat1 ?? 0),
        Stat2 = g.Sum(s => s.Stat2 ?? 0)
        // ... all stats needed for sorting/display
    })
    .Where(x => x.MinThreshold >= minValue);

ViewModel.TotalEntries = await careerQuery.CountAsync();
ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalEntries / PageSize);
ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

var orderedCareerQuery = ApplyOrdering(careerQuery, statColumn);

var careerData = await orderedCareerQuery
    .Skip((ViewModel.CurrentPage - 1) * PageSize)
    .Take(PageSize)
    .ToListAsync();

// Second query: Fetch player names ONLY for current page (100 max, not 20k)
var playerIds = careerData.Select(c => c.PlayerId).ToList();
var players = await context.People
    .Where(p => playerIds.Contains(p.PlayerId))
    .ToDictionaryAsync(p => p.PlayerId, p => (p.NameFirst ?? "") + " " + (p.NameLast ?? ""));
```

**Why:**
- GroupBy aggregation runs in database (efficient)
- Second query fetches only paginated names (100 max, not 20k+)
- Avoids N+1 and full entity hydration
- Consistent count/clamp/skip/take pattern

### 5. Dynamic Expression Tree Ordering
```csharp
private static IOrderedQueryable<T> ApplyOrdering<T>(IQueryable<T> query, string stat, bool ascending = false)
{
    if (ascending)
    {
        return stat.ToLower() switch
        {
            "era" => query.OrderBy(BuildEraExpression<T>()),
            "whip" => query.OrderBy(BuildWhipExpression<T>()),
            _ => query.OrderBy(BuildPropertyExpression<T>("DefaultStat"))
        };
    }

    return stat.ToLower() switch
    {
        "hr" => query.OrderByDescending(BuildPropertyExpression<T>("HR")),
        "avg" => query.OrderByDescending(BuildAvgExpression<T>()),
        _ => query.OrderByDescending(BuildPropertyExpression<T>("DefaultStat"))
    };
}

private static Expression<Func<T, int>> BuildPropertyExpression<T>(string propName)
{
    var param = Expression.Parameter(typeof(T), "x");
    var prop = Expression.Property(param, propName);
    return Expression.Lambda<Func<T, int>>(prop, param);
}

private static Expression<Func<T, double>> BuildAvgExpression<T>()
{
    var param = Expression.Parameter(typeof(T), "x");
    var hits = Expression.Convert(Expression.Property(param, "H"), typeof(double));
    var atBats = Expression.Convert(Expression.Property(param, "AB"), typeof(double));
    var zero = Expression.Constant(0.0);
    var denomIsZero = Expression.Equal(atBats, zero);
    var division = Expression.Divide(hits, atBats);
    var body = Expression.Condition(denomIsZero, zero, division);
    return Expression.Lambda<Func<T, double>>(body, param);
}

private static Expression<Func<T, double>> BuildEraExpression<T>()
{
    // ERA = (ER * 27.0) / IPOuts
    var param = Expression.Parameter(typeof(T), "x");
    var er = Expression.Convert(Expression.Property(param, "ER"), typeof(double));
    var ipouts = Expression.Convert(Expression.Property(param, "IPOuts"), typeof(double));
    var twentySeven = Expression.Constant(27.0);
    var num = Expression.Multiply(er, twentySeven);
    var zero = Expression.Constant(0.0);
    var denomIsZero = Expression.Equal(ipouts, zero);
    var division = Expression.Divide(num, ipouts);
    // Use double.MaxValue for zero IP — sorts to bottom when ascending
    var body = Expression.Condition(denomIsZero, Expression.Constant(double.MaxValue), division);
    return Expression.Lambda<Func<T, double>>(body, param);
}
```

**Why:**
- Expression trees compile to SQL (not in-memory sorting)
- Zero-division guards prevent runtime errors
- Property name string resolution allows dynamic ordering
- Ascending/descending switch supports stats where lower is better (ERA, WHIP)

### 6. ViewModel Mapping
```csharp
ViewModel.Leaders = data
    .Select((item, index) => new LeaderEntry
    {
        Rank = (ViewModel.CurrentPage - 1) * PageSize + index + 1,
        PlayerId = item.PlayerId,
        PlayerName = players.GetValueOrDefault(item.PlayerId, item.PlayerId), // Career mode
        // or PlayerName = item.PlayerName, // Single-season mode
        Stat1 = item.Stat1,
        Stat2 = item.Stat2
    })
    .ToList();
```

**Why:**
- Rank calculated with page offset
- Mapping happens in-memory after materialization
- No lazy-load risk in views

### 7. htmx Partial Detection
```csharp
if (Request.IsHtmxNonBoostedRequest())
{
    return Partial("_LeaderboardResults", ViewModel);
}
return Page();
```

**Why:**
- htmx requests get partial view only (no shell)
- Full-page requests get filter form + results
- Response cache separates via `VaryByHeader = "HX-Request"`

## Critical Gotchas

### 1. Expression Tree Property Names
**Risk:** Property name typos cause runtime exceptions (not compile-time)
**Mitigation:** 
- Use constants for property names
- Add unit tests for all stat columns
- Verify property names match anonymous type projection

### 2. Zero-Division Guards
**Risk:** Calculated stats (AVG, ERA, OPS, WHIP) can divide by zero
**Mitigation:**
- Always use `Expression.Condition(denomIsZero, zero, division)` pattern
- For ascending stats (ERA, WHIP), use `double.MaxValue` for zero denominator (sorts to bottom)
- For descending stats, use `0.0` for zero denominator

### 3. Two-Query Career Pattern
**Risk:** Loading all player names before pagination wastes memory
**Mitigation:**
- Fetch player names AFTER pagination (only 100 names per page)
- Use `.Select(c => c.PlayerId).ToList()` to materialize IDs first
- Use `.ToDictionaryAsync()` for efficient name lookup

### 4. Cache Key Collisions
**Risk:** Different pages using same cache key names
**Mitigation:**
- Use domain-prefixed keys (batting_years, pitching_years)
- Document intentionally shared keys (hof_player_ids)
- Never use generic keys (years, leagues)

### 5. Pagination Rank Calculation
**Risk:** Rank resets to 1 on each page
**Mitigation:**
- Always use: `Rank = (CurrentPage - 1) * PageSize + index + 1`
- Never use: `Rank = index + 1`

## Performance Characteristics

### Single-Season Mode
- **DB Queries:** 3-5 (3 cached filters, 1 leaderboard query, 0-1 HOF IDs)
- **Rows Scanned:** 100k+ (full table scan with filter)
- **Sort Load:** Medium (calculated stats compile to SQL expressions)
- **Memory:** Low (~10KB per page of 100 entries)
- **Response Time:** 50-200ms (mitigated by response cache 3600s TTL)

### Career Mode
- **DB Queries:** 4-6 (3 cached filters, 1 aggregation query, 1 player names, 0-1 HOF IDs)
- **Rows Scanned:** 100k+ (GroupBy aggregation)
- **Sort Load:** Medium (calculated stats on aggregated results)
- **Memory:** Low (~10KB per page of 100 entries)
- **Response Time:** 100-300ms (mitigated by response cache 3600s TTL)

## Testing Strategy

### Unit Tests
- Zero-division edge cases (zero AB, zero IP, zero W+L)
- Expression tree property name validation
- Ascending vs descending sort logic
- Rank calculation with pagination

### Integration Tests
- Pagination boundaries (page=0, negative, >maxpage)
- Career vs single-season mode switching
- Filter combinations (year range + league + minimum threshold)
- Cache hit behavior (first request vs subsequent)

### Smoke Tests
- Each stat column orders correctly
- ERA/WHIP ascending sort (lower is better)
- Player names match IDs in career mode
- htmx partial vs full-page responses

## References
- Baseball History Batting: `baseball-history-web/Pages/Stats/Batting.cshtml.cs`
- Baseball History Pitching: `baseball-history-web/Pages/Stats/Pitching.cshtml.cs`
- Sprint 4 Platform Audit: `.squad/decisions/inbox/ash-sprint4-guardrails.md`
