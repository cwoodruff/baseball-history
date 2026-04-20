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

## Shared Shell Implementation Review (2026-04-17)

### Global Search Wiring
- **Backend contract:** `SearchModel.OnGetAsync(string? q)` always returns `Partial()` — **safe across migrations**
- **Coupling:** Handler name `AllResults` hardcoded in frontend (`/Search?handler=AllResults`) — string-based magic, not validated
- **Risk:** Query param name `q` and partial view names (`_SearchResults`, `_SearchAllResultsModal`) are implicit dependencies
- **Migration impact:** Low — no handler changes needed, just partial name alignment

### Modal Host Lifecycle
- **Architecture:** Bare container `#modal-container` with document-level event listeners (persist across hx-boost)
- **Handlers:** `ModalModel.OnGetAsync()` always returns `Partial("_PlayerModal")` — **no htmx routing logic**
- **Response cache:** `[ResponseCache(Duration = 3600)]` on ModalModel, but missing `VaryByHeader = "HX-Request"` (not a blocker since always returns partial)
- **Bootstrap init:** Manual JavaScript in site.js (lines 154–165) — if htmxRazor has modal helpers, can migrate
- **Cleanup:** Modal disposal and backdrop cleanup are robust, but dropdown reinit runs twice (redundant optimization)
- **Migration impact:** Medium — must preserve target ID `#modal-container`, partial names, and request paths

### hx-boost Navigation Behavior
- **Configuration:** `<body hx-boost="true">` with `Request.IsHtmxNonBoostedRequest()` logic in most handlers
- **Fragility:** `SearchModel` and `ModalModel` always return partials (hardcoded), not checking for boosted requests
  - **Risk:** If user lands on `/Search` or `/Players/Modal/{id}` via direct link after boosted nav, would return partial without layout → broken page
  - **Status:** Edge case (not hit in current nav topology), but architecturally inconsistent
- **Fix:** Add validation in handlers: `if (!Request.IsHtmxRequest()) return BadRequest()` to enforce partial-only endpoints

### Request-Path Fragility
- **Implicit contracts:** 6 hardcoded strings (handler names, target IDs, params, routes, partial names) with zero enforcement
- **No contract tests:** SearchModel, ModalModel have 0 integration tests
- **String-based coupling:** Making renaming dangerous (handler name, routes, partial names could break silently)
- **Deferred:** Extract to constants, add integration tests post-migration

### Backend Readiness: ✅ **High Confidence**
- All handlers are read-only (OnGetAsync only)
- Projection-first queries, no N+1 risks
- Caching strategy can stay intact
- ViewModels are view-agnostic
- Database logic unchanged across migration

### Migration Checklist
✅ Search input stays in layout (styling only)  
✅ Modal host div stays (lifecycle script can move if htmxRazor has modal helpers)  
✅ All PageModel handlers stay (no refactoring)  
✅ Database queries stay  
✅ Route paths stay (`/Search`, `/Players/Modal/{id}`, `/Search?handler=AllResults`)  
⚠️ Partial names must align (htmxRazor equivalents)  
⚠️ Modal target ID stays `#modal-container`  
⚠️ Query param stays `q`  

### Deferred (Post-Migration)
- Add `VaryByHeader` to ModalModel cache (defensive)
- Consolidate Bootstrap dropdown reinit (optimize)
- Extract handler names to constants (safety)
- Add integration tests for search/modal endpoints (coverage)
- Add validation to SearchModel/ModalModel: enforce `IsHtmxRequest()` (tighten contract)

---

## 2026-04-16 Issue #7 Discovery: Backend Coupling & Handler Seams Analysis

### Discovery Completed
- Audited all 19 PageModel handlers; all use OnGetAsync with primary constructor injection
- Documented 6 implicit frontend-to-backend string dependencies (handler names, modal target ID, search param, partial names, routes, Bootstrap selectors)
- Verified partial/full-page routing consistent via `Request.IsHtmxNonBoostedRequest()`
- Identified 2 edge cases where boosted navigation to partial-only routes returns 200 with partial (breaks page)
- Confirmed zero integration tests for SearchModel and ModalModel handlers

### Key Findings
1. **Migration Risk: LOW** — No handler logic changes required for Tier 1 components
2. **Coupling Risk: MEDIUM** — 6 implicit string dependencies validated only by manual testing
3. **Testing Gap: HIGH** — Zero page model tests; edge cases uncovered

### Recommendations
1. Preserve all route paths, param names, handler names during migration
2. Add validation guards: `if (!Request.IsHtmxRequest())` in SearchModel and ModalModel
3. Extract handler names to constants in C# (avoid string magic)
4. Add post-migration integration tests for handler contracts

### Status
✅ Discovery complete. Delivered shell findings brief + primitives inventory. Ready for Tier 1 migration within existing handler infrastructure.

