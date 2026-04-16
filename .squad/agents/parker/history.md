# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

### Architecture Review (2026-04-16)

#### Backend Structure & Stack
- **Web Project:** 94 C# files, 44 Razor pages (.cshtml), read-only SQLite backend (Lahman Baseball Database)
- **Primary Pattern:** Razor Pages with PageModel primary constructors, EF Core projections, async/await everywhere
- **Service Layer:** Minimal — only 2 services (TeamColorService singleton, PlayerCacheService hosted service)
- **API Layer:** 9 minimal API endpoint groups under `/api` with separate DTOs (not reusing ViewModels)
- **Caching:** Three-tier:
  - `IMemoryCache` at app level (24-hour TTL for expensive queries like player letters, HOF IDs)
  - `[ResponseCache]` at PageModel level with `VaryByHeader = "HX-Request"` (htmx vs full-page caching)
  - Pre-warmed cache via `PlayerCacheService.GetCachedFirstPage()` for landing page

#### PageModel Handler Patterns
- **All handlers are OnGet** (19 OnGet methods found, 0 OnPost/OnPut/OnDelete) — pure read-only app
- **Primary Constructor Injection:** All PageModels use `(BaseballDbContext context, IMemoryCache cache)` pattern
- **Partial/Full Response Logic:**
  - Uses `Request.IsHtmxNonBoostedRequest()` to detect targeted htmx requests
  - Returns `Partial("_PartialName", viewModel)` for htmx non-boosted, `Page()` for full pages
  - 17 Partial returns vs 29 Page returns across codebase
- **Query Execution:**
  - EF projections `.Select()` with `NoTracking` behavior (global in DbContext)
  - No `.Include()` patterns — projection-first
  - Nullable int handling: `?? 0` throughout
  - Composite primary keys on entities (playerID + yearID + teamID + stint for Batting, Pitching, AllstarFull)

#### Database & Models
- **DbContext:** 28 DbSets from Lahman schema, auto-generated EF models with value converters for DateOnly?
- **Connection String:** `Mode=ReadOnly;Cache=Shared;Timeout=30` (read-only mode, shared cache for SQLite WAL)
- **WAL Mode:** Enabled at startup via `PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL`
- **Value Converters:** Custom `DateOnly?` converter handles empty strings from database (common pattern)

#### API Design
- **OpenAPI:** Spec at `/openapi/v1.json`, interactive Scalar docs at `/scalar/v1` (dev-only)
- **Endpoint Organization:** Static classes with `Map(RouteGroupBuilder)` pattern, registered via `ApiEndpointExtensions`
- **DTOs vs ViewModels:** API has separate record types in `Api/Dtos/` (e.g., `PlayerListItem`), not sharing with Razor page ViewModels
- **Response Wrapper:** `PagedResponse<T>` for paginated results
- **Expression Trees:** Leaderboard ordering helpers duplicated between API endpoints and Razor pages (not shared)

#### Extensions & Utilities
- **HtmxExtensions:** Comprehensive header detection (HX-Request, HX-Boosted, HX-Target, HX-Trigger, HX-Prompt, etc.)
- **Response Headers:** HtmxRedirect, HtmxRefresh, HtmxPushUrl, HtmxReplaceUrl, HtmxReswap, HtmxRetarget, HtmxTrigger available

#### Testing
- **xUnit + SQLite Integration:** Tests connect to actual `lahman.db` file, no in-memory database
- **Test Project:** Namespace `baseball_history_tests`, mirrors web project structure (ViewModels, Database, Extensions tests)
- **Pattern:** Simple CRUD assertions (CanConnectToDatabase, CanQueryPeople, etc.) and ViewModel unit tests

#### Program.cs Pipeline
- **Service Registration:** DbContext → MemoryCache → TeamColorService (singleton) → RazorPages → OpenApi
- **Middleware Stack:** ResponseCompression (Brotli+Gzip) → ExceptionHandler → HSTS → StaticAssets → Routing → RazorPages → OpenApi → Scalar (dev) → API Endpoints
- **Compression:** Brotli/Gzip on text, JS, CSS, JSON, SVG

#### No Identified Risks or Surprises
- Clean separation between read-only database queries and presentation logic
- No mutable state management (no form submissions, no state modifications)
- All handlers are straightforward data retrieval and view model construction
- htmx integration is well-decoupled via extension methods
- API and Razor pages could be better integrated (duplicated ordering logic) but isolated DTOs are defensible

**Prepared for:** htmxRazor migration — backend seams are well-structured and minimal coupling to presentation

## Codebase Review Output (2026-04-16)

**Backend readiness confirmed for migration**

- All PageModel handlers OnGetAsync-only, primary constructor injection verified
- htmx-aware response cache strategy validated (VaryByHeader works as intended)
- Leaderboard expression tree duplication noted for post-migration refactor
- API DTOs separate from ViewModels — defensible, no blocker
- Ash flagged cache invalidation SOP as missing — scope for documentation work
