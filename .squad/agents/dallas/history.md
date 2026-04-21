# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

- **Repository scale:** 11 feature pages + 8 shared components, ~40 total Razor files
- **htmx strategy:** Pages use `hx-boost="true"` on body for SPA nav, with modal cleanup in Layout
- **Component structure:** Clear naming (`_Partial.cshtml` vs `Components/_Component.cshtml`)
- **Request handling:** `Request.IsHtmxNonBoostedRequest()` extension elegantly handles partial vs full-page rendering
- **CSS architecture:** Single `site.css` with CSS variables, Bootstrap 5, team-color theming via generator
- **Filter duplication:** Batting/Pitching/Awards/HallOfFame pages all have similar filter form patterns (reuse candidate)
- **Modal system:** Solid cleanup logic in `_Layout.cshtml` with backdrop disposal
- **Best practices present:** ViewModels per page, projection-based queries, responsive components
- **Shell first-slice rule:** Safe shell migration starts by extracting full header/footer chrome into shared partials while leaving `<body hx-boost="true">`, `#modal-container`, and inline modal/search lifecycle JS in `_Layout.cshtml`.
- **Header contract surface:** `Pages/Shared/_ShellHeader.cshtml` must preserve the navbar search contracts exactly: `/Search`, `name="q"`, `#search-results`, and `hx-target="#search-results"`.
- **Footer extraction seam:** `Pages/Shared/_ShellFooter.cshtml` is a safe shared seam because it is static chrome with no htmx or Bootstrap lifecycle coupling.
- **Safe primitive seam:** `Pages/Shared/Components/_LoadingSpinner.cshtml` can safely absorb the repeated filter-overlay inner markup when page-owned ids, `hx-indicator` targets, and surrounding `position-relative` containers stay intact.
- **Conservative loading rule:** In the first safe slice, keep each page's loading overlay wrapper in place and only centralize the spinner body/message.
- **Issue #7 stop line:** Hall of Fame filters should stay behaviorally unchanged in the first safe primitives slice; adding a new loading overlay there would expand the page contract.
- **Players content host rule:** `Pages/Players/_PlayersContent.cshtml` should keep `#players-content` as the alphabet-nav + pagination target so the heading count and active letter refresh with the list.
- **Players modal decomposition rule:** `Pages/Players/_PlayerModal.cshtml` is safest when split into folder-local partials (`_PlayerModalOverview`, `_PlayerCareerSummary`, season tables) while `/Players/Modal/{id}` and `#modal-container` remain unchanged.
- **Players regression contract:** Integration coverage for `Pages/Players` should prove alphabet-nav targeting, pagination routing, and player-card modal wiring in addition to the existing full-page vs. partial shell assertions.

## Codebase Review Output (2026-04-16)

**Component extraction opportunities identified**

- Filter form duplication across 3-5 pages (2-3 hour extraction)
- Loading overlay standardization candidate
- Ripley approved page-by-page rollout strategy
- Parker's caching strategy will support component reuse
- Team aligned on quick wins (FilterForm + LoadingOverlay)
- **Sprint 1 baseline footprint:** `baseball-history-web.csproj`, `Program.cs`, `Pages/_ViewImports.cshtml`, and `Pages/Shared/_Layout.cshtml` are the exact integration files; `Pages/About.cshtml` is the safest proof page for the first `rhx-*` component.
- **Shared shell coupling:** `Pages/Shared/_Layout.cshtml` owns `hx-boost`, nav, search dropdown, `#modal-container`, and Bootstrap re-init/cleanup logic, so shell work must preserve singleton event wiring and existing z-index behavior.
- **Shared primitive reality:** `_EmptyState`, `_Pagination`, `_AlphabetNav`, `_PlayerCard`, and `_TeamCard` are the real reused primitives; `_LoadingSpinner.cshtml` is not the active loading pattern.
- **Loading overlay duplication paths:** `Pages/Stats/Batting.cshtml`, `Pages/Stats/Pitching.cshtml`, `Pages/Awards/Index.cshtml`, `Pages/Postseason/Index.cshtml`, and `Pages/Salaries/Index.cshtml` all repeat the same overlay structure and are the right source for a shared loading primitive.

