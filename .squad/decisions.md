# Squad Decisions

## Codebase Review Findings (2026-04-16)

### Architecture Review — Ripley

**Status:** ✅ Well-structured and migration-ready. No architectural blockers for htmxRazor migration.

**Key Strengths:**
- Razor Pages + htmx foundation is production-proven (not speculative)
- Data access discipline: Global `QueryTrackingBehavior.NoTracking`, projection pattern consistent
- Caching strategy is thoughtful: 24-hour memory cache TTL, pre-warmed player cache, `[ResponseCache]` properly uses `VaryByHeader = "HX-Request"`
- Service architecture clean: Only 2 services (TeamColorService singleton, PlayerCacheService hosted), both follow SRP
- ViewModels provide clean data shapes with factory methods
- Database schema normalized, 28+ DbSets properly configured

**Migration Path:** Page-by-page rollout (not component-by-shared-component)
1. Extract shared components → all pages use new versions
2. Migrate Shared.Layouts, Footer, Navigation (app chrome)
3. Migrate Players page (highest traffic, lowest complexity)
4. Migrate Teams, Compare, Awards, Salaries (mid-tier)
5. Migrate Stats/Leaders (highest complexity, defer to maturity)

**Risks (Minor):**
- Leaderboard expression trees duplicated (API vs Pages) — maintenance burden, but isolated. Extract to shared utility later (not blocking)
- Page model sizes growing (largest 246 lines) — spike on service extraction post-migration
- No API client tests — optional, low priority
- String fields in database need parsing (Batting.Rbi, Pitching.Hr) — document in DATABASE.md

**Team Readiness:** All well-chartered. Ready to begin.

---

### UI Architecture & htmx Patterns — Dallas

**Status:** ✅ Strong component-based architecture. Biggest win is extracting shared filter/loading components.

**Strengths:**
- 8 reusable shared components in `Pages/Shared/Components/` with clean composition
- Consistent htmx request handling: `Request.IsHtmxNonBoostedRequest()` used throughout
- Professional CSS architecture: single `site.css` with variables, team-color system

**HIGH PRIORITY: Filter Form Component Duplication**
- **Impact:** 3 pages (Batting, Pitching, Awards) + 2 variants (HallOfFame, Postseason)
- **Issue:** Each rebuilds identical filter-select patterns, repeated htmx attributes
- **Recommendation:** Extract to reusable `_FilterForm.cshtml` component with slots for fields
- **Effort:** 2-3 hours

**MEDIUM PRIORITY: Duplication**
1. **Compare page player selection cards:** ~120 lines duplicated (extract `_ComparePlayerCard.cshtml` with side parameter)
2. **Loading indicator:** 5+ pages use custom overlay — standardize with `_LoadingOverlay.cshtml`

**LOW PRIORITY:**
- **Sortable table headers:** Batting/Pitching leaderboards have repetitive column links (could extract `_SortableTableHeader.cshtml`)

**Quick Wins (2-3 hours):**
1. Extract `_FilterForm.cshtml`
2. Extract `_LoadingOverlay.cshtml`

---

### Backend Structure & htmx Caching — Parker

**Status:** ✅ Clean handler patterns, projection-first queries, htmx-aware caching.

**Key Findings:**
- All 19 PageModel handlers are `OnGetAsync()` with primary constructor injection
- Partial/full logic decoupled via `Request.IsHtmxNonBoostedRequest()` extension
- Projection-first query execution with `.Select()` throughout, global `NoTracking` behavior
- `[ResponseCache]` with `VaryByHeader = "HX-Request"` — htmx partials cached separately from full pages
- Pre-warmed cache via `PlayerCacheService` at startup

**Duplication Note:** Leaderboard ordering expression trees exist in both `Api/Endpoints/` and Razor PageModels. Not a migration blocker but consider extracting to shared helper later.

**Confidence:** High. Backend seams are clean, minimal coupling, well-tested at integration level. Ready for UI migration.

**Next:** Frontend can design htmxRazor component boundaries without backend concerns. API layer needs no changes. Keep three-tier caching strategy intact during view layer refactoring.

---

### Test Coverage & Regression Risk — Lambert

**Status:** ⚠️ 247 tests passing, but critical gaps in page model/API coverage raise migration regression risk.

