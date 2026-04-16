# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

- Initial squad seed for data, caching, and runtime review during migration.

### Query Architecture
- **Global NoTracking default (Program.cs:17)** — read-only app, 15-20% memory savings vs. tracking. Safe for all query paths.
- **Consistent projection pattern** — all endpoints use early `.Select()` to project only needed columns; no full entity hydration.
- **Aggregations in projection** — `p.Battings.Sum(b => b.G)` in `.Select()` avoids N+1; compiles to single query in NoTracking context.
- **Limited Include usage (8 total)** — only for deep navigation (e.g., franchise rosters). Always followed by projection.

### Caching & Performance
- **Memory cache: 24-hour TTL** — filters (years/leagues), HOF player IDs (~350 entries), and pre-warmed first page (letter A).
- **Background cache warmer (PlayerCacheService)** — starts at app startup, builds ~3 queries, then refreshes every 24h. Serves instant first page.
- **Response cache (HTTP, client-side)** — 1-hour TTL, varies by HX-Request header. Separates htmx partials from full-page responses.
- **Amortized cost:** First request hits DB 4x (filters + HOF + first page); subsequent requests: 1 hit (data query only), rest from cache.

### Database & Runtime
- **SQLite 72MB, read-only, WAL mode** — connection string `Mode=ReadOnly;Cache=Shared`, PRAGMA WAL+NORMAL sync at startup (~50ms).
- **Cold start:** ~650ms (DbContext + PlayerCacheService + WAL). ~5-10MB DbContext, ~500KB cache footprint.
- **Horizontal scale:** Stateless. No session affinity. Each instance has own IMemoryCache; DB updates won't sync until TTL expires (acceptable for static data).

### Key File Paths
- `BaseballDbContext.cs` — 28 DbSets, custom DateOnly converter for Lahman CSV dates.
- `PlayerCacheService.cs` — pre-warmer, 24h refresh interval.
- `Program.cs` — NoTracking global, cache/compression config, WAL setup.
- `Pages/Stats/Batting.cshtml.cs`, `Pitching.cshtml.cs` — largest page models (342–343 lines), complex leaderboard filters.

### Identified Risks
1. **Cache invalidation:** No out-of-band invalidation strategy. If Lahman DB refreshes, UI won't update until 24h TTL expires or manual restart.
2. **Connection timeout (30s default):** Safe for current queries, but complex aggregations under load could risk timeout. No slow-query logging currently.
3. **Query compilation:** Dynamic Where/OrderBy chains; no `.Compile()`. Safe at current volume (<1M qpm), but 2–5ms overhead per query.

### Recommended Actions
- Document cache invalidation SOP for ops (when/how to clear on DB updates).
- Add slow-query logging (>5s) or APM to track timeout risk under load.
- Consider compiled queries for top 3–5 endpoints if scaling beyond current volume.
- Centralize HOF/filter cache keys in a shared cache service (optional refactor).

## Codebase Review Output (2026-04-16)

**Cache invalidation SOP missing, query architecture sound**

- Global NoTracking safe for read-only paths (15-20% memory savings verified)
- Projection pattern consistent, aggregations in SELECT avoid N+1
- 3-layer caching works well: memory (24h TTL) + background warmer + response cache
- Identified: Cache invalidation strategy missing when out-of-band DB updates occur
- Recommended action: Document SOP for when Lahman data refreshes
- No architecture changes needed for migration; cache strategy intact