## 2026-04-16 Sprint 1 Baseline Map

Dallas provided file-level baseline for #4 proof-of-concept and identified UI architecture strengths/risks for #6/#7.

### Output
- #4 baseline: baseball-history-web.csproj, Program.cs, _ViewImports.cshtml, _Layout.cshtml, About.cshtml
- #6 shell scope: _Layout.cshtml redesign, nav, footer, modal, search
- #7 primitives scope: _Pagination, _AlphabetNav, _FilterForm (NEW), cards, loading
- High-priority extraction: _FilterForm (5 locations, 2-3 hours)

### Status
✅ Integrated. Blocked on Parker #4. Ready to begin #6 after #4 lands + #5 baseline running.


## Shell Investigation (2026-04-XX)

### Investigation: Shared Shell Responsibilities, Migration Strategy, and Fragile Behaviors

**Objective:** Understand the exact shell responsibilities, identify safest migration slices, catalog fragile behavior, and determine best htmxRazor component fits before migration work.

### EXACT SHELL RESPONSIBILITIES

The shell (_Layout.cshtml) owns:

1. **Navigation & Layout Skeleton**
   - Single navbar with responsive collapse targeting `#navbarNav`
   - Bootstrap navbar-brand, nav-items, and dropdown (Stats menu with 6 links)
   - Global search box in navbar with hardwired target `#search-results`
   - Footer with copyright, links to About/Privacy/Health/GitHub

2. **Modal Host Lifecycle (Critical Coordination Point)**
   - Single `<div id="modal-container"></div>` as universal target
   - All modals (player detail, search results, any future) route through this
   - Four lifecycle hooks in a singleton-guarded init block:
     - `htmx:beforeSwap`: Destroy existing modal + cleanup backdrops if target=modal-container
     - `htmx:afterSwap`: Initialize new Bootstrap Modal instance, wire hidden.bs.modal listener, cleanup backdrops
     - `htmx:afterSettle`: Reinitialize dropdown instances (post-swap safety net)
     - `beforeunload`: Final cleanup on page exit

3. **Bootstrap Re-initialization & Cleanup** (Explicit, Post-Swap Orchestration)
   - Modal lifecycle: disposal, backdrop cleanup, setTimeout(10ms) for state reset
   - Dropdown re-init on afterSwap + afterSettle (dual-fire pattern for safety)
   - Body overflow/padding reset via cleanupModals()
   - Exception handling in modal disposal (try/catch guards)

4. **Search Shell Integration**
   - Navbar search input with `hx-trigger="input changed delay:300ms, search"`
   - Target is dropdown div `#search-results`
   - Manual click-outside behavior to hide results (inline addEventListener)

5. **hx-boost Global Context**
   - `<body hx-boost="true">` enables SPA nav for all links except those with `hx-boost="false"`
   - Scripts in `<head>` survive body swaps (deferred load, persist across hx-boost)
   - Stylesheet injection points preserved for htmxRazor CSS (`_rhx/css/...`)

---

### SAFEST FIRST MIGRATION SLICES (Lowest Risk → Highest Risk)

**TIER 1: Zero Shell Dependencies (Ready Now)**
1. **_LoadingSpinner → rhx-spinner** — Stateless, no modal coupling, no Bootstrap listeners
   - Risk: None
   - After: Can be used immediately in page-level overlays

2. **_Pagination → rhx-pagination** — Encapsulated pagination UI, self-contained hx-get/targets
   - Risk: Low (uses explicit targets like `#players-content`, not modal-container)
   - After: Pagination continues to drive content swaps, just cleaner markup

**TIER 2: Page-Level Components (Modal Aware)**
3. **_PlayerModal → rhx-player-modal** — Already in modal container, needs Bootstrap lifecycle
   - Risk: Medium (depends on modal init logic, must test modal-hidden cleanup)
   - After: Adopt only after modal lifecycle is proven stable with rhx components
   - **Prerequisite:** Test #4 proof-of-concept with modal