**What's Well Tested:**
- ✅ Database layer: 30+ integration tests covering all major DbSets, FK navigation verified, DateOnly converter tested
- ✅ htmx extensions: 21 comprehensive tests for request detection and response headers
- ✅ Selected ViewModels: PlayerDetailViewModel, LeaderboardViewModel, TeamSeasonViewModel tested

**Critical Gaps (Zero Coverage):**
- 🔴 **Page models:** 0/19 tested (Search, Players, Stats, Teams, Awards, HoF, Salaries, Postseason, Compare)
- 🔴 **API endpoints:** 0/20 tested (Result.NotFound() conditions untested)
- 🔴 **ViewModels:** 9 untested (AlphabetNav, AwardVoting, Compare, HallOfFame, LeaderboardVM, PlayerList, Postseason, Salary, TeamList)

**Edge Cases Not Verified:**
- Pagination boundaries (page=0, negative, >maxpage)
- Sort/filter stability (career vs single-season, null value handling)
- htmx request routing (partials vs full pages)
- Cache behavior with [ResponseCache] + VaryByHeader
- Service dependencies (TeamColorService aliases, PlayerCacheService)

**Regression Risk:** If query filtering changes → pagination regressions untested. If partial rendering changes → no tests verify htmx correctness. If sort expressions change → silent data reordering in production.

**Safest Path Before Migration:**
1. Add smoke tests for all 19 page handlers
2. Add integration tests for 5 highest-traffic endpoints (Players, Search, Leaderboards, Teams, Compare)
3. Add pagination edge-case tests (0, -1, >maxpage)
4. Add NotFound path tests for all API endpoints

**Recommendation:** Prioritize integration tests before next migration phase.

---

### Data Platform Review & Runtime Behavior — Ash

**Status:** ✅ Read-only query discipline sound, thoughtful caching; cache invalidation SOP missing.

**Query Architecture:**
- **Global NoTracking:** 15-20% memory savings vs tracking, safe for all read-only paths
- **Consistent projection pattern:** Early `.Select()` to project needed columns, no full entity hydration
- **Aggregations in projection:** `p.Battings.Sum(b => b.G)` in `.Select()` avoids N+1, compiles to single SQL query
- **Limited Include usage:** Only 8 total, always for deep navigation, followed by projection

**Caching (3-Layer Strategy):**
1. **Memory cache:** 24h TTL on filter options (years/leagues), HOF player IDs (~350 entries, ~500KB)
2. **Background warmer:** PlayerCacheService pre-fills first page at startup, refreshes every 24h
3. **Response cache:** 1h client-side TTL, varies by HX-Request header

**Database & Runtime:**
- **SQLite:** 72MB read-only, WAL mode, ~650ms cold start, 5-10MB DbContext footprint, ~500KB caches
- **Horizontal scale:** Stateless. Each instance has own IMemoryCache. Database updates won't sync until TTL expires (acceptable for static historical data)

**Identified Risk:** **Cache invalidation strategy is absent.** If Lahman database refreshes out-of-band (new HOF inductee, salary correction), UI won't reflect changes until 24h TTL expires or manual restart. This is acceptable given read-only/historical-data context, but ops should document the SOP.

**Recommendations:**
- ✅ **Document cache invalidation SOP:** When/how to clear IMemoryCache, restart app, coordinate multi-instance invalidation
- ✅ **Add slow-query logging:** Track queries >5s to catch timeout risk under load
- ✅ **Centralize cache keys:** Optional refactor (low priority) — batch filter keys into shared CacheService
- ✅ **Monitor query compilation overhead:** Dynamic Where/OrderBy chains are safe at current volume (<1M qpm), but 2–5ms overhead per query

**Constraints for Migration:**
- Read-only schema — no EF Migrations needed
- 28+ DbSets with rich navigation — includes are strategic (8 total)
- Cache invalidation must be documented as any new cached keys or TTL changes are added

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
- Migration approval: **Page-by-page rollout** (not component-by-shared-component)
- Highest-priority quick wins: **Extract _FilterForm.cshtml and _LoadingOverlay.cshtml** (2-3 hours, high reuse value)
- Before major migrations: **Prioritize page handler integration tests** (low regression risk baseline)
