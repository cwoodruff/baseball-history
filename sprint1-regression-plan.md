# Sprint 1 Issue #5: Regression Safety-Net Slice — Execution Plan

**Prepared by:** Lambert (Tester)  
**Date:** 2026-04-16  
**Scope:** First regression safety-net slice for htmxRazor migration  
**Focus Areas:** Page handler routing, htmx partial-vs-full behavior, pagination edge cases, API error paths

---

## Current Status

### Baseline Test Suite Health
- **Total Tests:** 254 (247 passing + 4 failing + 3 compilation errors)
- **Passing:** Database (30+), Htmx Extensions (21), View Models (12 of 14)
- **Failing:** 4 page model tests (NullReferenceException in Partial rendering)
- **Missing:** 0/19 page models tested, 0/20 API endpoints tested

### Critical Blockers Discovered

1. **Test Infrastructure Missing**
   - `PageModelTestBase` lacks proper `ITempDataProvider` and `ViewDataDictionary` initialization
   - `PageContext.ViewData` null when calling `Partial()` — blocks page model unit testing
   - Test project missing `Microsoft.AspNetCore.Mvc` package reference

2. **Existing Failing Tests**
   - `PlayersIndexModelTests.OnGetAsync_WithHtmxHeader_ReturnsPlayersContentPartial` 
   - `SearchModelTests` suite (3 tests failing for same reason)
   - All fail with: `System.NullReferenceException` at `PageModel.Partial()`

3. **Under-Tested Integration Flows**
   - htmx request detection (Extension tested, handler routing not tested)
   - Partial response clamping vs full page behavior
   - Pagination boundary conditions (0, -1, >maxpage)
   - Sort expression stability across letters/years
   - Cache invalidation and 24-hour TTL assumptions

---

## Execution Plan: First Regression Slice

### Phase 1: Fix Test Infrastructure (Blocker)
**Status:** REQUIRED before any new page model tests can run  
**Effort:** 30 mins

#### 1.1 Add Missing Package
- Add `<PackageReference Include="Microsoft.AspNetCore.Mvc" Version="..." />` to test `.csproj`
- Ensures `ActionContext`, `PageContext`, `RazorPageResult` types available

#### 1.2 Enhance PageModelTestBase
- Expand `CreatePageContext()` to properly initialize:
  - `ViewDataDictionary` with metadata provider
  - `ModelStateDictionary` 
  - `ITempDataDictionary` backed by `TempDataProvider`
- Add helper `SetupPartialRendering(PageModel)` to set `ViewData` and `TempData` on model instance
- Add helper to inject `IMemoryCache` warmed with HOF and letter cache entries

#### 1.3 Fix Existing Page Model Tests
- Update 4 failing tests to use enhanced `PageModelTestBase` helpers
- Verify all 4 tests pass: 
  - `PlayersIndexModelTests` (2 tests fixed)
  - `SearchModelTests` (2 tests fixed)

### Phase 2: Extend Page Model Smoke Tests (High ROI)
**Status:** Implements coverage plan from history.md  
**Effort:** 4-6 hours

Create `Pages/{Feature}Tests.cs` for each handler. Each test file should cover:
- **Handler existence** — Can instantiate with DI
- **Full page response** — `OnGetAsync()` without htmx header returns `PageResult`
- **Htmx partial response** — `OnGetAsync()` with `HX-Request` header returns partial view
- **Representative edge case** — Pagination boundary, invalid filter, not-found ID

#### 2.1 Priority Test Files (Top 5, ordered by regression blast radius)

| Feature | File Path | Key Edge Case | Why It Matters |
|---------|-----------|---------------|----------------|
| **Players** | `Pages/PlayersIndexModelTests.cs` | Page 0, page >max | Shared _Pagination used by Stats, HoF, Awards, Salary |
| **Search** | `Pages/SearchModelTests.cs` | Empty query, <2 chars | Global header search in _Layout |
| **Stats/Batting** | `Pages/Stats/BattingModelTests.cs` | Invalid stat column, year range validation | Complex filter logic, career vs season toggle |
| **Teams** | `Pages/Teams/IndexModelTests.cs` | League filter, franchise sorting | Grouped results, in-memory sorting after query |
| **Compare** | `Pages/Compare/IndexModelTests.cs` | Same player twice, invalid IDs | Two-player composition, modal launching |

#### 2.2 Remaining Page Models (10 tests, each basic smoke coverage)
- `Pages/HallOfFame/IndexModelTests.cs`
- `Pages/Awards/IndexModelTests.cs`
- `Pages/Salaries/IndexModelTests.cs`
- `Pages/Postseason/IndexModelTests.cs`
- `Pages/Teams/SeasonModelTests.cs`
- `Pages/Teams/FranchiseModelTests.cs`
- `Pages/Stats/PitchingModelTests.cs`
- `Pages/Players/ModalModelTests.cs`
- `Pages/Home/IndexModelTests.cs`
- `Pages/ApiDocsModelTests.cs`