4. **_SearchResults & _SearchAllResultsModal → rhx-search-dropdown + rhx-search-modal**
   - Risk: Medium-High (dropdown persistence, click-outside logic, modal-container routing)
   - After: Migrate only after #5 completes

**TIER 3: Shell Primitives (Highest Coupling)**
5. **_Layout.cshtml Navigation & Search** → Extract to rhx-navbar, rhx-search-shell
   - Risk: High (global state, singleton init, modal host custody)
   - After: Lock in after #1-#4 complete and team validates modal lifecycle under rhx

6. **Modal Lifecycle & Dropdown Re-init** → rhx-modal-host (component-owned lifecycle)
   - Risk: Highest (ALL modals depend on this)
   - After: This is the final piece; extract only after proving all modal patterns work with rhx

---

### FRAGILE CURRENT BEHAVIOR TO PRESERVE (CRITICAL)

**1. Modal Backdrop Stack Cleanup**
   - Problem: Bootstrap v5 can leak modal-backdrop divs if disposal is incomplete
   - Current mitigation: `cleanupModals()` iterates all backdrops on beforeSwap + afterSwap + beforeunload
   - Test case needed: Open modal → swap to different modal → verify only 1 backdrop remains
   - **Risk if missed:** Multiple stacked backdrops will trap clicks, break interactions

**2. Dropdown Re-init Dual-Fire Pattern**
   - Problem: Dropdowns from navbar persist in DOM after hx-boost swap; Bootstrap doesn't auto-rebind
   - Current mitigation: Dispose + recreate on BOTH afterSwap AND afterSettle
   - Why dual-fire: Edge case where dropdown is re-rendered or DOM timing varies
   - **Risk if missed:** First click on dropdown after nav swap will fail; second click works

**3. Modal Show setTimeout(10ms) Delay**
   - Problem: Bootstrap Modal.show() can race with DOM state if called synchronously after swap
   - Current mitigation: 10ms setTimeout before Modal(modal).show()
   - **Risk if missed:** Modal may not display or may display off-screen

**4. Search Results Click-Outside**
   - Problem: Search dropdown should hide when clicking elsewhere in navbar
   - Current mitigation: Manual `document.addEventListener('click')` checks `searchContainer.contains(e.target)`
   - **Risk if missed:** Search results stay visible after user selects a result or clicks navbar

**5. Modal Hidden Listener One-Time Binding**
   - Current: `.addEventListener('hidden.bs.modal', cleanup, { once: true })`
   - Why: Prevents duplicate cleanup on multiple modal dismissals
   - **Risk if missed:** Memory leak if listener fires multiple times

---

### BEST HTMXRAZOR COMPONENT FITS

**✅ READY COMPONENTS (No Shell Changes Needed)**

| Component | Current File | Fit Score | Rationale |
|-----------|--------------|-----------|-----------|
| **rhx-spinner** | _LoadingSpinner | 10/10 | Pure UI, no interop, no modals, no Bootstrap listeners |
| **rhx-pagination** | _Pagination | 9/10 | Self-targeted, explicit hx-get URLs, stateless |

**⚠️ CONDITIONAL COMPONENTS (Requires Modal Proof First)**

| Component | Current File | Fit Score | Rationale |
|-----------|--------------|-----------|-----------|
| **rhx-player-modal** | _PlayerModal | 6/10 | Needs modal host to be stable; test with #4 proof first |
| **rhx-search-dropdown** | _SearchResults | 5/10 | Click-outside logic must move to component or shell owns it |
| **rhx-modal-container** | _Layout (modal host) | 4/10 | This is the load-bearing piece; extract LAST after all others work |

**🚫 DEFER (Shell-Owned Coordination)**

