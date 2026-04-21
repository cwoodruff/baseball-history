# Test Implementation Checklist — Sprint 1 Issue #5

**Status:** Plan complete, ready for approval  
**Last Updated:** 2026-04-16  
**Total Tests:** 50+ regression tests across 5 phases

---

## Phase 1: Infrastructure Fix [BLOCKER] — 1.5 hours

### Changes Required (No test files yet)

- [ ] Add package reference: `Microsoft.AspNetCore.Mvc` (Version 10.0.5)
- [ ] Enhance `PageModelTestBase.CreatePageContext()` to initialize `ViewData`
- [ ] Add `InitializePageModelForPartialRendering()` helper method
- [ ] Add `CreateTempData()` helper method

### Tests Fixed (Already exist, currently failing)

- [ ] `PlayersIndexModelTests.OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `PlayersIndexModelTests.OnGetAsync_WithHtmxHeader_ReturnsPlayersContentPartial()`
- [ ] `SearchModelTests.OnGetAsync_WithShortQuery_ReturnsSearchResultsPartial()`
- [ ] `SearchModelTests.OnGetAllResultsAsync_WithShortQuery_ReturnsAllResultsModalPartial()`

**Verification Gate:** `dotnet test baseball-history-tests --nologo` → All 254 tests pass

---

## Phase 2: Page Model Smoke Tests [HIGH ROI] — 6 hours

### Priority 5 Handlers (20 tests total)

#### File: `Pages/PlayersIndexModelTests.cs` (4 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsPlayersContentPartial()`
- [ ] `OnGetAsync_WithPageZero_ClampsCurrentPageToOne()`
- [ ] `OnGetAsync_WithPageAboveMax_ClampsToTotalPages()`

#### File: `Pages/SearchModelTests.cs` (4 tests) — Extend existing
- [ ] `OnGetAsync_WithEmptyQuery_ReturnsEmptyResultsPartial()`
- [ ] `OnGetAsync_WithShortQuery_ReturnsEmptyResultsPartial()`
- [ ] `OnGetAsync_WithValidQuery_PopulatesViewModelAndReturnsPartial()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsSearchResultsPartial()`

#### File: `Pages/Stats/BattingModelTests.cs` (4 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsBattingLeadersPartial()`
- [ ] `OnGetAsync_WithDefaultStat_DefaultsToHomeRuns()`
- [ ] `OnGetAsync_WithInvalidStat_DefaultsToHomeRuns()`

#### File: `Pages/Teams/IndexModelTests.cs` (4 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsTeamListPartial()`
- [ ] `OnGetAsync_WithoutLeagueFilter_ReturnsBothActiveAndInactive()`
- [ ] `OnGetAsync_WithLeagueFilter_FiltersCorrectly()`

#### File: `Pages/Compare/IndexModelTests.cs` (4 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithValidPlayers_PopulatesViewModel()`
- [ ] `OnGetAsync_WithInvalidPlayerId_ReturnsEmptyComparison()`
- [ ] `OnGetAsync_WithSamePlayerTwice_AllowsComparison()`

### Priority 10 Handlers (15 tests total)

#### File: `Pages/HallOfFame/IndexModelTests.cs` (3 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsInducteeListPartial()`
- [ ] `OnGetAsync_WithYear_FiltersCorrectly()`

#### File: `Pages/Awards/IndexModelTests.cs` (3 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsAwardListPartial()`
- [ ] `OnGetAsync_WithYear_FiltersCorrectly()`

#### File: `Pages/Salaries/IndexModelTests.cs` (3 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsSalaryListPartial()`
- [ ] `OnGetAsync_PopulatesAvailableYears()`

#### File: `Pages/Postseason/IndexModelTests.cs` (3 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsPostseasonListPartial()`
- [ ] `OnGetAsync_WithYear_FiltersCorrectly()`

#### File: `Pages/Teams/SeasonModelTests.cs` (2 tests)
- [ ] `OnGetAsync_WithValidTeamIdAndYear_ReturnsPageResult()`
- [ ] `OnGetAsync_WithInvalidTeamId_ReturnsNotFound()`

#### File: `Pages/Teams/FranchiseModelTests.cs` (2 tests)
- [ ] `OnGetAsync_WithValidFranchiseId_ReturnsPageResult()`
- [ ] `OnGetAsync_WithInvalidFranchiseId_ReturnsNotFound()`

#### File: `Pages/Stats/PitchingModelTests.cs` (3 tests)
- [ ] `OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()`
- [ ] `OnGetAsync_WithHtmxHeader_ReturnsPitchingLeadersPartial()`
- [ ] `OnGetAsync_WithDefaultStat_DefaultsToWins()`

#### File: `Pages/Players/ModalModelTests.cs` (2 tests)
- [ ] `OnGetAsync_WithValidPlayerId_PopulatesPlayerDetail()`
- [ ] `OnGetAsync_WithInvalidPlayerId_ReturnsNotFound()`

#### File: `Pages/Home/IndexModelTests.cs` (2 tests)
- [ ] `OnGetAsync_PopulatesRecentStatsViewModel()`
- [ ] `OnGetAsync_CachesDataFor24Hours()`

#### File: `Pages/ApiDocsModelTests.cs` (1 test)
- [ ] `OnGetAsync_ReturnsPageResult()`

---

## Phase 3: Pagination Boundaries [REGRESSION GATE] — 2 hours

### File: `Pages/Shared/PaginationBoundaryTests.cs` (6+ tests)

