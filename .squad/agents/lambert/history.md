# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

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