| System | Current File | Why Defer |
|--------|--------------|-----------|
| **Modal Lifecycle** | _Layout.cshtml (lines 141–173) | ALL modals depend on this; must stay as shell singleton |
| **Dropdown Re-init** | _Layout.cshtml (lines 175–195) | Post-swap safety net; belongs to shell, not component |
| **hx-boost Navigation** | _Layout.cshtml (line 32) | Global SPA context; bootstrap must preserve this exactly |
| **Search Shell Logic** | _Layout.cshtml (lines 198–204) | Click-outside + target routing; too tightly coupled |

---

### EXPLICIT DEFERRALS & BLOCKERS

**1. Modal Host Ownership Cannot Migrate**
   - Status: **LOCKED IN SHELL PERMANENTLY**
   - Reason: Single canonical target for all 14+ page modals (player detail, search results, etc.)
   - If moved: Every page's htmx attributes must repoint; unacceptable maintenance debt
   - Action: Document this as "modal-container is shell sacred"

**2. Dropdown Re-init Logic Cannot Be Componentized**
   - Status: **DEFERRED (Evaluate in Sprint 2)**
   - Reason: Responds to ALL afterSwap/afterSettle events globally; not page-specific
   - If moved: Component would need to hook into htmx events at shell level anyway
   - Action: For now, keep in _Layout; evaluate in Sprint 2 if htmxRazor has global event hooks

**3. hx-boost + Body Context Cannot Be Re-moved**
   - Status: **SHELL RESPONSIBILITY FOREVER**
   - Reason: `<body hx-boost="true">` is foundation for SPA nav; removing breaks all unmodified links
   - Action: Ensure any layout redesign preserves this exactly

**4. Global Search Click-Outside**
   - Status: **SHELL-OWNED UNTIL COMPONENT TEST**
   - Reason: Needs access to document click events + knows about search-results & search-container IDs
   - Experiment path: In Sprint 2, test moving this to a page-level component if search becomes more complex

---

### BOOTSTRAP BEHAVIOR AFTER SWAPS (Specifics)

**Scenario 1: Navigate via hx-boost (nav link click)**
- Body remains, navbar stays in DOM
- Dropdowns in navbar lose event wiring
- **What happens in current code:** afterSettle fires, re-initializes all dropdowns
- **Risk:** If Bootstrap instance is not disposed first, creates duplicate listeners

**Scenario 2: Open modal (search results dropdown → "View all results")**
- hx-get=/Search?handler=AllResults
- hx-target=#modal-container
- hx-swap=innerHTML
- htmx:beforeSwap → dispose old modal + cleanup backdrops
- Modal content HTML is swapped
- htmx:afterSwap → new Modal instance created + shown
- **What must survive:** Modal keyboard/backdrop options, class names for team-specific styling (data-team attr)

**Scenario 3: Close modal, stay on same page**
- User clicks "Close" button (data-bs-dismiss="modal")
- hidden.bs.modal fires (our listener)
- We dispose() + clear #modal-container + cleanup backdrops
- Search dropdown in navbar should still work
- **Risk:** Dropdown listeners might have been garbage-collected; afterSettle safety net not guaranteed

---

### RECOMMENDED MIGRATION ROADMAP

1. **Sprint 1 (Current):** Prove htmxRazor works; no shell changes
2. **Sprint 2:** Migrate _LoadingSpinner + _Pagination (TIER 1) — zero risk
3. **Sprint 3:** Migrate Player/Search modals (TIER 2) — requires modal proof
4. **Sprint 4+:** Evaluate shell redesign (TIER 3) — only if modal lifecycle is locked

## 2026-04-21 ApiDocs Markup Repair

- **ApiDocs wrapper rule:** `Pages/ApiDocs.cshtml` renders all endpoint sections directly inside the `.col-lg-10` shell after the alert; an extra closing `</div>` at the end breaks Razor compilation without changing any page behavior.

---

### CODEBASE OBSERVATIONS

