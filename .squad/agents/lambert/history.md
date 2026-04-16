# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

### Sprint 1 htmxRazor Regression Strategy (2026-04-16)
- **Baseline status:** `dotnet test baseball-history-tests --nologo` passes with 247/247 green, but the suite still has no PageModel or API endpoint harness.
- **Exact #5 risk:** current coverage stops at DB, selected view models, and `Extensions/HtmxExtensions.cs`; the migration safety net still needs handler-level checks for partial-vs-page routing, pagination clamping, and representative API `NotFound` paths.
- **Shared shell blast radius (#6):** `Pages/Shared/_Layout.cshtml`, `Pages/_SearchResults.cshtml`, and `Pages/_SearchAllResultsModal.cshtml` carry `hx-boost`, global search, modal host, and Bootstrap re-init logic; regressions there fan out to Home, Players modal links, Awards, Stats, Salaries, Hall of Fame, Teams/Season, Compare, and search flows.
- **Shared primitives blast radius (#7):** `Pages/Shared/Components/_Pagination.cshtml`, `_AlphabetNav.cshtml`, `_PlayerCard.cshtml`, `_TeamCard.cshtml`, and repeated filter/loading markup in `Pages/Stats/*.cshtml`, `Pages/Awards/Index.cshtml`, `Pages/HallOfFame/Index.cshtml`, and `Pages/Postseason/Index.cshtml` need contract-style tests before conversion.
- **Safe parallel lanes after baseline tests land:** shell migration (#6), primitive/filter extraction (#7), and continued API/page smoke-test expansion can run in parallel only if ownership boundaries stay separate (`_Layout`/search/modal host vs shared components vs tests-only files).

### Codebase Structure & Architecture
- **Solution:** 2 projects — baseball-history-web (19 page models, 20 API endpoints) and baseball-history-tests (18 test files, 247 tests)
- **Tech Stack:** .NET 10 Razor Pages, EF Core, SQLite Lahman DB (read-only), htmx 2.0.4, Bootstrap 5
- **Key File Paths:**
  - Page models: `Pages/{Feature}/Index.cshtml.cs` (no subdirectory tests)
  - API endpoints: `Api/Endpoints/{Feature}Endpoints.cs` (20 files)
  - View models: `ViewModels/*.cs` (14 files)
  - Services: `Services/TeamColorService.cs`, `Services/PlayerCacheService.cs`
  - DB context: `Models/BaseballDbContext.cs` with 28+ DbSets

### Testing Status (Current)
- **247 tests passing** (100% pass rate)
- **Database layer:** Strong — 30+ integration tests, all major DbSets covered, FK navigation verified
- **htmx extensions:** Comprehensive — 21 tests covering request detection and response headers
- **View models:** Selective — 12 of 14 tested (missing: AlphabetNav, AwardVoting, Compare, HallOfFame, LeaderboardVM, PlayerList, Postseason, Salary, TeamList)
- **Page models:** 0/19 tested (Search, Players, Stats, Teams, Awards, HoF, Salaries, Postseason, Compare, etc.)
- **API endpoints:** 0/20 tested (Result.NotFound() conditions untested)

### Regression Risk Profile
1. **Page handler routing** — htmx partial vs. full page returns untested
2. **Pagination boundaries** — offsets (0, -1, >maxpage) never verified
3. **Sort expressions** — complex OrderByDescending in Search, career vs. season leaderboards untested
4. **API not-found paths** — invalid IDs return 404 but never tested
5. **Service aliases** — TeamColorService team ID aliases (NYA/NYY, TBA/TBD) untested
6. **Cache behavior** — [ResponseCache] with VaryByHeader="HX-Request" never verified

### Key Patterns & Conventions
- Primary constructors with DI: `IndexModel(BaseballDbContext context, IMemoryCache cache)`
- Query projection with `.Select()` preferred over loading full entities
- Nullable int fields handled with `?? 0` pattern
- Root namespace: `baseball_history_web` (underscore)
- Test namespace: `baseball_history_tests`
- Cache duration: 24 hours for expensive queries, 1 hour for client-side [ResponseCache]
- Composite primary keys on AllstarFull, Batting, Pitching (playerID+yearID+teamID+stint)

### Recommended Verification Path (Before Next Migration)
1. Add smoke tests for all 19 page handlers (basic OnGetAsync + sample inputs)
2. Add integration tests for top-5 features (Players, Search, Leaderboards, Teams, Compare)
3. Add edge-case pagination tests (boundary offsets)
4. Add NotFound path tests for all API endpoints
5. Add TeamColorService alias validation tests

### Architecture Decisions to Remember
- Database is read-only (Mode=ReadOnly;Cache=Shared)
- Global QueryTrackingBehavior.NoTracking in Program.cs
- SQLite WAL mode configured at startup
- Custom DateOnly converter handles empty string dates from Lahman database
- htmx request detection via `Request.IsHtmxNonBoostedRequest()` extension

## Codebase Review Output (2026-04-16)

**Test coverage gaps identified, regression risk assessed**

- 247 tests passing, but 0/19 page models tested, 0/20 API endpoints tested
- Database layer solid (30+ integration tests), htmx extensions comprehensive
- Pagination boundaries, sort stability, cache behavior all untested
- Recommended safest path: smoke tests for all handlers before migration
- Ripley approved proceeding with coverage work in parallel to component extraction

## 2026-04-16 Sprint 1 Regression Gates

Lambert identified critical test gaps and proposed baseline coverage targets for #5 regression test suite that gates #6/#7 merges.

### Output
- Coverage targets: 19 PageModel smoke tests, 8+ integration (htmx routing), 5+ edge-case, 5+ API NotFound
- Merge gate requirement: #5 passes before #6/#7 can merge
- Safe parallelism: Lane A (#6 shell) + Lane B (#7 primitives) both after #5 lands
- Under-coverage fallback: Post-hoc tests with "regression-coverage-spike" tag

### Status
✅ Integrated. Blocked on Parker #4. Ready to begin #5 in parallel with Dallas #6 after #4 lands. Tests gate all merges.

## 2026-04-16 Shell Migration Fragility Analysis

Lambert reviewed `_Layout.cshtml`, `site.js`, modal handlers, search lifecycle, and dropdown re-init for htmxRazor migration risks.

### Four Critical Regression Vectors Found

1. **hx-boost document flow:** Body swap with guard `if (!window.__bbHistoryInit)` may not re-initialize properly if shell becomes component. CSS/JS imports in head may not reload.
2. **Modal lifecycle coupling:** Event listeners on `#modal-container` target may not fire correctly if component re-renders the container. Stale backdrops can persist.
3. **Outside-click search cleanup (race condition):** Two mechanisms (global click listener + inline onclick handlers) both clear search results. Order undefined after component lifecycle changes. Can race with htmx requests.
4. **Bootstrap dropdown re-init:** Dropdowns re-initialized in both `afterSwap` AND `afterSettle`. If markup moves to component with local lifecycle, double-init risk.

### Blast Radius
12 pages affected: Home, About, Privacy, Health, Players, Teams, Stats, HallOfFame, Awards, Salaries, Postseason, Compare, Search, ApiDocs. Multiple modals, search flow, dropdown navigation.

### Output
- 4 verification checklists written per vector.
- 4 contract-level test specs (BDD-style) defined.
- Migration strategy: Keep `_Layout` as Razor page. If component conversion required, extract event listeners to separate JS file, consolidate search cleanup, guard dropdown re-init.
- Decision written to `.squad/decisions/inbox/lambert-shell-brief.md` for Scribe merge.

### Status
⚠️ **Recommendation: Do not move `_Layout` to component until #5 tests pass.** Estimated effort to guard all vectors: 1.5–2 days (tests + guards).


## Issue #5 First Regression Slice — Plan Complete (2026-04-16)

### Executive Summary
Investigated current test suite, identified 4 failing page model tests (NullReferenceException in Partial rendering), and produced **execution-ready plan for regression safety-net foundation**. No code changes made — analysis only.

### Key Findings

**Test Suite Baseline:**
- 254 total tests: 247 passing + 4 failing (page models) + 3 compilation errors
- Database layer: 30+ integration tests ✅ Strong
- htmx Extensions: 21 tests ✅ Comprehensive
- View Models: 12 of 14 tested ⚠️ Selective
- **Page Models: 0/19 tested** ❌ Zero coverage
- **API Endpoints: 0/20 tested** ❌ Zero coverage

**Critical Infrastructure Blocker:**
- Test project missing `Microsoft.AspNetCore.Mvc` package
- `PageModelTestBase.CreatePageContext()` doesn't initialize `ViewData`/`TempData`
- All 4 failing tests fail with NullReferenceException when calling `Partial()`

**Regression Risk Profile (Confirmed):**
1. Htmx partial-vs-page routing untested at handler level (extensions tested, routing not)
2. Pagination boundaries (0, -1, >max) never clamped and verified
3. Complex sort expressions (career vs season, across letters) unstable
4. API not-found paths (404) never verified
5. Service aliases (TeamColorService) untested
6. Cache behavior ([ResponseCache] VaryByHeader) not verified

### Execution Plan: 5 Phases, ~40 hours

**Phase 1 (1.5h) - Fix Infrastructure [BLOCKER]**
- Add Microsoft.AspNetCore.Mvc to test .csproj
- Enhance PageModelTestBase with ViewData/TempData initialization
- Fix 4 failing tests
- Gate: All 254 tests pass

**Phase 2 (6h) - Page Model Smoke Tests [HIGH ROI]**
- 19 handlers × smoke coverage (full page + htmx partial + edge case each)
- Priority 5: Players, Search, Stats/Batting, Teams, Compare
- Priority 10: HoF, Awards, Salaries, Postseason, etc.

**Phase 3 (2h) - Pagination Boundaries [REGRESSION GATE]**
- 6+ edge case tests: page 0, -1, >max, empty set, pageSize clamp

**Phase 4 (3h) - API NotFound Paths [ERROR PATH SAFETY]**
- 6+ endpoint tests with invalid IDs (404 verification)

**Phase 5 (2h) - htmx Routing Contracts [BEHAVIOR GATE]**
- 8+ tests verifying HX-Request header routes to partial, not full page

**First 5-10 Tests (MVP Regression Gate):**
1. Fix infrastructure
2. Players.Index smoke (full page + htmx partial)
3. Search smoke (htmx + empty query)
4. Pagination boundary (page 0, >max)
5. Stats.Batting smoke (full page + htmx + stat validation)
6. API NotFound (PlayerEndpoints 404)
7. Teams.Index smoke (league filter, sorting)
8. htmx routing contracts (5 critical handlers)

**Result:** 50+ regression tests gate #6 (Shell) and #7 (Primitives) merges.

### Test Patterns Available for Reuse

- **Database queries:** `DatabaseIntegrationTests` has 30+ examples
- **Header injection:** `HtmxExtensionsTests` pattern: `context.Request.Headers["HX-Request"] = "true"`
- **Edge cases:** `PaginationModelTests` has boundary validation patterns
- **Result assertions:** `Assert.IsType<PageResult>(result)`, `Assert.IsType<PartialViewResult>(result)`
- **Context setup:** `CreateContext()`, `CreateMemoryCache()`, base helpers in `PageModelTestBase`

### Deliverables

✅ Comprehensive plan saved to: `/Volumes/extra/Git/baseball-history/sprint1-regression-plan.md`  
✅ Historical context recorded (this entry)  
✅ Ready for team approval before Phase 1 execution

### Team Decision Points

- **Approve plan structure?** 5 phases, 40 hours, 50+ regression tests
- **Prioritize handler coverage?** Players → Search → Stats → Teams → Compare
- **Gate merges on Phase 1 completion?** (Infrastructure fix required before any page model tests)
- **Run Phases 2–5 in parallel or serial?** (Recommend serial for focused iteration)

### Architecture Notes for Future

- `Math.Clamp(page, 1, Math.Max(1, totalPages))` pattern used in Players, Stats, Leaderboards — consistency opportunity
- htmx routing check `Request.IsHtmxNonBoostedRequest()` used consistently — good foundation
- Cache keys ("player_letters", "hof_player_ids", "batting_years", etc.) are handler-specific, no conflicts ✅
- Response caching attribute pattern: `[ResponseCache(Duration=3600, VaryByHeader="HX-Request")]` — test coverage needed