### Phase 3: Pagination Edge Cases (Boundary Safety)
**Status:** Regression gate for downstream features  
**Effort:** 1-2 hours

New test file: `Pages/Shared/PaginationBoundaryTests.cs`  
**Goal:** Verify Math.Clamp correctness across all handlers

Tests (5–8 test cases):
1. **Page 0 → Clamps to 1**
   ```csharp
   // All handlers: Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages))
   await model.OnGetAsync(page: 0);
   Assert.Equal(1, model.ViewModel.CurrentPage);
   ```

2. **Page -1 → Clamps to 1**

3. **Page > TotalPages → Clamps to TotalPages**

4. **Page == TotalPages → Accepted (boundary)**

5. **PageSize validation** — Verify min/max clamp in API endpoints (e.g., `pageSize = Math.Clamp(pageSize, 1, 100)`)

6. **Empty result set** — TotalPages = 1, CurrentPage = 1 when 0 items

### Phase 4: API NotFound Path Coverage
**Status:** Catch 404 logic regressions  
**Effort:** 2-3 hours

New test file: `Api/ApiNotFoundTests.cs`  
**Goal:** Verify all endpoints return 404 for invalid IDs

Representative tests (priority order):
1. **PlayerEndpoints.GetPlayerDetail** — Invalid playerId → 404
2. **PlayerEndpoints.GetPlayerBatting** — Invalid playerId → 404
3. **TeamEndpoints.GetTeamDetail** — Invalid teamId → 404
4. **HallOfFameEndpoints.GetInducteeDetail** — Invalid playerId → 404
5. **AwardEndpoints.GetAwardWinners** — Invalid awardId → 404
6. **PostseasonEndpoints.GetSeriesByYear** — Invalid year (e.g., 1869) → 404

Pattern for each test:
```csharp
[Fact]
public async Task GetPlayerDetail_WithInvalidPlayerId_Returns404()
{
    var result = await PlayerEndpoints.GetPlayerDetail("INVALID_ID", context, cache);
    Assert.IsType<NotFoundResult>(result);
}
```

### Phase 5: htmx Routing Contract Tests
**Status:** Catch partial-vs-page regressions  
**Effort:** 2 hours

New test file: `Pages/HtmxRoutingContractTests.cs`  
**Goal:** Verify critical handlers properly detect and route to partials

Handlers to cover (highest risk):
1. **Players.Index** — `HX-Request` → `_PlayersContent` partial
2. **Search** — `HX-Request` → `_SearchResults` partial
3. **Stats.Batting** — `HX-Request` → `_BattingLeaders` partial
4. **Stats.Pitching** — `HX-Request` → `_PitchingLeaders` partial
5. **Teams.Index** — `HX-Request` → `_TeamList` partial

Each test verifies:
- Without `HX-Request` header → `PageResult` (full page)
- With `HX-Request` header → `PartialViewResult` with correct view name

Example:
```csharp
[Fact]
public async Task OnGetAsync_HtmxRequest_ReturnsPartialView()
{
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["HX-Request"] = "true";
    var model = new IndexModel(context, cache) { PageContext = CreatePageContext(httpContext) };
    
    var result = await model.OnGetAsync("A", 1);
    
    var partial = Assert.IsType<PartialViewResult>(result);
    Assert.Equal("_PlayersContent", partial.ViewName);
}

[Fact]
public async Task OnGetAsync_FullPageRequest_ReturnsPage()
{
    var model = new IndexModel(context, cache) 
    { 
        PageContext = CreatePageContext(new DefaultHttpContext()) 
    };
    
    var result = await model.OnGetAsync("A", 1);
    
    Assert.IsType<PageResult>(result);
}
```

---

## Test Files to Create or Extend

### New Files (Phase 2–5)

| File Path | Tests | Phase | Status |
|-----------|-------|-------|--------|
| `Pages/PlayersIndexModelTests.cs` | 5 | 2 | Create (fix existing) |
| `Pages/SearchModelTests.cs` | 5 | 2 | Extend (fix existing) |
| `Pages/Stats/BattingModelTests.cs` | 4 | 2 | Create |
| `Pages/Teams/IndexModelTests.cs` | 4 | 2 | Create |
| `Pages/Compare/IndexModelTests.cs` | 3 | 2 | Create |
| `Pages/HallOfFame/IndexModelTests.cs` | 3 | 2 | Create |
| `Pages/Awards/IndexModelTests.cs` | 3 | 2 | Create |
| `Pages/Salaries/IndexModelTests.cs` | 3 | 2 | Create |
| `Pages/Postseason/IndexModelTests.cs` | 3 | 2 | Create |
| `Pages/Teams/SeasonModelTests.cs` | 2 | 2 | Create |
| `Pages/Teams/FranchiseModelTests.cs` | 2 | 2 | Create |
| `Pages/Stats/PitchingModelTests.cs` | 3 | 2 | Create |
| `Pages/Players/ModalModelTests.cs` | 2 | 2 | Create |
| `Pages/Home/IndexModelTests.cs` | 2 | 2 | Create |
| `Pages/ApiDocsModelTests.cs` | 1 | 2 | Create |
| `Pages/Shared/PaginationBoundaryTests.cs` | 6 | 3 | Create |
| `Api/ApiNotFoundTests.cs` | 6 | 4 | Create |
| `Pages/HtmxRoutingContractTests.cs` | 8 | 5 | Create |

