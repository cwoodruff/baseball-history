# Decisions

## 2026-06-08 — Ash: Postgres Insert Export Decision

**Decision:** Export one replayable Postgres-compatible per-table INSERT file per source table into `database/postgres-inserts/`, preserving row order and using one statement per row.

**Why:** Keeps existing `database/` SQL assets untouched; makes replay/diff/debug straightforward; quotes identifiers to avoid Postgres parser hazards; converts SQLite empty strings to `NULL` for numeric/date-like fields (notably `People.weight`, `People.height`, `People.debut`, `People.finalGame`).

**Output Paths:** `database/postgres-inserts/{TableName}.sql` (27 files; e.g., `People.sql`, `Batting.sql`, `Teams.sql`, ...)

**Validation:** File counts and non-empty exports verified against `lahman.db` row counts.

---

## 2026-06-08 — Lambert: Lahman Postgres export approval

Reviewed per-table Postgres insert scripts against `lahman.db`.

- All Lahman tables have a matching non-empty `.sql` file.
- Identifiers are quoted, apostrophes are escaped correctly, and empty numeric SQLite values are represented safely.

**Decision:** ✅ Approved for landing.

---

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

---

## 2026-04-20 — Parker: Issue #4 Decision

**Decision:** Keep the first htmxRazor proof on `Pages/About.cshtml` and leave shared shell behaviors untouched.

**Key Points:**
- Treat `Pages/_ViewImports.cshtml` Tag Helper registration as mandatory baseline wiring
- Without `@addTagHelper *, htmxRazor`, `/_rhx/` assets can load while `rhx-*` tags still leak to browser as raw markup
- Keep component CSS imports centralized in `Pages/Shared/_Layout.cshtml` during Sprint 1
- Page migrations should not invent per-page asset strategies

**Status:** ✅ Decision locked for Sprint 1

---

## 2026-04-20 — Dallas: Sprint 1 UI — Shell & Primitives Decision

**Decision:** Land Issue #6 as a shell-chrome extraction only: `_Layout.cshtml` keeps ownership of `hx-boost`, `#modal-container`, global search ids/targets, and Bootstrap modal/dropdown lifecycle, while `_ShellHeader.cshtml` and `_ShellFooter.cshtml` carry the reusable navbar/footer markup.

**Safe #7 Included:**
- Harden shared primitives through `_EmptyState.cshtml` and `_LoadingSpinner.cshtml`
- Reuse `_LoadingSpinner` inside the existing loading-overlay wrappers on:
  - `Pages/Stats/Batting.cshtml`
  - `Pages/Stats/Pitching.cshtml`
  - `Pages/Awards/Index.cshtml`
  - `Pages/Postseason/Index.cshtml`
  - `Pages/Salaries/Index.cshtml`

**Blocker:**
- Do not start full `_FilterForm.cshtml` extraction until the post-#6 shell/container pattern is treated as stable
- That work touches page-owned `hx-target`/container contracts and is riskier than the safe loading-body reuse that landed here

**Status:** ✅ Approved for Sprint 1

---

## 2026-04-20 — Ash: Sprint 1 Platform Guardrails & Blockers

**Decision:** Sprint 1 foundation is sound. htmxRazor integration is already in place, caching patterns are correct, and the shared component approach is compatible with existing `[ResponseCache]` + `VaryByHeader = "HX-Request"` strategy.

**Critical Blocker Found & Fixed:**
- **Issue:** `_ViewImports.cshtml` missing htmxRazor tag helper registration
- **Fix Applied:** Added `@addTagHelper *, htmxRazor` to _ViewImports.cshtml
- **Impact:** Without this, all `rhx-*` tag helpers render as plain HTML instead of interactive components

**Platform Constraints Locked (9 total):**
1. Response Cache Variance Must Stay Locked — `VaryByHeader = "HX-Request"` mandatory
2. Static Assets & htmxRazor Foundation CSS Must Be Served First — middleware ordering locked
3. Tag Helper Registration (FIXED) — `@addTagHelper *, htmxRazor` required in _ViewImports.cshtml
4. Memory Cache TTL Consistency — all entries use 24-hour TTL
5. NoTracking Query Behavior Must Stay Global — locked in Program.cs
6. htmx Boost Behavior & Partial/Full Page Routing — use `IsHtmxNonBoostedRequest()` consistently
7. Modal Container Lifecycle During Shell Migration — modal cleanup JS must remain untouched
8. Component Asset Import Strategy — CSS in `/_rhx/css/components/`, JS in layout head only
9. HX-Request Header Caching — Client Side Only — keep `Location = ResponseCacheLocation.Client`

**Status:** ✅ All platform guardrails documented and locked

---

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

---

# 2026-06-08 — Ash: Lahman Postgres schema generation decision

**Decision:** Generate PostgreSQL table DDL directly from the live `lahman.db` metadata and preserve only the constraints present in that source database.

**Why:** The checked-in SQL assets already show migration drift across formats. Reading `PRAGMA table_info`, `PRAGMA index_list`, and `PRAGMA foreign_key_list` from the live SQLite file keeps table shapes, primary keys, unique constraints, and foreign keys aligned with the real dataset.

**Important nuance:** `AllstarFull` retains the `playerID` foreign key to `People`, but it should not gain a synthetic team foreign key. The live source omits that relationship, matching known historical all-star team rows that do not resolve cleanly through `Teams`.

**Artifacts:**
- `scripts/generate_postgres_schema.py`
- `database/postgres-schema/{TableName}.sql`
- `database/postgres-schema/all_tables.sql`

---

# 2026-06-08 — Lambert: Lahman Postgres Export Review

Approved.

## Validation

- All 27 Lahman tables have matching non-empty Postgres DDL files.
- Spot checks on People, Teams, Batting, HallOfFame, HomeGames, and TeamsHalf showed quoted identifiers, preserved keys, and sensible type mapping.
- No material migration bug found.

---

## 2026-06-08 — Ripley: Issue #18 Triage & Routing (Salaries Page Currency Formatting)

**Decision:** LOW-risk, straightforward Razor UI formatting fix routed to Dallas.

**Issue Summary:** Salary amounts display as plain numbers (e.g., "5000000") instead of formatted currency (e.g., "$5,000,000") across:
- Salaries page player salary displays
- Player salary history views
- Team payroll displays
- Highest-paid leaders leaderboards

**Routing Rationale:** Dallas owns Razor UI/component formatting and page markup rendering layer. Fix is display-layer formatting only: updating salary value rendering in Salaries.cshtml and related partials to use currency formatting (likely `@salary.ToString("C")` or similar).

**Risk Assessment:** LOW
- Isolated display formatting change
- No data retrieval or page logic affected
- No cross-page contract changes
- Straightforward Razor display fix
- No blocking dependencies; ready to pick up

**Actions Taken:**
- Applied label: `squad:dallas`
- Left triage comment with routing rationale and coordination notes

**Next Steps:** Dallas implements currency formatting across all salary displays; Lambert available for regression verification post-fix.

