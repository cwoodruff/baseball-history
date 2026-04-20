# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

- Initial squad seed for data, caching, and runtime review during migration.

## Sprint Planning (2026-04-20)

### Corrected Sprint Milestone Plan Rationale
- **Why 5 sprints?** Issues #4–#15 grouped by flow dependency + data coherence gates. Sprint 1 gates Sprints 2+. Sprints 2–4 strictly ordered (pattern stability → learning application). Sprint 5 overlaps but documents Platform decisions.
- **Why #16 unassigned?** Meta-tracking issue; links to all sprints but doesn't need its own milestone (avoids scope pollution).
- **Data/Platform constraints embedded:** Cache strategy stability (#7) gates feature work (#8+). Response cache keys verified after each sprint. Leaderboard query regression instrumentation deferred to Sprint 5 docs.
- **Blocker acknowledged:** Lambert flagged #5 regression suite missing; Sprint 2 gate explicitly requires #5 completion before feature pages start.
- **Risk mitigations by sprint:** Sprint 1 (#5 blocker), Sprint 2 (cache hit rates + pagination expression trees), Sprint 3 (filter aggregation queries), Sprint 4 (leaderboard query overhead), Sprint 5 (slow-query instrumentation roadmap + cache invalidation SOP).

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

## Team Update: Sprint Milestone Planning (2026-04-20)

**Status:** ✅ APPROVED & ADOPTED

**Ash's Contribution:**
- Produced corrected 5-sprint milestone plan addressing Ripley's factual baseline error
- Plan restructured to reflect actual GitHub state (Sprint 1 in progress, not complete)
- Integrated Lambert's blocker constraints (#5 regression suite gates Sprint 2 entry)
- Documented data/platform risk mitigations for each sprint

**Approved Milestone Structure (Ash's Plan):**
1. Sprint 1 — Foundation & Regression Gates (#4–#7)
2. Sprint 2 — Foundation Pages (#8–#9)
3. Sprint 3 — Comparison & Features (#10–#11)
4. Sprint 4 — Leaderboard Pages (#12–#13)
5. Sprint 5 — Polish & Documentation (#14–#15)
6. #16 remains outside milestones (umbrella tracking linked to all sprints)

**Platform Decisions Documented in Plan:**
- Cache coherence strategy reviewed per sprint (#7 filter cache → #8/#9 alpha cache → #11–#13 response cache keys)
- Query regression gates (#5 regression suite → #12/#13 leaderboard profiling)
- Expression tree refactoring decision deferred to Sprint 5 (after leaderboard behavior locked in)
- Slow-query instrumentation roadmap defined post-Sprint 4

**Verified by Lambert:** All constraints satisfied, blocker logic sound, platform concerns comprehensive.

**Next Steps:**
- Scribe: Create 5 GitHub milestones with issue assignments
- Team: Confirm Sprint 1 patterns stable before Sprint 2 kickoff
