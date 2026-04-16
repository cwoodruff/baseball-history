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

