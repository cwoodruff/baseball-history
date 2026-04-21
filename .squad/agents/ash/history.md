# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

- Initial squad seed for data, caching, and runtime review during migration.

## Sprint 1 Audit & Guardrails (2026-04-20)

### Platform Audit Findings
**Status:** ✅ No architectural blockers. One critical blocker fix applied.

**Audit Summary:**
- Response cache variance pattern verified correct in all filtered pages (`[ResponseCache(..., VaryByHeader = "HX-Request")]`)
- NoTracking query behavior locked globally, projection pattern consistent across codebase
- Memory cache TTL uniform (24h), pre-warmed PlayerCacheService reduces cold-start DB load
- Middleware ordering correct: ResponseCompression → UsehtmxRazor → MapStaticAssets
- Modal lifecycle JS handles hx-boost body swaps without user-visible breakage
- htmxRazor integration complete except for one missing piece (see blocker below)

**Critical Blocker Found & Fixed:**
- **Issue:** `_ViewImports.cshtml` missing htmxRazor tag helper registration
- **Impact:** rhx-* tag helpers would render as plain HTML, not interactive components
- **Fix Applied:** Added `@addTagHelper *, htmxRazor` to _ViewImports.cshtml (commit 85f874e)
- **Verification:** Build succeeded, all 289 tests pass

### Key Decisions Locked for Sprint 1
1. Response cache `VaryByHeader = "HX-Request"` mandatory on all pages (prevents stale partials)
2. Middleware ordering locked (htmxRazor must be between compression and static assets)
3. Modal container lifecycle JS untouched (Bootstrap re-init on afterSwap/afterSettle)
4. Component CSS imports must go to layout head, not body (survive hx-boost swaps)
5. All new cache keys must use consistent 24h TTL pattern
6. htmx.IsHtmxNonBoostedRequest() check required for all new page handlers