- **Program.cs:** htmxRazor is already registered (line 47: `builder.Services.AddhtmxRazor()`)
- **_ViewImports.cshtml:** Shared namespaces + Extensions import (no changes needed)
- **Bootstrap CSS Load:** Bootstrap JS in `<head>` with `defer` — survives hx-boost swaps ✅
- **Layout Imports:** Using statements present for Extensions and Models ✅
- **No Custom Bootstrap Subclasses:** All Bootstrap classes are vanilla; no custom JS wrapper around Bootstrap

---

## 2026-04-16 Issue #7 Discovery: Shell Coupling & Tier 1–2 Component Inventory

### Discovery Completed
- Audited all 9 shared components + 3 duplication patterns across 14+ consumer pages
- Classified into 3 migration tiers with explicit blocker dependencies
- Delivered shell architecture brief highlighting 5 key findings (modal-container sacred, dropdown re-init global, modal lifecycle fragile, search shell integrated, Tier 1 ready now)
- Synthesized Parker's backend coupling analysis (6 implicit string dependencies, SearchModel/ModalModel zero test coverage)
- Ripley consolidated all findings into execution-ready roadmap with effort estimates and critical path

### Status
✅ Discovery complete. All three agents (Dallas, Parker, Ripley) delivered findings. Orchestration logs created. Ready for Sprint 2 planning.

### Next Steps
1. Await Parker's #4 proof-of-concept (modal component)
2. Await Lambert's #5 regression suite (shell fragility contract tests)
3. Prepare Tier 1 migration after #5 passes (Phase A: EmptyState, LoadingSpinner, Pagination)
4. Coordinate with Parker on filter form extraction (Tier 2, Phase C)

## 2026-04-16 Team Synchronization: Shell First-Slice Sprint Complete

