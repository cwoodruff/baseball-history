# Decisions

## 2026-04-16 — Dallas: Shell First-Slice Extraction (Issue #6)

**Decision:** Safe shell first-slice is extracting full header and footer chrome from `Pages/Shared/_Layout.cshtml` into `Pages/Shared/_ShellHeader.cshtml` and `Pages/Shared/_ShellFooter.cshtml`.

**Contracts Preserved:**
- `<body hx-boost="true">` unchanged (SPA navigation)
- `#modal-container` unchanged (universal modal target)
- Inline `htmx:beforeSwap/afterSwap/afterSettle` lifecycle script unchanged (Bootstrap modal + dropdown re-init)
- Search form contracts unchanged: `/Search`, `name="q"`, `#search-results`
- Player modal routes unchanged: `/Players/Modal/{id}` targeting `#modal-container`

**Status:** ✅ Approved by Lambert, build + tests passing (268/268)

---

## 2026-04-16 — Lambert: Regression Safety Net Implementation (Issue #5)

**Decision:** Implemented 40 new integration tests using WebApplicationFactory<Program> pattern across three focused test suites:

1. **Page Routing Integration Tests (11 tests)** — Verify htmx partial vs full-page rendering behavior for Players, Search, Stats/Batting, Stats/Pitching, and Teams index pages
2. **Pagination Boundary Tests (12 tests)** — Verify page 0, negative page, and page > max clamping for both Razor Pages and API endpoints
3. **API 404 Edge-Case Tests (17 tests)** — Verify proper HTTP 404 responses for invalid player/team/HOF/postseason routes, plus sanity checks for valid routes

**Rationale:**
- Integration-first approach verifies end-to-end behavior (HTTP → PageModel/Endpoint → Rendering)
- Surgical scope: limited changes to baseball-history-tests only (added 4 files, modified 1 csproj)
- No web project changes needed — leveraged existing `public partial class Program;`
- Real database validation against lahman.db in read-only mode

**Files Added:**
- `baseball-history-tests/IntegrationTestBase.cs` — Base class with WebApplicationFactory setup
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs` — 11 htmx routing tests
- `baseball-history-tests/Pages/PaginationBoundaryTests.cs` — 12 boundary condition tests
- `baseball-history-tests/Api/ApiNotFoundTests.cs` — 17 API edge-case tests

**Files Modified:**
- `baseball-history-tests/baseball-history-tests.csproj` — Added Microsoft.AspNetCore.Mvc.Testing v10.0.5

**Validation:**
- ✅ `dotnet build baseball-history.sln` — succeeded
- ✅ `dotnet test baseball-history-tests` — 287/287 passing (247 baseline + 40 new)
- ✅ All three new test suites passing independently

**Migration Gates Satisfied:**
- ✅ Page routing behavior verified for 5 critical pages
- ✅ Pagination boundary conditions verified across pages and API
- ✅ API 404 edge cases covered for invalid player/team/HOF/postseason routes

**Future Implications:**
- `IntegrationTestBase` pattern established for all future endpoint integration tests
- WebApplicationFactory setup demonstrates proper test isolation (AllowAutoRedirect = false)
- htmx request detection pattern established (Request.Headers["HX-Request"] = "true")
- Pattern can be extended for additional API endpoint suites and page smoke tests

**Status:** ✅ COMPLETE — Issue #5 regression baseline established, Sprint 2 unblocked

---

## 2026-04-16 — Lambert: Shell First-Slice Review Gate (Issue #6)

**Decision:** Approve the shared shell first-slice extraction for issue #6.

**Why:**
- `_Layout.cshtml` preserved load-bearing shell contracts in place while delegating only navbar/footer chrome to partials
- Verified unchanged contracts: `<body hx-boost="true">`, `#modal-container`, inline modal/bootstrap lifecycle JS, `/Search`, `name="q"`, `#search-results`, `/Players/Modal/{id}`
- `_ShellFooter.cshtml` matches original footer exactly
- `_ShellHeader.cshtml` preserves header/search structure with only non-behavioral line-wrap change

**Validation:** `dotnet build baseball-history.sln --nologo` ✅, `dotnet test baseball-history-tests --no-build --nologo` ✅ 268/268

**Status:** ✅ Approved

---

## 2026-04-16 — Lambert: Reject Issue #7 Safe-Primitives Candidate

**Decision:** Reject the current candidate for issue #7's first safe-primitives slice.

**Reason:** Scope drift toward shell/integration instead of primitives. Diff lands in `_Layout`, `_ShellHeader`, `_ShellFooter`, `About`, `Program`, `_ViewImports`, project wiring — none of the scoped primitive targets (`_EmptyState`, `_LoadingSpinner`, filter extraction) were actually migrated.