- [ ] `Page0_ClampsToPageOne_AllHandlers()`
- [ ] `PageNegative_ClampsToPageOne_AllHandlers()`
- [ ] `PageBeyondMax_ClampsToTotalPages_AllHandlers()`
- [ ] `PageEqualToMax_AcceptedWithoutClamping()`
- [ ] `EmptyResultSet_TotalPagesEqualsOne()`
- [ ] `PageSizeClampedToMinMax_ApiEndpoints()`

**Coverage:** Verifies Math.Clamp logic across Players, Stats, Teams, HoF, Awards, Salaries, Postseason

---

## Phase 4: API Not-Found Paths [ERROR PATH SAFETY] — 3 hours

### File: `Api/ApiNotFoundTests.cs` (6+ tests)

- [ ] `GetPlayerDetail_WithInvalidPlayerId_Returns404()`
- [ ] `GetPlayerBatting_WithInvalidPlayerId_Returns404()`
- [ ] `GetTeamDetail_WithInvalidTeamId_Returns404()`
- [ ] `GetHallOfFameDetail_WithInvalidPlayerId_Returns404()`
- [ ] `GetAwardWinners_WithInvalidAwardId_Returns404()`
- [ ] `GetPostseasonSeries_WithInvalidYear_Returns404()`

**Pattern for each:**
```csharp
[Fact]
public async Task GetPlayerDetail_WithInvalidPlayerId_Returns404()
{
    var result = await PlayerEndpoints.GetPlayerDetail("INVALID_ID", context, cache);
    var notFound = Assert.IsType<NotFoundResult>(result);
    Assert.Equal(404, notFound.StatusCode);
}
```

---

## Phase 5: htmx Routing Contracts [BEHAVIOR GATE] — 2 hours

### File: `Pages/HtmxRoutingContractTests.cs` (8+ tests)

#### Players.Index (2 tests)
- [ ] `OnGetAsync_WithHXRequest_ReturnsPlayersContentPartial()`
- [ ] `OnGetAsync_WithoutHXRequest_ReturnsFullPage()`

#### Search (2 tests)
- [ ] `OnGetAsync_WithHXRequest_ReturnsSearchResultsPartial()`
- [ ] `OnGetAsync_WithoutHXRequest_ReturnsFullPage()`

#### Stats.Batting (1 test)
- [ ] `OnGetAsync_WithHXRequest_ReturnsBattingLeadersPartial()`

#### Stats.Pitching (1 test)
- [ ] `OnGetAsync_WithHXRequest_ReturnsPitchingLeadersPartial()`

#### Teams.Index (1 test)
- [ ] `OnGetAsync_WithHXRequest_ReturnsTeamListPartial()`

#### Generic Contract (1 test)
- [ ] `HXRequest_AlwaysRoutesToPartial_KeyHandlers()`

**Pattern for each:**
```csharp
[Fact]
public async Task OnGetAsync_WithHXRequest_ReturnsPlayersContentPartial()
{
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["HX-Request"] = "true";
    
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model, httpContext);
    
    var result = await model.OnGetAsync("A", 1);
    
    var partial = Assert.IsType<PartialViewResult>(result);
    Assert.Equal("_PlayersContent", partial.ViewName);
}

[Fact]
public async Task OnGetAsync_WithoutHXRequest_ReturnsFullPage()
{
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model);
    
    var result = await model.OnGetAsync("A", 1);
    
    Assert.IsType<PageResult>(result);
}
```

---

## Test Summary Table

| Phase | File Count | Test Count | Effort | Status |
|-------|-----------|-----------|--------|--------|
| 1 | 0 (modify existing) | 4 fixed | 1.5h | Ready |
| 2 | 15 | 35 | 6h | Ready |
| 3 | 1 | 6 | 2h | Ready |
| 4 | 1 | 6 | 3h | Ready |
| 5 | 1 | 8 | 2h | Ready |
| **TOTAL** | **18 files** | **50+ tests** | **40h** | **Ready** |

---

## Test Execution Order (Recommended)

1. **Phase 1 first** — Infrastructure fix unblocks all downstream tests
2. **Phase 2 priority 5 first** — High-value handlers tested first
3. **Phase 3 parallel with Phase 2** — Pagination tests can run independently
4. **Phase 4 after Phase 2** — API tests don't depend on page model tests
5. **Phase 5 last** — Routing contract tests verify behavior after handlers are tested

---

## Test Helpers to Use

### From `PageModelTestBase`
- `CreateContext()` — Load Lahman database
- `CreateMemoryCache()` — Fresh MemoryCache per test
- `CreatePageContext(httpContext)` — Initialize PageContext
- `InitializePageModelForPartialRendering(model, httpContext)` — Full setup
- `CreateTempData(httpContext)` — ITempDataDictionary

### HTTP Headers
```csharp
httpContext.Request.Headers["HX-Request"] = "true";
httpContext.Request.Headers["HX-Boosted"] = "true";
```

### Assertions
```csharp
Assert.IsType<PageResult>(result);
Assert.IsType<PartialViewResult>(result);
Assert.IsType<NotFoundResult>(result);
Assert.IsType<RedirectResult>(result);
```

---

## Success Criteria

- [ ] Phase 1: All 254 tests passing (no failures)
- [ ] Phase 2: All 35 page model tests passing
- [ ] Phase 3: All 6+ pagination boundary tests passing
- [ ] Phase 4: All 6+ API not-found tests passing
- [ ] Phase 5: All 8+ htmx routing contract tests passing
- [ ] Overall: 50+ total regression tests, >95% pass rate
- [ ] Gate met: Ready for #6 and #7 merges

---

## Notes

- All tests must use **fresh `MemoryCache()` per test** to avoid key collisions
- Database path resolution handled by `CreateContext()` helper
- ViewData/TempData initialization handled by `InitializePageModelForPartialRendering()`
- No application code changes required; tests only
- Patterns are reusable across all page models and API endpoints

