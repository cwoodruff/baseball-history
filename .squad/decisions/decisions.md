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

## 2026-04-16 — Lambert: Regression Safety Net Split (Issue #5)

**Decision:** Keep Sprint 1 issue #5 regression coverage in three WebApplicationFactory-backed integration slices:

1. `Pages/PageRoutingIntegrationTests.cs` — Full-page vs non-boosted htmx routing contracts
2. `Pages/PagePaginationIntegrationTests.cs` — Boundary clamping via rendered pagination summaries
3. `ApiEdgeIntegrationTests.cs` — Representative player/team 404 paths

**Why:** This split keeps migration-risk assertions behavior-focused and reviewable without brittle HTML snapshots. Provides clear merge gate: shell or shared-partial changes should not land unless these contract tests stay green.

**Scope Covered:**
- Stats/Batting full vs htmx
- Stats/Pitching full vs htmx
- Teams index full vs htmx
- Pagination boundaries for Players, Stats/Batting, Stats/Pitching
- API 404s for invalid player and team routes

**Gate:** All #6/#7 merges blocked until #5 regression tests pass

**Status:** ✅ Approved, architecture decision locked

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