**Impact:** Issue #7 acceptance criteria still unmet. This is scope drift toward #4/#6.

**Required Revision:** Have Parker or non-Dallas implementer split by issue boundary — keep shell/integration proof under #4/#6, then submit separate #7 slice touching only shared primitive/filter files plus directly relevant tests.

**Status:** ⛔ Rejected with clear path to resubmission

---

## 2026-04-16 — Ripley: Safe Primitives Review Gate (Issue #7)

**Decision:** Approve issue #7 scope with phased rollout: Phase A (immediate), Phase B/C (conditional).

### Phase A — Safe to Harden Now ✅

**_EmptyState**
- Consumers: 9 pages (Players, Teams, Stats/Batting, Stats/Pitching, Awards, HallOfFame, Salaries, Postseason, Teams/Season)
- Model signature: `EmptyStateModel { Title, Message, Icon, ActionUrl, ActionText }`
- Factory methods stable: `NoPlayers()`, `NoTeams()`, `NoStats()` all maintain signature
- Guard: No breaking changes without atomic updates to all 9 consumers
- ✅ Safe to wrap with htmxRazor, keep factory pattern intact

**_LoadingSpinner**
- Consumers: 0 (dormant reference in comments)
- Model: `string?` (optional message)
- Ultra-simple, lowest risk
- Guard: Never change model signature; treat as immutable baseline
- ✅ Safe to document and standardize; no logic changes needed

### Phase B — Wait for #6 Completion (Deferred)

**_FilterForm (New Extraction)**
- Identified duplication: Batting, Pitching, Awards, HallOfFame, Postseason — identical `<select>` patterns
- Blocker: FilterForm lives inside shell container with htmx indicators (`hx-indicator="#loading-indicator"`)
- Dependency: Must verify shell container IDs stable after #6 before extraction
- Condition: Extract only after #6 shell review complete and container names committed
- Prevents double-refactor risk

### Phase C — Defer Until Pattern Emerges

**_LoadingOverlay (New Extraction)**
- Current state: 5 pages have custom overlay markup; not yet unified
- Blocker: Different triggers, positions, styles across pages
- Decision: Do not extract until pattern stabilizes and 3+ pages identical
- Revisit after FilterForm extraction

**Rejection Gates:**
- ❌ Phase A PR rejected if EmptyStateModel signature changes without atomic updates to all 9 consumers
- ❌ Phase A PR rejected if LoadingSpinner model changes
- ❌ FilterForm PR rejected if lands before #6 shell review merged
- ❌ FilterForm PR rejected if assumes specific shell container ID without documentation

**Status:** ✅ Approved (Phase A only, Phase B/C conditions locked)

---

## Merge Gate Summary

| Issue | Owner | Status | Gate Dependency |
|-------|-------|--------|-----------------|
| #6 Shell | Dallas | ✅ Approved | #5 regression tests passing |
| #5 Regression | Lambert | ✅ Approved | Unblocked (gate for #6/#7) |
| #7 Phase A | Dallas | ⏳ Ready | Phase A scope locked to EmptyState/LoadingSpinner only |
| #7 Phase B | TBD | ⏸️ Blocked | Waiting for #6 shell freeze |
| #7 Phase C | TBD | ⏸️ Blocked | Pattern emergence + Phase B complete |

---

## 2026-04-16 — Ripley: Final Sprint 1 Acceptance Review

**Decision:** Sprint 1 is **ACCEPTED**. All four issues (#4, #5, #6, #7 Phase A) are delivered and verified. Regression safety net in place. No blockers.

**Status:** ✅ **FINAL** — Unblocks Sprint 2

### Validation Summary

| Issue | Component | Status | Notes |
|-------|-----------|--------|-------|
| #4 | htmxRazor baseline | ✅ Verified | Search, player modal, team list routing working |
| #5 | Regression tests | ✅ 40 delivered | 287/287 tests passing (247 baseline + 40 new) |
| #6 | Shell extraction | ✅ Verified | Contracts preserved, 268/268 tests |
| #7A | Safe primitives | ✅ Verified | EmptyState + LoadingSpinner stable, tests passing |

### Non-Blocking Follow-ups (Sprint 2)

1. Add shell contract tests (`#modal-container`, `name="q"`) before shell modifications
2. Expand routing coverage to HallOfFame, Awards, Postseason, Salaries, Compare
3. Verify EmptyState factory test coverage for Phase B safety  
4. Tighten postseason API assertions once behavior confirmed
5. Plan Phase B (_FilterForm extraction) dependency chain with #6 completion

### Impact

- ✅ Sprint 2 unblocked
- ✅ Component migrations may proceed with regression safety net
- ✅ Team ready for Phase B rollout (filtered by Phase B gates)
- ✅ Orchestration log recorded at 2026-04-16T20:57:47Z