### Next Steps
1. Deliver #4 proof-of-concept (modal component)
2. Support Dallas during Tier 1 migration (confirm partial names don't break)
3. Collaborate on filter form extraction (URL parameter alignment across 5 pages)
4. Post-migration: Add constant extraction for handler names


### Safe Primitive Slice Review (2026-04-17)
- Shared filter/loading foundation stayed view-only: reused `_LoadingSpinner`, added shared filter shell classes in `wwwroot/css/site.css`, and kept all existing `hx-get`, `hx-target`, `hx-include`, `hx-push-url`, and handler routes unchanged.
- Hall of Fame adopted the same loading overlay safely by adding `hx-indicator` only to the existing filter controls; no PageModel or query contract changes were required.
- Added page-routing coverage for Awards, Postseason, Salaries, and Hall of Fame full-page vs HTMX partial behavior in `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs`.
- Validation: `dotnet build baseball-history.sln` and `dotnet test baseball-history-tests` passed after the slice.

### Safe Slice Revision (2026-04-16)
- Ripley/Lambert rereview tightened Issue #7 Phase A to shared primitives only: `_EmptyState.cshtml`, `_LoadingSpinner.cshtml`, and directly supporting `site.css` rules.
- Page-level filter wrappers, loading-overlay rewiring, `_ViewImports.cshtml` changes, and page integration-test scaffolding were reverted to keep handler contracts and HTMX wiring untouched.
- Validation after the trim passed with `dotnet build baseball-history.sln --no-restore` and `dotnet test baseball-history-tests --no-restore` (247/247).

### Phase A Final Approval & Orchestration (2026-04-16)

**Decision:** ✅ Phase A approved for landing (Lambert reviewed and signed off)

**Scope Lock:**
- Component-only slice: `_EmptyState.cshtml`, `_LoadingSpinner.cshtml`, CSS filter foundation only
- No PageModel handlers modified
- No route changes
- No htmx contract changes
- All handler seams preserved for future extraction

**What Stayed Out:**
- Filter form extraction (deferred to separate Phase B review)
- Loading overlay consolidation (deferred to separate Phase C review)
- Page-level integration test scaffolding

**Key Pattern:** Narrow scope + regression verification = approval-safe even with unrelated branch state

**Next Steps:** Phase B requires explicit scope review after #6 shell stabilization

### Issue #4 htmxRazor Baseline Completion (2026-04-20)
- Verified the package and middleware wiring were already present in `baseball-history-web/baseball-history-web.csproj` and `baseball-history-web/Program.cs`; the missing seam was `Pages/_ViewImports.cshtml`, which lacked the `htmxRazor` Tag Helper registration.
- Confirmed the shared asset strategy in `Pages/Shared/_Layout.cshtml` is safe for incremental migration: foundation assets are injected from `/_rhx/`, while component CSS stays explicitly imported in layout to avoid page-by-page drift.
- Kept the proof surface on `Pages/About.cshtml` so Sprint 1 does not perturb modal, search, or boosted-navigation handlers; once Tag Helpers were registered, the `rhx-button` rendered to standard button markup instead of leaking a raw `<rhx-button>` tag.
- Added integration coverage in `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs` to lock the baseline: `/About` now proves `/_rhx/` assets are present and the proof component is rendered, and `/_rhx/css/rhx-core.css` is served successfully.

## Sprint 1 Completion (2026-04-20)

**Status:** ✅ COMPLETE — Orchestration log recorded

### Work Summary
- **Issue #4 verification:** About.cshtml proof component rendering correctly
- **Asset loading:** /_rhx/ static assets confirmed loading without 404s
- **Tag helper wiring:** Confirmed integration with cleaned-up _ViewImports.cshtml
- **Guardrails documented:** Skill watchout recorded (assets may load while tag helpers fail)

### Integration Test Coverage Added
- ✅ `/About` routes to full page (GET without HX-Request header)
- ✅ `/About` returns partial when htmx requested (HX-Request: true)
- ✅ /_rhx/ foundation CSS verified present and loadable
- ✅ rhx-button component renders as interactive element (tag helper processed)

### Key Finding: Skill Watchout
**Assets may load while tag helpers still fail.** This is a silent failure:
- Browser network shows /_rhx/css/foundation.css loading (200 OK)
- Browser console shows no errors
- But rhx-* tag helpers still render as plain HTML if tag helper registration is missing
- Mitigation: Always verify `@addTagHelper *, htmxRazor` in _ViewImports.cshtml before testing component wiring

### Decisions Merged to decisions.md
- parker-issue4.md → decisions.md (proof component strategy locked)

### Test Results
- ✅ dotnet build succeeded
- ✅ dotnet test: 289/289 passing (including new integration coverage)

### Sprint 2 Readiness
✅ Ready to proceed with Issues #8–#9 (foundation pages) under regression safety net.
✅ All backend seams preserved; no PageModel refactoring needed.
✅ Asset loading strategy proven on About proof component; can scale to other pages.