### Files to Modify

| File Path | Change | Phase |
|-----------|--------|-------|
| `baseball-history-tests.csproj` | Add `Microsoft.AspNetCore.Mvc` package | 1 |
| `Pages/PageModelTestBase.cs` | Enhance context initialization, add helpers | 1 |

---

## Most Valuable First 5–10 Tests (MVP Regression Gate)

**Recommend landing in this order for fastest cycle feedback:**

1. **Fix PageModelTestBase + test infrastructure** (Phase 1)
2. **Players.Index smoke test** — Full page + htmx partial routing
3. **Search smoke test** — Htmx routing + empty query edge case
4. **Pagination boundary test** — Page 0, page >max (applies to 7 handlers)
5. **Stats.Batting smoke test** — Full page + htmx partial + stat validation
6. **API NotFound test** — PlayerEndpoints.GetPlayerDetail with invalid ID
7. **Teams.Index smoke test** — League filter, franchise sorting
8. **htmx routing contract tests** — Verify 5 critical handlers route correctly

**Expected result after these 8:** Coverage of 4 page handlers, 3 edge cases, 1 API path, regression safety for 60% of user flows.

---

## Test Helpers & Patterns Already Present

### In `PageModelTestBase`
- `CreateContext()` — Loads Lahman database from disk
- `CreateMemoryCache()` — Fresh `MemoryCache` instance
- `CreatePageContext()` — Basic HTTP context (needs enhancement)
- `CreateTempData()` — ITempDataDictionary with test provider

### In `DatabaseTest` & `DatabaseIntegrationTests`
- Database connection path resolution (relative to solution root)
- Context instantiation pattern
- 30+ example queries to copy for edge case setup

### In `HtmxExtensionsTests`
- Header injection pattern: `httpContext.Request.Headers["HX-Request"] = "true"`
- Assertion pattern: `Assert.IsType<T>(result)`
- Both can be reused for page model tests

### In `PaginationModelTests`
- Edge case patterns for boundary testing
- Query parameter encoding validation
- Can extend for pagination handler tests

### ViewModels Under Test
- `PlayerDetailViewModel` — Factory method pattern (`FromPeople()`)
- `SearchViewModel` — Result composition pattern
- `LeaderboardViewModel` — Filter aggregation pattern

---

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **PageContext initialization complexity** | Blocker for all page model tests | Phase 1: Create reusable helper; test with 1 handler first |
| **Cache cache key conflicts** | Tests interfere with each other | Use fresh `MemoryCache()` per test; don't share cache state |
| **Composite primary keys on Batting/Pitching** | Edge case queries complex | Copy patterns from existing DB tests; use PlayerId+YearId+TeamId+Stint |
| **Htmx header name case sensitivity** | Routing might fail | Test shows `HX-Request` works; verify in actual requests |
| **Query projection complexity** | ViewModel population untested | Focus on data shape, not EF internals; test view model factory methods |

---

## Success Criteria

**Phase 1 (Infrastructure Fix):** All 4 existing page model tests pass ✅  
**Phase 2 (5 Priority Handlers):** 20+ new tests, all passing ✅  
**Phase 3 (Pagination):** 6+ boundary tests, all passing ✅  
**Phase 4 (API 404s):** 6+ not-found paths covered ✅  
**Phase 5 (htmx Routing):** 8+ routing contract tests ✅  

**Gate for #6 (Shell Migration) & #7 (Primitives):** 50+ total regression tests passing with >95% pass rate

---

## Next Steps (Immediate)

1. Review plan with Woody, Ripley, Parker for approval
2. Fix test infrastructure (Phase 1) — 30 mins
3. Create Players test file skeleton + 2 tests — 30 mins
4. Verify 2 tests pass before extending to full 5-priority suite
5. Report back with Phase 1 completion + lessons learned

---

## Notes for Reviewer

This plan **does not modify application code**, only test infrastructure and coverage. All 254 existing tests must pass before landing Phase 1. The 4 currently failing tests are **not regressions** — they're incomplete page model harness work that needs fixing as part of test infrastructure setup, not application fixes.