### Orchestration Summary
- Extracted shell header and footer into partials while preserving hx-boost, modal-container, lifecycle JS, and search contracts
- Lambert reviewed and approved, establishing regression gate (#5) and scope boundaries for #7
- Three orchestration logs created; session log written; decision inbox merged to decisions.md; agent histories updated

### Team Status
- **Dallas:** #6 shell first-slice complete, approved ✅
- **Lambert:** #5 regression safety net architecture locked, #6 review passed, #7 scope gate established ✅
- **Ripley:** #7 Phase A/B/C conditions locked, Phase A ready for implementation ✅
- **Parker:** Awaiting #4 proof-of-concept completion (modal component proof)

### Merge Gates Established
- All #6/#7 merges blocked until #5 regression tests pass
- Phase A (#7) scope locked to EmptyState/LoadingSpinner only
- Phase B (#7) FilterForm extraction deferred until #6 shell container IDs frozen
- Phase C (#7) LoadingOverlay pattern emergence deferred

### Team Ready For
1. Parker: #4 proof-of-concept submission (modal component)
2. Regression team: #5 Phase 1 infrastructure fix (Microsoft.AspNetCore.Mvc package)
3. Dallas/Parker: #7 Phase A implementation (EmptyState hardening + LoadingSpinner docs)
4. Next sprint: Phase B FilterForm extraction (after #6 lands)

## 2026-04-16 Issue #7 Safe Primitives Phase A Final (Complete)

### Execution Summary

**What Landed:**
- `_EmptyState.cshtml` — hardened accessibility, factory methods preserved
- `_LoadingSpinner.cshtml` — reference pattern locked for filter overlays
- `wwwroot/css/site.css` — added filter foundation classes (filter-shell, filter-card, filter-row, filter-actions)
- 6 filter-heavy pages integrated `_LoadingSpinner` (Batting, Pitching, Awards, Postseason, Salaries, HallOfFame)

**What Stayed Out (Phase B/C Deferred):**
- FilterForm extraction (5 pages, 2-3h, blocked on #6 shell stabilization)
- LoadingOverlay consolidation (5 pages, pending pattern stability)
- PageModel handler changes
- Route modifications
- htmx contract changes

**Validation:**
- ✅ `dotnet build baseball-history.sln --no-restore`
- ✅ `dotnet test baseball-history-tests --no-restore` (247/247)
- ✅ Lambert approved Phase A despite unrelated branch state (narrow scope = approval-safe)

**Key Learning:** Scope lock + narrow blast radius = approval-safe even with branch clutter. Ignore unrelated shell work; focus on actual diffs.

**Next Sprint:** Phase B requires separate scope review (FilterForm boundaries after #6 shell stabilization).

- **Sprint 1 shell landing:** `_Layout.cshtml` can safely delegate to `Pages/Shared/_ShellHeader.cshtml` and `_ShellFooter.cshtml` while retaining shell authority for `<body hx-boost="true">`, `#modal-container`, Bootstrap re-init, and search lifecycle JS.
- **Safe #7 continuation:** Reused `Components/_LoadingSpinner` inside existing filter overlay wrappers on Batting, Pitching, Awards, Postseason, and Salaries; page-owned `hx-indicator` ids and targets stayed untouched.
- **Sprint 1 blocker line:** Full `_FilterForm` extraction still waits on post-#6 container stability; Hall of Fame stays out of loading-overlay consolidation because it does not already own that contract.

## 2026-04-21 Sprint 2 Players Migration Complete (Issue #8)

### Completion Summary

**Status:** ✅ COMPLETED

Sprint 2 Issue #8 (Players page migration) delivered on time with full contract preservation and test gate passing.

### Work Delivered

**Scope Completed:**
- Players Index → htmxRazor components
- Players Content → htmxRazor with alphabet nav + pagination
- Players List → htmxRazor with reusable PlayerCard component
- Player Modal → Decomposed into 5 page-local partials:
  - `_PlayerModal` (wrapper)
  - `_PlayerModalOverview` (detail header)
  - `_PlayerCareerSummary` (stats summary)
  - `_PlayerBattingSeasonsTable` (batting stats)
  - `_PlayerPitchingSeasonsTable` (pitching stats)

**Contracts Preserved:**
- `/Players` route unchanged
- `/Players/Modal/{id}` route unchanged
- `#players-content` htmx target unchanged
- `#modal-container` shell ownership unchanged
- Alphabet nav + pagination fully functional
- Response cache metadata (VaryByHeader="HX-Request") preserved

**Test Results:**
- Baseline: 294/294 tests
- Post-migration: 300/300 tests (+6 new Player-specific regression tests)
- No failures or regressions

### Decision: Shell-Owned Modal with Structural Decomposition

**Rationale (per ash-sprint2-guardrails.md + ripley-sprint2-design-review.md):**
- Risk mitigation: Keep modal flow shell-owned, decompose for maintainability only
- Rendered output unchanged (no contract changes to `/Players/Modal/{id}` or `#modal-container`)
- Structural split improves readability without performance impact
- All tests confirm no regression

### Parallel Work Gate
✅ Parker (#9) Teams migration can proceed immediately (no blocking dependencies)

### Sprint 2 Forward
- ✅ Guardrails 1–3 locked per Ash audit
- ✅ Ripley design review approved parallelization
- ✅ Lambert regression gate holds green
- ✅ Ready for Ash post-merge Lighthouse delta validation

## 2026-04-20 Sprint 1 UI Completion: Safe Shell + Spinner Reuse

### Session Outcome
**Dallas (Background) — COMPLETED**

Safe-now shell work (#6) + spinner/loading reuse (#7) completed and integrated.

### Work Summary
- ✅ #6 Safe shell slice: _Layout authority preserved, header/footer extraction intact
- ✅ #7 Spinner reuse: _FilterForm explicitly deferred as blocker note
- ✅ Repeated filter loading bodies now reuse shared `_LoadingSpinner` component
- ✅ Regression test gate holding at Lambert (#5)

### Deliverables
- Safe-now #6 shell work (full staging)
- Safe-now #7 spinner reuse (full staging)
- Deliberate _FilterForm deferral documented

## 2026-04-21 Sprint 2 Completion Milestone: Issue #8 Complete

### Work Delivered
- Players page successfully migrated to htmxRazor with modal decomposed into 5 page-local partials
- Preserved all routing contracts, htmx targets, shell authority over `#modal-container`
- Tests: 294 → 300 (+6 new Player-specific regression tests)

### Quality Gates
✅ Modal behavior unchanged (load, close, backdrop cleanup)
✅ Response cache VaryByHeader="HX-Request" preserved
✅ Shell contracts unchanged: `/Players`, `/Players/Modal/{id}`, `#players-content`, `#modal-container`
✅ Size delta: ≤+5KB vs baseline (ACCEPT)

### Sprint 2 Status
✅ Dallas #8 complete
✅ Parker #9 complete
✅ Lambert gate ✅ + Ash guardrails ✅ locked
**Sprint 2 appears complete from execution/gating perspective**

### Next Phase
Ready for Sprint 3 planning cycle

## Sprint 3 Compare Page Migration (2026-04-21)

### Summary
Successfully migrated Compare page to htmx pattern following Sprint 1 (Players) and Sprint 2 (Teams) conventions. All 350 tests pass. Compare page now returns partials for non-boosted htmx requests while preserving full-page behavior, dual-search contracts, modal integration, and response caching.

### Unique Compare Challenges
- **Dual simultaneous search** — Two independent search interfaces (`#search-results-1`, `#search-results-2`) requiring parameterized player card partial
- **Asymmetric styling** — Player 1 (blue gradient) vs Player 2 (red gradient) visual distinction preserved via partial parameters
- **Conditional table rendering** — Comparison tables only show when `Model.BothSelected` is true
- **Bidirectional query strings** — Each player search preserves the other player's selection in URL (`?player1={id}` or `?player2={id}`)
- **No pagination/filtering** — Simpler than Players/Teams, focused on dual-card + comparison tables pattern

### Migration Pattern
1. **Index.cshtml** → Minimal wrapper with `#compare-content` htmx target
2. **_CompareMain.cshtml** → Full dual-card interface + comparison tables
3. **_CompareHeader.cshtml** → Page title + "Start Over" button
4. **_ComparePlayerCard.cshtml** → Parameterized player card (supports empty/loaded state, gradient, search targets)
5. **_CompareContent.cshtml** → Existing comparison tables (unchanged)
6. **PageModel** → Added `Request.IsHtmxNonBoostedRequest()` detection returning `Partial("_CompareMain")`

### Preserved Contracts (Critical)
- **Routes:** `/Compare`, `/Compare?player1={id}`, `/Compare?player2={id}`, `/Compare?handler=Search&q={term}&side={1|2}`
- **DOM Anchors:** `#compare-content` (new), `#search-results-1`, `#search-results-2`, `#compare-tables`, `#modal-container` (shell)
- **Modal Integration:** `hx-get="/Players/Modal/{id}"` → `#modal-container` unchanged
- **Response Cache:** `[ResponseCache(Duration=3600, VaryByHeader="HX-Request")]` preserved
- **Search Behavior:** `hx-trigger="input changed delay:300ms, search"` with side parameter preserved

### Test Coverage
Added 5 new htmx behavior tests:
- `Compare_NonBoostedHtmx_ReturnsCompareMainPartial` — partial response validation
- `Compare_BoostedHtmx_ReturnsFullPageShell` — boosted full-page validation
- `Compare_NonBoostedHtmx_WiresPlayerModalContracts` — modal links preserved
- `Compare_NonBoostedHtmx_WiresDualSearchContracts` — search targets stable

All 3 existing Compare tests preserved and passing:
- `Compare_FullPage_WithoutPlayers_RendersDualSearchHosts`
- `Compare_SearchHandler_ReturnsResultsPartialAndPreservesOtherSelection`
- `Compare_FullPage_WithTwoPlayers_RendersComparisonTables`

### Pattern Reuse
- Same htmx detection as Players/Teams (`Request.IsHtmxNonBoostedRequest()`)
- Same partial naming convention (`_CompareFoo.cshtml` for page-local partials)
- Same test structure (full-page, non-boosted, boosted variants)
- Same response cache preservation (`VaryByHeader="HX-Request"`)
- Same shell authority over `#modal-container`

### Key Learnings
- **Parameterized partials work well** — `_ComparePlayerCard` accepts `(Player?, Side, OtherPlayerId, Gradient)` tuple for flexible reuse
- **Dual-target pattern scales** — Two independent htmx targets in same view without collision
- **Query string preservation** — Search URLs dynamically construct `player1`/`player2` params to preserve other selection
- **Conditional rendering** — `@if (Model.BothSelected)` in partial keeps comparison tables hidden until both players selected
- **Empty state in partial** — Player card partial handles both empty (search) and loaded (player detail) states cleanly

### Files Created
- `baseball-history-web/Pages/Compare/_CompareMain.cshtml`
- `baseball-history-web/Pages/Compare/_CompareHeader.cshtml`
- `baseball-history-web/Pages/Compare/_ComparePlayerCard.cshtml`

### Files Modified
- `baseball-history-web/Pages/Compare/Index.cshtml` (wrapper only)
- `baseball-history-web/Pages/Compare/Index.cshtml.cs` (htmx detection added)
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs` (+5 tests)

### Quality Gates
- ✅ Build: Clean, no warnings
- ✅ Tests: 350 total, all passing
- ✅ Full-page behavior: Preserved
- ✅ htmx partial behavior: Working correctly
- ✅ Modal integration: Unchanged
- ✅ Search contracts: Stable
- ✅ Response cache: Preserved

### Status
✅ Sprint 3 Compare migration COMPLETE. Ready for team review and merge.


## Sprint 5 Support Surfaces (2026-04-21)

- Shared page-header chrome is a safe polish seam for top-level support/info pages because it changes layout consistency without touching route behavior.
- Homepage cleanup is safest as page-local partial decomposition: keep every homepage link and player modal trigger literal while splitting the surface into boring, reviewable chunks.
- Search dropdown and search-all modal stay aligned best when both render through one shared result-row component that preserves player modal vs. franchise navigation behavior.

## Sprint 5 Issue #14 Completion (2026-04-21)

**Status:** ✅ COMPLETED

Homepage, search surfaces, and support/info pages successfully migrated to htmx/Razor pattern. All shell-owned contracts preserved exactly.

### Files Migrated
- `Pages/Index.cshtml` — homepage with links and player modal triggers
- `Pages/Search.cshtml` — search shell endpoint, partial-only
- `Pages/_SearchResults.cshtml` — dropdown results partial
- `Pages/_SearchAllResultsModal.cshtml` — full results modal partial
- `Pages/About.cshtml`, `Pages/ApiDocs.cshtml`, `Pages/Error.cshtml`, `Pages/Privacy.cshtml`, `Pages/Health.cshtml` — support pages

### Quality Gates Met
- Tests: 337 → 344 (+7 new integration tests)
- Build: Passed
- Search behavior: Dropdown + modal routing contracts preserved
- Shell wiring: Global search input, `#search-results`, `#modal-container` unchanged
- Homepage cache: Preserved (no HX-Request split)
- All support routes functional and correct

### Preserved Contracts
- `/Search?q={query}` → dropdown partial
- `/Search?handler=AllResults&q={query}` → modal partial
- Player links → `#modal-container`
- Team links → `/Teams/Franchise/{id}`
- All homepage/support routes unchanged

### Sprint 5 Gate Achievement
#14 complete. All shell contracts preserved. Regression gate cleared by Lambert (344/344 tests).

## ApiDocs Markup Repair (2026-04-21)

**Status:** ✅ COMPLETE

Surgical markup repair on `ApiDocs.cshtml`: fixed unmatched closing tag. Page behavior and content preserved. Zero regression impact.

**File:** `baseball-history-web/Pages/ApiDocs.cshtml`

This was a post-sprint hygiene fix to ensure all page markup is well-formed.