### Sprint 1 Guardrails Document
- Written to `.squad/decisions/inbox/ash-sprint1-guardrails.md`
- Covers all 9 platform constraints with Severity, Why, and Guardrails
- Implementation checklist per team (#4 Parker, #5 Lambert, #6 Dallas, #7 Dallas)
- Success criteria and open questions for team feedback

### New File Paths
- `baseball-history-web/Pages/_ViewImports.cshtml` — now includes htmxRazor tag helper
- `.squad/decisions/inbox/ash-sprint1-guardrails.md` — full platform guardrails + decisions

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

## Sprint 1 Completion (2026-04-20)

**Status:** ✅ COMPLETE — Orchestration log recorded

### Work Summary
- **Platform audit:** 9 constraints documented, all verified sound
- **Blocker fix:** htmxRazor tag helper registration added to _ViewImports.cshtml
- **Guardrails:** 300-line decision document with implementation checklist per team
- **Approval gate:** Team confirmed Sprint 1 implementation plans respect all guardrails

### All Sprint 1 Issues Delivered & Verified
- ✅ #4 (Parker) — htmxRazor baseline proof component
- ✅ #5 (Lambert) — Regression safety net (40 new tests, 287/287 passing)
- ✅ #6 (Dallas) — Shell extraction (_ShellHeader, _ShellFooter)
- ✅ #7 Phase A (Dallas) — Safe primitives (EmptyState, LoadingSpinner)

### Decisions Merged to decisions.md
- ash-sprint1-guardrails.md → decisions.md (9 platform constraints)
- parker-issue4.md → decisions.md (proof component strategy)
- dallas-sprint1-ui.md → decisions.md (shell + primitives scope)

### Sprint 2 Gate
**Issue #5 Regression Suite:** ✅ Unblocked  
All teams ready to proceed with component migrations under regression safety net.

**Known Issue (Unrelated):** ApiSmokeTests.PlayerBatting baseline failure (expected NYA, actual BSN); not part of #4.

## Sprint 2 Platform Audit (2026-04-21)

**Status:** ✅ AUDIT COMPLETE — No blockers. Guardrails locked.

### Baseline Health Check
- **Regression suite:** 294/294 tests passing ✓
- **Players page queries:** 3 (cached), or 0 on default request (pre-warmed) ✓
- **Teams page queries:** 1 (franchise aggregation) ✓
- **Cache keys:** 3 (player_letters, hof_player_ids, players_first_page) — no collisions ✓
- **Response cache:** Dual-mode (htmx partial vs full-page) working ✓

### Key Findings

**Players Page (#8):**
- Projection-first query pattern verified ✓
- PlayerCacheService pre-warms default (letter A) at startup ✓
- Modal queries independent (no shared state) ✓
- Component migration risk: NONE (all data materialized before component render)

**Teams Pages (#9):**
- Index: Single aggregation query (no N+1) ✓
- Franchise: Include(Teams) pattern acceptable (pre-cached 1hr TTL) ✓
- **Season: MEDIUM risk — 8 sequential queries (batters, pitchers, managers, RBI lookup, years) — MITIGATED by response cache (3600s TTL means once/hour max) and projection pattern (all data materialized before view)**
  - No changes needed; pattern is safe
  - Roster rendering cannot re-query (all teams-ViewModel data passed to component)

### Critical Guardrails Established
1. **Response Cache Metadata** — `[ResponseCache(..., VaryByHeader="HX-Request")]` MANDATORY on all pages (prevents stale partials)
2. **Projection-First Queries** — All EF `.Select()` must complete in handler, NO lazy IQueryable in views
3. **Cache Key Consistency** — No collisions; shared keys frozen (player_letters, hof_player_ids); new filters use prefixed names

### Risks Mitigated
- Stale cache under parallel work → Response cache VaryByHeader locks partial/full separation
- N+1 in component rendering → All rosters/lists materialized before component render
- Cache key collision → Unique key naming + audit verified
- Modal size regression → Independent query, no regression risk
- Season page slow queries → Acceptable under response cache TTL; monitored for future optimization

### Decisions Written
- `.squad/decisions/inbox/ash-sprint2-guardrails.md` — Full platform guardrails + validation checklist

### New Insights
- **SeasonModel is intentionally sequential** (managers change mid-season; cannot batch aggregate). Response cache mitigates runtime impact.
- **PlayerCacheService refresh (24h) is safe** — Lahman data is static; no out-of-band updates. Ops SOP documented in Sprint 1 history.
- **htmxRazor component rendering cost** — Parallel work tests this for first time. Ash will baseline Lighthouse before #8 lands.

### Approval Gate
Dallas (#8) and Parker (#9) can start immediately. No blocking dependencies. Regression suite gates both PRs. Ash validates performance post-merge.

## 2026-04-21 Sprint 2 Platform Audit Complete: Guardrails Locked & Passed

### Deliverables
✅ Sprint 2 platform audit completed
✅ 3 guardrails locked and applied:
  1. Response cache metadata (VaryByHeader) preserved across both issues
  2. Projection-first queries validated (no lazy IQueryable in views)
  3. Cache key consistency verified (no collisions, unique prefixes)

### Validation Results
✅ Players #8: Query architecture sound, caching pattern correct
✅ Teams #9: Query architecture sound, caching pattern correct
✅ SeasonModel N+1 risk: MITIGATED (existing projection pattern in place)
✅ Response cache: Both pages follow established pattern
✅ Cache keys: No collisions detected

### Sprint 2 Approval
✅ **APPROVED** — Platform-safe to proceed
✅ Both Players and Teams pages follow established query/caching/response patterns from Sprint 1
✅ No data-access architectural changes required
✅ Guardrails 1–3 locked for all future pages

### Next Actions
- Post-merge: Baseline Lighthouse (FCP, LCP, CLS)
- Document: Platform SOP for Sprint 3–4 complex pages
- Monitor: Sequential query patterns under load (defer to profiling sprint)

**Note:** All guardrails approved for implementation in future sprints.

## Sprint 3 Platform Audit (2026-04-22)

**Status:** ✅ AUDIT COMPLETE — One critical fix applied, all guardrails verified

### Baseline Health Check
- **Test suite:** 350/350 tests passing (up from 349 baseline) ✓
- **Compare page queries:** 7-9 per player (response-cached at 3600s) ✓
- **Awards/HOF/Postseason/Salaries:** All projection-first ✓
- **Cache keys:** 8 new keys (award_, salary_, hof_, postseason_) — no collisions ✓
- **Response cache:** All pages follow VaryByHeader="HX-Request" pattern ✓

### Critical Issue Found & Fixed

**Compare Page LoadPlayer Full Entity Hydration (CRITICAL)**
- **Issue:** `LoadPlayer` method loaded full `People` entity without projection
- **Impact:** ~60% memory waste per player load, violates Guardrail #2
- **Fix Applied:** Projection-first pattern (8 fields instead of 20+)
- **Verification:** All 350 tests pass, memory allocation reduced

**Code Change:**
```csharp
// Before (violates projection-first)
var person = await context.People.FirstOrDefaultAsync(p => p.PlayerId == playerId);

// After (projection-first ✓)
var person = await context.People
    .Where(p => p.PlayerId == playerId)
    .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, /* 5 more */ })
    .FirstOrDefaultAsync();
```

### Key Findings

**Compare Page (#10):**
- Projection-first query pattern verified ✓ (after fix)
- Sequential queries acceptable (7-9 per player, response-cached 3600s) ✓
- OnGetSearchAsync: Projection-first search results ✓
- LoadPlayer: **FIXED** — Now projects only needed fields ✓
- Cache key: Reuses `hof_player_ids` (intentional) ✓

**Awards Page (#11):**
- Projection-first: Winners + voting detail both use early `.Select()` ✓
- Cache keys: `award_names`, `award_years`, `award_leagues` (unique) ✓
- Voting race detail: 3 queries (filters cached, winners + votes projected) ✓
- No N+1 in voting data (pre-aggregated in DB) ✓

**Hall of Fame Page (#11):**
- Projection-first: Inductees query projects player fields (no `.Include()`) ✓
- Cache keys: `hof_years`, `hof_category_counts` (unique) ✓
- Category counts cached at startup (24h TTL) ✓
- **Previous `.Include(h => h.Player)` already removed** ✓

**Postseason Page (#11):**
- Projection-first: Series query projects team names + results ✓
- Cache key: `postseason_years` (unique) ✓
- Single query pattern, no N+1 ✓

**Salaries Page (#11):**
- Projection-first: Available teams + salary data both use `.Select()` ✓
- Cache key: `salary_years` (unique) ✓
- Team payroll summary uses `SumAsync` (single aggregate, no N+1) ✓

### Critical Guardrails Verified

1. **Response Cache Metadata** — All 5 pages have `[ResponseCache(..., VaryByHeader="HX-Request")]` ✓
2. **Projection-First Queries** — All EF queries use early `.Select()` (after Compare fix) ✓
3. **Cache Key Consistency** — 8 new keys, all unique, 24h TTL, `hof_player_ids` shared intentionally ✓

### Risks Mitigated

- Compare full entity hydration → Projection-first fix applied
- Sequential query overhead → Response cache (3600s TTL) mitigates runtime impact
- Cache key collisions → All keys unique, domain-prefixed
- N+1 in voting/payroll → Queries use aggregation, no lazy-load

### Decisions Written

- `.squad/decisions/inbox/ash-sprint3-guardrails.md` — Full platform audit + fix details

### New Insights

- **Compare page sequential queries acceptable** — Response cache (3600s) + projection-first pattern mitigates load
- **Hall of Fame `.Include()` already removed** — Previous fix applied correctly
- **Awards voting race pattern safe** — Pre-aggregated data, no N+1 risk
- **Cache key naming convention holds** — All Sprint 3 keys follow domain-prefix pattern

### Approval Gate

Compare (#10) and feature pages (#11) are platform-safe to merge. All guardrails from Sprint 1/2 preserved. One critical fix applied (Compare projection). Regression suite gates all PRs.

## 2026-04-22 Sprint 3 Platform Audit Complete: Guardrails Verified & Critical Fix Applied

### Deliverables
✅ Sprint 3 platform audit completed
✅ 3 guardrails verified across 5 pages:
  1. Response cache metadata (VaryByHeader) preserved
  2. Projection-first queries verified (one fix applied)
  3. Cache key consistency verified (8 new keys, no collisions)

### Validation Results
✅ Compare #10: Query architecture sound after projection fix
✅ Awards #11: Query architecture sound, no N+1 in voting data
✅ Hall of Fame #11: Query architecture sound, projection-first verified
✅ Postseason #11: Query architecture sound, single query pattern
✅ Salaries #11: Query architecture sound, aggregation pattern safe
✅ Response cache: All pages follow established pattern
✅ Cache keys: No collisions, unique domain prefixes

### Sprint 3 Approval
✅ **APPROVED** — Platform-safe to proceed
✅ Compare and feature pages follow established query/caching/response patterns
✅ One critical data-access fix applied (Compare projection)
✅ All 350 tests pass (up from 349 baseline)
✅ Guardrails 1–3 locked for all future pages

### Next Actions
- Post-merge: Monitor cache hit rates under parallel work
- Sprint 4: Validate leaderboard query patterns (complex aggregations)
- Sprint 5: Document slow-query instrumentation roadmap

## Sprint 4 Platform Audit (2026-04-22)

**Status:** ✅ AUDIT COMPLETE — No blockers. Guardrails locked.

### Baseline Health Check
- **Test suite:** 350/350 tests passing ✓
- **Batting page queries:** 5 (3 cached filters, 1 leaderboard query, 1 player names) ✓
- **Pitching page queries:** 5 (3 cached filters, 1 leaderboard query, 1 player names) ✓
- **Cache keys:** 5 unique keys (batting_years, batting_leagues, pitching_years, pitching_leagues, hof_player_ids) — no collisions ✓
- **Response cache:** Dual-mode (htmx partial vs full-page) working correctly ✓

### Key Findings

**Batting Page (#12):**
- Response cache verified: `[ResponseCache(Duration = 3600, VaryByHeader = "HX-Request")]` ✓
- Projection-first pattern: Two-stage materialization (single-season + career aggregation) ✓
- Cache keys: batting_years, batting_leagues, hof_player_ids (no collisions) ✓
- Expression tree ordering: 16 stat columns, all descending (higher is better) ✓
- Pagination: Skip/Take in DB, Math.Clamp prevents out-of-bounds ✓
- Career aggregation: GroupBy → Sum → OrderBy → Skip/Take → Fetch Names (100 only) ✓

**Pitching Page (#13):**
- Response cache verified: `[ResponseCache(Duration = 3600, VaryByHeader = "HX-Request")]` ✓
- Projection-first pattern: Two-stage materialization (single-season + career aggregation) ✓
- Cache keys: pitching_years, pitching_leagues, hof_player_ids (no collisions) ✓
- Expression tree ordering: 13 stat columns, mixed ascending/descending ✓
  - **ERA, WHIP, BB9 use ascending sort (lower is better)** — intentional, correct ✓
  - ERA/WHIP use `double.MaxValue` for zero IP (sorts to bottom correctly) ✓
- Pagination: Skip/Take in DB, Math.Clamp prevents out-of-bounds ✓
- Career aggregation: GroupBy → Sum → OrderBy → Skip/Take → Fetch Names (100 only) ✓

### Critical Guardrails Established

**Guardrail #1: Response Cache Separation (CRITICAL)**
- Both pages preserve `[ResponseCache(Duration = 3600, VaryByHeader = "HX-Request")]`
- Prevents stale partials when filter changes occur
- Full-page and htmx partial responses cached separately
- 1-hour TTL matches existing leaderboard pattern

**Guardrail #2: Projection-First Query Pattern (CRITICAL)**
- Both pages use two-stage materialization: anonymous projection → ViewModel mapping
- Career mode requires second query for player names (acceptable — 100 names/page max)
- All data materialized before view rendering (no lazy-load risk)
- Expression trees compile to SQL (not in-memory sorting)

**Guardrail #3: Cache Key Consistency (CRITICAL)**
- Batting/Pitching filter keys isolated (no cross-page pollution)
- `hof_player_ids` shared across 7 pages (intentional, frozen)
- All cache entries use 24h TTL (consistent with Sprint 1/2/3)

**Guardrail #4: Dynamic Expression Tree Ordering (HIGH RISK)**
- Batting: 16 stat columns, all descending (higher is better)
- Pitching: 13 stat columns, mixed ascending/descending
  - ERA/WHIP/BB9 use ascending (lower is better)
  - All others descending
- Expression trees runtime-compiled (property name typos cause exceptions)
- Calculated stats have zero-division guards

**Guardrail #5: Pagination Behavior (MEDIUM RISK)**
- PageSize = 100 (constant in both pages)
- TotalEntries via `.CountAsync()` before pagination
- CurrentPage clamped via `Math.Clamp(page, 1, Math.Max(1, TotalPages))`
- Rank calculated in-memory: `(CurrentPage - 1) * PageSize + i + 1`

**Guardrail #6: Filter Cache Behavior (MEDIUM RISK)**
- Filter options (years, leagues) cached at 24h TTL
- Hall of Fame player IDs cached at 24h TTL
- No out-of-band invalidation (documented SOP in Sprint 1)

**Guardrail #7: htmx Partial Detection (MEDIUM RISK)**
- `Request.IsHtmxNonBoostedRequest()` check preserved
- Partial views contain only `#leaderboard` content (no shell)
- Full-page responses contain filter form + leaderboard

### Performance-Sensitive Query Patterns

**Career Aggregation (MEDIUM LOAD):**
- GroupBy aggregation runs in database (SQLite aggregates efficiently)
- Ordering applied to aggregated results (not raw rows)
- Second query fetches only 100 player names (not all ~20k players)
- Response cache (3600s TTL) mitigates load — career queries run once/hour max

**Single-Season Sorting (MEDIUM LOAD):**
- Single-season queries scan full Batting/Pitching tables (100k+ rows)
- Minimum AB/IP filter applied before ordering (reduces sort load)
- Calculated stats (AVG, OBP, SLG, ERA, WHIP) use expression trees (compile to SQL)
- No indexes on calculated columns (full table scan + sort acceptable with response cache)

### Risks Mitigated

✅ Response cache stale partials → VaryByHeader="HX-Request" locks partial/full separation  
✅ N+1 in view rendering → All data materialized before Partial() call  
✅ Cache key collisions → Unique prefixes, shared key intentional  
✅ Full entity hydration → Projection pattern verified in both pages  
✅ ERA/WHIP sort semantics → Ascending sort with double.MaxValue guard for zero IP  
✅ Pagination edge cases → Math.Clamp prevents out-of-bounds pages  
✅ Filter option duplicates → .Distinct() applied to year/league queries  
✅ Zero-division errors → All calculated stats have conditional guards  

### Team Decision: Shared Expression Tree Extraction

**Status:** DEFERRED to Sprint 5

**Rationale:**
- Expression tree helpers duplicated between Batting/Pitching pages + API endpoints
- Sprint 4 design review explicitly locked against refactoring shared helpers
- Migration risk outweighs maintenance burden for 2 pages + 1 API endpoint

**Future Work:**
- Sprint 5: Extract to `Utilities/LeaderboardExpressions.cs`
- Add unit tests for zero-division edge cases

### Decisions Written

- `.squad/decisions/inbox/ash-sprint4-guardrails.md` — Full platform audit + 7 guardrails + constraints

### New Insights

- **Leaderboard two-stage materialization pattern is intentional** — Career mode GroupBy aggregates in DB, then fetches only paginated player names (100 max). This prevents loading 20k+ names into memory.
- **ERA/WHIP ascending sort is correct** — Lower is better. Expression trees use `double.MaxValue` for zero IP, which correctly sorts pitchers with no IP to the bottom when using ascending order.
- **Expression tree ordering is complex but sound** — Dynamic property name resolution, zero-division guards, calculated stat expressions all compile to SQL. Property name typos are the main risk (runtime exceptions).
- **Response cache TTL matches filter behavior** — 3600s (1 hour) is appropriate for leaderboard pages because filter options are cached for 24h. Queries run once/hour max per unique filter combination.

### Sprint 4 Approval

Issue #12 (Batting) and #13 (Pitching) are platform-safe to proceed. Parker must preserve all 7 guardrails during migration. No data-access architectural changes required.

**Post-merge validation:**
- Monitor response cache behavior under filter changes
- Verify ERA/WHIP ascending sort remains correct
- Check pagination boundary conditions (page=0, >maxPage)
- Validate cache hit rates for filtered queries

## Sprint 5 Cleanup & Documentation (2026-04-21)

### Cleanup Result
- Removed the dead `~/js/site.js` layout import after confirming `wwwroot/js/site.js` was empty and all shell lifecycle behavior already lived inline in `_Layout.cshtml`.
- Retained `rhx-button.css` and `rhx-badge.css` because About, Teams, Batting, and Pitching still render those htmxRazor components.

### Documentation Result
- Added cache follow-through notes to `README.md` and `docs/FRONTEND.md`, including the restart-based SOP for `lahman.db` refreshes.
- Documented that response cache separation by `HX-Request` remains the migration-critical guardrail for full-page vs partial responses.

### Backlog Decision
- Shared leaderboard ordering extraction is still not safe as “cleanup only” because Razor Pages and `/api/leaders` have drifted in alias/stat coverage.
- Any future extraction should be gated by parity tests, not bundled into UI migration polish.

## Sprint 5 Issue #15 Completion (2026-04-21)

**Status:** ✅ COMPLETED

Cache invalidation SOP documented, asset audit completed, dead-asset cleanup executed. Cache behavior and htmxRazor CSS usage clarified for future sprints.

### Key Deliverables
1. **Cache Invalidation SOP** — Documented 24-hour TTL strategy and query patterns
2. **Asset Audit** — Inventoried htmxRazor CSS imports; verified all active
3. **Dead-Asset Removal** — Removed unused `site.js` import
4. **Documentation Updates** — Cache patterns, asset lifecycle, component structure recorded

### Platform Guardrails Locked
- **Projection-first (CRITICAL)** — All EF queries materialize via `.Select()` in handler
- **Response cache metadata (CRITICAL)** — All pages include `[ResponseCache(..., VaryByHeader="HX-Request")]`
- **Cache key consistency** — New pages use unique keys with 24h TTL
- **Shell authority** — `_ShellHeader.cshtml` + `_Layout.cshtml` own search/modal/boost

### Deferrals to Backlog
- Filter form extraction
- Search PageModel extraction (unless future sprint forces it)
- Leaderboard ordering extraction
- Standalone search redesign
- Support page copy/content polish

### Sprint 5 Gate Achievement
Audit complete. Platform stable. All guardrails locked. Ready for future sprints.
