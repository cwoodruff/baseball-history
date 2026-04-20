# Squad Decisions

## Issue #5 Sprint 1 Gate Hardening (2026-04-20)

### Lambert — Issue #5 Gate Hardening

**Status:** ✅ IMPLEMENTED

Sprint 1 regression approval should rely on **behavioral contract assertions**, not shallow smoke markers.

**Rationale:**
- The migration risk is concentrated at full-page shell boundaries, non-boosted htmx partial handlers, modal hosts, and pagination clamping.
- Tests that only prove "response contains a div" are too weak to block Sprint 2 regressions.

**Applied Gate:**
- Full-page tests now prove shared shell markers (`hx-boost`, search host, modal host).
- Partial-handler tests now prove those shell wrappers are absent.
- Pagination tests now exercise the real htmx path and parse `Page X of Y` summaries.
- API smoke now covers representative happy paths in addition to existing 404 edges.

**Validation:**
- ✅ 294/294 tests green
- ✅ Sprint 1 gate standard updated
- ✅ Sprint 2 cleared for parallel Issue #6/#7 work

---

## Issue #7 Safe Primitives Final Decisions (2026-04-16)

### Parker — Safe Primitives Phase A Revision Decision

**Status:** ✅ APPROVED

For Issue #7 rereview, Phase A is component-only. The live slice is locked to `_EmptyState.cshtml`, `_LoadingSpinner.cshtml`, and minimum shared CSS.

**Rationale:** Reviewer guidance explicitly rejected page-level filter-shell extraction and loading-overlay rewiring. No reason to change handler contracts or routes for primitive markup refinements.

**Scope (In):**
- `baseball-history-web/Pages/Shared/Components/_EmptyState.cshtml`
- `baseball-history-web/Pages/Shared/Components/_LoadingSpinner.cshtml`
- `baseball-history-web/wwwroot/css/site.css`

**Scope (Out):**
- Page-level filter wrappers
- Loading-overlay rewiring
- PageModel handler changes
- Route modifications
- htmx contract changes

**Guardrails:**
- No PageModel, route, query-string, `hx-target`, `hx-include`, or `hx-push-url` contract changes
- Future filter-form extraction must return as separate follow-up slice

---

### Dallas — Safe Primitives Slice Implementation Decision

**Status:** ✅ IMPLEMENTED

Standardized active filter loading body through `_LoadingSpinner.cshtml`, keeping each page's existing `hx-indicator` id and filter container contract intact.

**Applied:**
- Reused `_LoadingSpinner` in filter-heavy pages (Batting, Pitching, Awards, Postseason, Salaries, HallOfFame)
- Added additive filter-foundation classes (`filter-shell`, `filter-card`, `filter-row`, `filter-actions`) to `site.css`
- Preserved all existing `hx-get`, `hx-target`, `hx-include`, `hx-push-url`
- Added `hx-indicator` only to existing filter controls without handler changes

**Validation:**
- ✅ `dotnet build baseball-history.sln --no-restore` passed
- ✅ `dotnet test baseball-history-tests --no-restore` passed (247/247)

---

### Lambert — Issue #7 Phase A Re-Review Decision

**Status:** ✅ APPROVED

Current Issue #7 Phase A revision approved for landing.

**Review Findings:**
- Green validation: `dotnet build` succeeded, `dotnet test` passed 276/276
- Live working-tree slice stays within Phase A boundary: `_EmptyState.cshtml`, `_LoadingSpinner.cshtml`, `site.css` only
- Phase A guards all hold:
  - `EmptyStateModel` signature unchanged
  - `_LoadingSpinner` still uses immutable `string?` model
  - No filter-heavy pages, `_ViewImports.cshtml`, or page-level contracts modified
- Unrelated branch state (shell files, proof-of-integration work) does not block narrowly-scoped approval

**Guidance:** Ignore unrelated preexisting shell work when evaluating Phase A. Blast radius is narrow and stable.

**Next Steps:** Phase A may proceed. Future filter/loading-overlay extraction must return as separate follow-up review.

---

## Codebase Review Findings (2026-04-16)

### Architecture Review — Ripley

**Status:** ✅ Well-structured and migration-ready. No architectural blockers for htmxRazor migration.

**Key Strengths:**
- Razor Pages + htmx foundation is production-proven (not speculative)
- Data access discipline: Global `QueryTrackingBehavior.NoTracking`, projection pattern consistent
- Caching strategy is thoughtful: 24-hour memory cache TTL, pre-warmed player cache, `[ResponseCache]` properly uses `VaryByHeader = "HX-Request"`
- Service architecture clean: Only 2 services (TeamColorService singleton, PlayerCacheService hosted), both follow SRP
- ViewModels provide clean data shapes with factory methods
- Database schema normalized, 28+ DbSets properly configured

**Migration Path:** Page-by-page rollout (not component-by-shared-component)
1. Extract shared components → all pages use new versions
2. Migrate Shared.Layouts, Footer, Navigation (app chrome)
3. Migrate Players page (highest traffic, lowest complexity)
4. Migrate Teams, Compare, Awards, Salaries (mid-tier)
5. Migrate Stats/Leaders (highest complexity, defer to maturity)

**Risks (Minor):**
- Leaderboard expression trees duplicated (API vs Pages) — maintenance burden, but isolated. Extract to shared utility later (not blocking)
- Page model sizes growing (largest 246 lines) — spike on service extraction post-migration
- No API client tests — optional, low priority
- String fields in database need parsing (Batting.Rbi, Pitching.Hr) — document in DATABASE.md

**Team Readiness:** All well-chartered. Ready to begin.

---

### UI Architecture & htmx Patterns — Dallas

**Status:** ✅ Strong component-based architecture. Biggest win is extracting shared filter/loading components.

**Strengths:**
- 8 reusable shared components in `Pages/Shared/Components/` with clean composition
- Consistent htmx request handling: `Request.IsHtmxNonBoostedRequest()` used throughout
- Professional CSS architecture: single `site.css` with variables, team-color system

**HIGH PRIORITY: Filter Form Component Duplication**
- **Impact:** 3 pages (Batting, Pitching, Awards) + 2 variants (HallOfFame, Postseason)
- **Issue:** Each rebuilds identical filter-select patterns, repeated htmx attributes
- **Recommendation:** Extract to reusable `_FilterForm.cshtml` component with slots for fields
- **Effort:** 2-3 hours

**MEDIUM PRIORITY: Duplication**
1. **Compare page player selection cards:** ~120 lines duplicated (extract `_ComparePlayerCard.cshtml` with side parameter)
2. **Loading indicator:** 5+ pages use custom overlay — standardize with `_LoadingOverlay.cshtml`

**LOW PRIORITY:**
- **Sortable table headers:** Batting/Pitching leaderboards have repetitive column links (could extract `_SortableTableHeader.cshtml`)

**Quick Wins (2-3 hours):**
1. Extract `_FilterForm.cshtml`
2. Extract `_LoadingOverlay.cshtml`

---

### Backend Structure & htmx Caching — Parker

**Status:** ✅ Clean handler patterns, projection-first queries, htmx-aware caching.

**Key Findings:**
- All 19 PageModel handlers are `OnGetAsync()` with primary constructor injection
- Partial/full logic decoupled via `Request.IsHtmxNonBoostedRequest()` extension
- Projection-first query execution with `.Select()` throughout, global `NoTracking` behavior
- `[ResponseCache]` with `VaryByHeader = "HX-Request"` — htmx partials cached separately from full pages
- Pre-warmed cache via `PlayerCacheService` at startup

**Duplication Note:** Leaderboard ordering expression trees exist in both `Api/Endpoints/` and Razor PageModels. Not a migration blocker but consider extracting to shared helper later.

**Confidence:** High. Backend seams are clean, minimal coupling, well-tested at integration level. Ready for UI migration.

**Next:** Frontend can design htmxRazor component boundaries without backend concerns. API layer needs no changes. Keep three-tier caching strategy intact during view layer refactoring.

---

### Test Coverage & Regression Risk — Lambert

**Status:** ⚠️ 247 tests passing, but critical gaps in page model/API coverage raise migration regression risk.

**What's Well Tested:**
- ✅ Database layer: 30+ integration tests covering all major DbSets, FK navigation verified, DateOnly converter tested
- ✅ htmx extensions: 21 comprehensive tests for request detection and response headers
- ✅ Selected ViewModels: PlayerDetailViewModel, LeaderboardViewModel, TeamSeasonViewModel tested

**Critical Gaps (Zero Coverage):**
- 🔴 **Page models:** 0/19 tested (Search, Players, Stats, Teams, Awards, HoF, Salaries, Postseason, Compare)
- 🔴 **API endpoints:** 0/20 tested (Result.NotFound() conditions untested)
- 🔴 **ViewModels:** 9 untested (AlphabetNav, AwardVoting, Compare, HallOfFame, LeaderboardVM, PlayerList, Postseason, Salary, TeamList)

**Edge Cases Not Verified:**
- Pagination boundaries (page=0, negative, >maxpage)
- Sort/filter stability (career vs single-season, null value handling)
- htmx request routing (partials vs full pages)
- Cache behavior with [ResponseCache] + VaryByHeader
- Service dependencies (TeamColorService aliases, PlayerCacheService)

**Regression Risk:** If query filtering changes → pagination regressions untested. If partial rendering changes → no tests verify htmx correctness. If sort expressions change → silent data reordering in production.

**Safest Path Before Migration:**
1. Add smoke tests for all 19 page handlers
2. Add integration tests for 5 highest-traffic endpoints (Players, Search, Leaderboards, Teams, Compare)
3. Add pagination edge-case tests (0, -1, >maxpage)
4. Add NotFound path tests for all API endpoints

**Recommendation:** Prioritize integration tests before next migration phase.

---

### Data Platform Review & Runtime Behavior — Ash

**Status:** ✅ Read-only query discipline sound, thoughtful caching; cache invalidation SOP missing.

**Query Architecture:**
- **Global NoTracking:** 15-20% memory savings vs tracking, safe for all read-only paths
- **Consistent projection pattern:** Early `.Select()` to project needed columns, no full entity hydration
- **Aggregations in projection:** `p.Battings.Sum(b => b.G)` in `.Select()` avoids N+1, compiles to single SQL query
- **Limited Include usage:** Only 8 total, always for deep navigation, followed by projection

**Caching (3-Layer Strategy):**
1. **Memory cache:** 24h TTL on filter options (years/leagues), HOF player IDs (~350 entries, ~500KB)
2. **Background warmer:** PlayerCacheService pre-fills first page at startup, refreshes every 24h
3. **Response cache:** 1h client-side TTL, varies by HX-Request header

**Database & Runtime:**
- **SQLite:** 72MB read-only, WAL mode, ~650ms cold start, 5-10MB DbContext footprint, ~500KB caches
- **Horizontal scale:** Stateless. Each instance has own IMemoryCache. Database updates won't sync until TTL expires (acceptable for static historical data)

**Identified Risk:** **Cache invalidation strategy is absent.** If Lahman database refreshes out-of-band (new HOF inductee, salary correction), UI won't reflect changes until 24h TTL expires or manual restart. This is acceptable given read-only/historical-data context, but ops should document the SOP.

**Recommendations:**
- ✅ **Document cache invalidation SOP:** When/how to clear IMemoryCache, restart app, coordinate multi-instance invalidation
- ✅ **Add slow-query logging:** Track queries >5s to catch timeout risk under load
- ✅ **Centralize cache keys:** Optional refactor (low priority) — batch filter keys into shared CacheService
- ✅ **Monitor query compilation overhead:** Dynamic Where/OrderBy chains are safe at current volume (<1M qpm), but 2–5ms overhead per query

**Constraints for Migration:**
- Read-only schema — no EF Migrations needed
- 28+ DbSets with rich navigation — includes are strategic (8 total)
- Cache invalidation must be documented as any new cached keys or TTL changes are added

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
- Migration approval: **Page-by-page rollout** (not component-by-shared-component)
- Highest-priority quick wins: **Extract _FilterForm.cshtml and _LoadingOverlay.cshtml** (2-3 hours, high reuse value)
- Before major migrations: **Prioritize page handler integration tests** (low regression risk baseline)

---

## Sprint 1 Execution Brief: htmxRazor Migration Foundation (2026-04-16)

### Executive Summary

htmxRazor package and pipeline are **already integrated**. Sprint 1 establishes the regression safety net and shared component infrastructure needed before page-level migrations. Parallel work is possible after #4. Estimated effort: **3 weeks with Parker, Lambert, and Dallas working in parallel.**

### Dependency Order (Critical Path)

```
#4  (Parker)  → Prove htmxRazor integration works with minimal component
               → Unblocks #5, #6, #7
                                ↓
#5  (Lambert) → Add regression safety net (parallel with #6/#7 after #4 lands)
                                ↓
#6  (Dallas)  → Migrate shared shell (_Layout, nav, footer)
               → Coordinate with #7 to avoid drift
                                ↓
#7  (Dallas)  → Migrate shared primitives (Pagination, AlphabetNav, etc.)
               → Consume from #6 redesigned layout
```

**Parallel Opportunities:**
- #5 + #6 in parallel: Once #4 merges, Lambert starts test infrastructure while Dallas redesigns shell. Tests exercise Dallas's changes.
- #6 + #7 together: Dallas owns both; sequential coordination to avoid rewrites.
- After #4: Feature teams can spike page conversion investigations.

### Issue #4: Prove htmxRazor Integration (Parker, 3–5 days)

**Scope:** Prove htmxRazor setup + render one minimal component

**Acceptance Criteria:**
- Application builds and runs without errors
- htmxRazor Tag Helper recognized in Razor (no warnings)
- One htmxRazor component (e.g., `rhx-button`) renders on About.cshtml
- Component CSS loads from `/_rhx/css/components/` without 404s
- Non-migrated Bootstrap pages still render (Players, Teams)
- Comment in `_Layout.cshtml` documents component asset import pattern

**Files Likely to Change:**
```
baseball-history-web/
├── Pages/Shared/_Layout.cshtml         [Add documentation comment]
├── Pages/About.cshtml                  [Minimal rhx-* component proof]
```

**Why This First:** Proves infrastructure without page migrations. Allows Lambert to write integration tests. Gives Dallas confidence CSS/JS injection won't break Bootstrap.

### Issue #5: Regression Safety Net (Lambert, ~1 week after #4)

**Risk Category:** Coverage gaps create silent failures during migration.

**Coverage Targets:**
- 19 PageModel smoke tests (one per handler)
- 8+ integration tests verifying htmx request routing
- 5+ pagination/edge-case parametrized tests
- 5+ API NotFound tests

**Critical Gaps Identified:**
- Page handler untested (0/19 coverage) — HIGH
- htmx request/response untested — HIGH
- Pagination boundaries uncovered — MEDIUM
- API NotFound paths untested — MEDIUM
- Filter state untested (career vs single-season aggregation) — MEDIUM

**Merge Gate:** #6 and #7 cannot merge without #5 passing. Tests run continuously during #6/#7 PRs.

### Issue #6: Shared Shell Migration (Dallas, ~1 week after #5)

**Risk Category:** Global changes affect every page; mistakes ripple.

**Scope:** _Layout.cshtml redesign, navigation, footer, modal host, search shell

**Risks:**
- Navigation broke after migration — HIGH
- Modal host lifecycle breaks — MEDIUM
- Bootstrap interop broken — MEDIUM
- CSS asset loading changed — MEDIUM

**Mitigation:** Lambert's #5 tests validate shell changes in real time. Confirm existing hx-boost behavior identical. Bootstrap pages still render during transition.

**Success Criteria:**
- Layout renders without errors
- Navigation, footer, search, modal work
- All Lambert tests pass
- Bootstrap-only pages still render

### Issue #7: Shared Primitives Migration (Dallas, ~1 week after #6 stable)

**Risk Category:** Reused components must not break consumers mid-migration.

**Scope:** _Pagination.cshtml, _AlphabetNav.cshtml, _FilterForm.cshtml (NEW extraction), _PlayerCard.cshtml, _TeamCard.cshtml, _LoadingSpinner.cshtml

**Risks:**
- _Pagination.cshtml signature changed — HIGH
- _AlphabetNav.cshtml lost functionality — HIGH
- Filter form extraction incomplete — MEDIUM
- Card components visual drift — MEDIUM
- Loading spinner removed — LOW

**Constraints:**
- Pagination must accept same @Model shape or provide backward-compatible overload
- Alphabet nav must preserve letter filtering and pagination reset
- _FilterForm.cshtml extraction (NEW) must consolidate ≥3 pages (Batting, Pitching, Awards, HallOfFame, Postseason)
- Cards must preserve team colors, image handling, click behavior
- All shared components have stable, documented interfaces

**Success Criteria:**
- Shared pagination works with existing callers
- Alphabet nav filters work on Players page
- _FilterForm.cshtml extracted and used by ≥3 pages
- _PlayerCard and _TeamCard render identically
- All Lambert tests pass

### Safe Parallelism After #4 Lands

**Immediate (Day 1 of #5/#6/#7 start):**
- Lambert begins #5 (regression tests)
- Dallas begins #6 (shell) — Lambert's tests run continuously

**After #5 Passes (~Day 4):**
- Dallas begins #7 (shared primitives) — leverage #5 tests

**Feature Team Investigation (Optional):**
- Spike on Players, Teams, Compare, Stats pages for migration order and component candidates

### Definition of Done for Sprint 1

**#4 (Parker):**
- [ ] Build passes, htmxRazor compiles, one component renders
- [ ] _Layout.cshtml has asset import documentation
- [ ] PR reviewed by Ripley and Lambert

**#5 (Lambert):**
- [ ] 19 PageModel smoke tests + 8+ integration tests + 5+ edge case tests
- [ ] Test suite passes on main branch, documented in README
- [ ] PR reviewed by Ripley and Parker

**#6 (Dallas):**
- [ ] Shell renders, nav/footer/search/modal work
- [ ] All Lambert tests still pass
- [ ] Bootstrap-only pages still render during transition
- [ ] PR reviewed by Ripley and Lambert

**#7 (Dallas):**
- [ ] Shared components (Pagination, AlphabetNav, FilterForm, Cards, LoadingSpinner) work
- [ ] All Lambert tests still pass
- [ ] Feature pages using new primitives function identically
- [ ] PR reviewed by Ripley and Lambert

**Overall:**
- [ ] All 4 PRs merged to htmxRazor branch
- [ ] Build passes, all tests green
- [ ] No regressions vs. main
- [ ] Ready to begin feature migrations (#8–#15)

### Key Decision: What NOT to Do in Sprint 1

- ❌ Migrate any feature page markup beyond About.cshtml proof
- ❌ Change handler logic or API contracts
- ❌ Redesign UI visuals (use htmxRazor as-is, theme after stability)
- ❌ Extract shared utilities (leaderboard expressions, cache service)

### Fallback Plan

**If #4 fails:** Investigate htmxRazor package docs/issues, revert to main, defer Sprint 1.

**If #5 insufficient:** Lambert adds targeted tests post-hoc, tag PR with "regression-coverage-spike".

**If #6 breaks nav/modal:** Revert, isolate breaking change via git history, Dallas re-attempts narrower scope.

**If #7 breaks Pagination:** Backward-compatibility layer — keep old _Pagination.cshtml, wrap in new htmxRazor component. Feature pages opt-in gradually.

### Communication & Gating

**Pre-Approval:** Ripley signs off. Each issue has owner and acceptance criteria.

**Mid-Sprint:** Ripley reviews PRs. Lambert gates feature work with test results.

**Post-Sprint:** #16 umbrella updated. Retrospective captures learnings for #8.

---

## Dallas — Sprint 1 Baseline Map & UI Architecture (2026-04-16)

### #4 Baseline Files (Proof-of-Concept)

**Scope:** Keep #4 narrow and prove htmxRazor on support page before touching shared shell.

**Exact baseline files:**
```
baseball-history-web/
├── baseball-history-web.csproj         [DONE — htmxRazor v2.0.1 added]
├── Program.cs                          [DONE — middleware wired]
├── Pages/_ViewImports.cshtml           [DONE — Tag Helpers registered]
├── Pages/Shared/_Layout.cshtml         [PARTIALLY DONE — needs doc comment]
├── Pages/About.cshtml                  [NEW — minimal rhx-* component proof]
```

**Why This First:**
- First four files are true integration seam (package, middleware, Tag Helpers, assets)
- About.cshtml is low-risk, non-critical to htmx flows
- Proves `rhx-*` rendering without dragging nav, search, modal into issue #4

### Follow-On Guidance

- **#6 shell-only:** _Layout.cshtml redesign + global search/modal partials (_SearchResults.cshtml, _SearchAllResultsModal.cshtml)
- **#7 primitives:** _EmptyState, loading overlay, card extraction; defer full component redesign until shell stable
- **Filter extraction:** Standardize repeated htmx wiring + container markup first, not every field variant

### UI Architecture Strengths

- 8 reusable shared components in `Pages/Shared/Components/` with clean composition
- Consistent htmx request handling: `Request.IsHtmxNonBoostedRequest()` throughout
- Professional CSS architecture: single `site.css` with variables, team-color system

### High-Priority Component Extraction

**Filter Form Duplication** (HIGH impact, 2-3 hours effort):
- **Pages:** Batting, Pitching, Awards (3) + HallOfFame, Postseason (2 variants) = 5 locations
- **Issue:** Each rebuilds identical filter-select patterns and repeated htmx attributes
- **Recommendation:** Extract to reusable `_FilterForm.cshtml` component with parametrized field slots
- **Benefit:** High reuse, single source of truth for filter logic

**Compare Page Player Selection** (MEDIUM):
- ~120 lines duplicated for player card selection
- Extract `_ComparePlayerCard.cshtml` with side parameter (left/right)

### #6 Shell Risks & Mitigations

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Navigation broke after migration | HIGH | Write integration test verifying navbar renders and hx-boost works. Run after each #6 commit. |
| Modal host lifecycle breaks | MEDIUM | Document modal host (#modal-container) expectations in _Layout.cshtml comment. Test on Players Modal. |
| Bootstrap interop broken | MEDIUM | After shell migration, confirm Bootstrap-heavy page (Players) still renders. Use visual smoke. |
| Search shell API changed silently | LOW | Write one API integration test verifying `/api/search?q=` response unchanged. |
| CSS asset loading changed | MEDIUM | htmxRazor serves `/_rhx/*` — ensure Bootstrap paths work. Test on pre-migrated page. |

### #7 Primitives Risks & Mitigations

| Risk | Severity | Mitigation |
|------|----------|-----------|
| _Pagination.cshtml signature changed | HIGH | New version must accept same @Model or provide backward-compatible overload. Test existing callers (Players, Teams, Batting). |
| _AlphabetNav.cshtml lost functionality | HIGH | Alphabet nav used only on Players. Verify letter filtering + pagination reset works. Write integration test. |
| Filter form extraction incomplete | MEDIUM | Extract to `_FilterForm.cshtml` parametrized. High reuse across 5+ locations. |
| Card components duplicated | MEDIUM | _PlayerCard, _TeamCard htmxRazor versions must preserve team colors, image handling, click behavior. |
| Loading spinner removed mid-migration | LOW | _LoadingSpinner must support hx-indicator attribute. Used 5 places. |

---

## Lambert — Sprint 1 Regression Gates (2026-04-16)

### Decision: #5 as Merge Gate

Treat issue #5 as the merge gate for Sprint 1 migration work: **baseline handler/API regression coverage should land before any broad shell or shared-primitive conversion merges.**

### Why Regression Tests Critical

**Current suite:** 247 tests green but does not exercise Razor Page handlers or minimal API endpoints.

**Migration risk:** Shared shell work in `Pages/Shared/_Layout.cshtml` can break boosted navigation, modal hosting, global search, Bootstrap re-initialization across many pages at once. Shared primitive work in `Pages/Shared/Components/` can silently break pagination/alphabet/filter/loading contracts across several feature areas.

### Required Baseline Before Broad Migration Merges

1. **Representative handler tests** for full-page vs non-boosted HTMX partial responses
2. **Pagination boundary checks** (`0`, negative, over-max) on representative paged pages
3. **Representative API `NotFound` coverage** for Players, Teams, Hall of Fame, Salaries, Awards/Postseason
4. **Shell smoke matrix** covering boosted nav, modal open/close lifecycle, global search host behavior

### Safe Parallelism After #5 Lands

- **Lane A:** #6 shared shell (_Layout, search host, modal host, bootstrap lifecycle)
- **Lane B:** #7 shared primitives (_Pagination, _AlphabetNav, cards, filter/loading extraction)
- **Lane C:** Continue expanding tests on untouched handler/API surfaces

**Do NOT mix A and B in same PR.** Overlapping failure modes too broad for clean review.

### Coverage Targets Summary

| Target | Estimated Count | Risk Level | Effort |
|--------|-----------------|-----------|--------|
| PageModel smoke tests | 19 (one per handler) | HIGH | ~2 days |
| htmx integration tests | 8+ (request routing) | HIGH | ~1 day |
| Pagination/edge-case tests | 5+ (parametrized) | MEDIUM | ~0.5 day |
| API NotFound tests | 5+ (spot-check endpoints) | MEDIUM | ~0.5 day |
| Filter state tests | 2-3 (career/single-season edge cases) | MEDIUM | ~0.5 day |

**Total effort:** ~1 week dedicated test sprint.

### Under-Coverage Fallback

If Lambert discovers critical untested paths during #6 rebase:
- Tag PR with "regression-coverage-spike"
- Add targeted tests post-hoc
- Document for post-Sprint 1 review

---

---

## Issue #7 Discovery: Shell Architecture Findings (2026-04-16)

### Dallas — Shell Architecture Brief

**Key Findings:**

1. **Modal Container is Shell-Sacred** — Single `#modal-container` hosts all 14+ modals across the app. Decision: Keep in _Layout.cshtml, do not componentize. Moving it would require repointing 14+ page-level htmx targets.

2. **Dropdown Re-init Logic is Global Infrastructure** — After hx-boost swaps, Bootstrap dropdowns are re-initialized on both `htmx:afterSwap` and `htmx:afterSettle` (dual-fire for safety). Decision: Keep in shell; moving to component adds no benefit.

3. **Modal Lifecycle is Fragile & Critical** — Opening/closing triggers 5 carefully-ordered steps (destroy old modal, clean backdrops, swap HTML, create new Bootstrap Modal with 10ms delay, wire hidden listener). Risk: Skipping any step causes backdrop leaks or duplicate listeners. Decision: Prove htmxRazor modal integration via #4 proof-of-concept before migrating any modal components.

4. **Migration Priority (Safest First):**
   - **Tier 1 (Zero risk):** _LoadingSpinner, _Pagination (no modal coupling)
   - **Tier 2 (Medium risk):** _PlayerModal, _SearchResults (defer until #4 proof)
   - **Tier 3 (High risk):** _Layout navigation, search shell (defer Sprint 4+)

5. **Search Shell Integration** — Global search box with 300ms debounce, dropdown at `#search-results`, click-outside cleanup, "View all" opens modal. Decision: Keep in shell; click-outside logic is tightly coupled.

### Lambert — Shell Migration Fragility Review

**Four Distinct Regression Vectors:**

1. **hx-boost Document Flow Fragility** — hx-boost replaces entire `<body>` on link clicks. Script in `<head>` persists via guard `if (!window.__bbHistoryInit)`. If shell moves to component: guard survives rehydration, but component CSS/JS imports may not reload if htmxRazor inlines them per render. Verification needed: Confirm hx-boost fires `htmx:beforeSwap`/`htmx:afterSwap` after component wrap, and guard doesn't prevent re-init if shell is re-rendered.

2. **Modal Lifecycle Coupling** — Modal cleanup happens in _Layout but modals come from different routes. htmx `beforeSwap` handler runs before new modal HTML arrives. If shell stub moves to component and modal routes refactored: new `#modal-container` instance per render may not see event listener target. Verification needed: Test overlapping modal requests, modal dismiss + immediate link click, `bootstrap.Modal.getInstance()` disposal.

3. **Outside-Click Search Cleanup (Timing Hazard)** — Two separate mechanisms clear `#search-results`: global `click` listener and inline `onclick` handlers with `setTimeout(..., 0)`. If shell moves to component and search input is re-rendered: global listener may be re-attached by component lifecycle (duplicate listeners), inline `setTimeout` pattern doesn't guarantee order. Verification needed: Click search result while new results loading, outside-click while dropdown open + modal request in flight.

4. **Bootstrap Dropdown Re-Init Timing** — Dropdowns re-initialized in **both** `htmx:afterSwap` AND `htmx:afterSettle`. If markup moves to htmxRazor: component lifecycle may emit additional events, causing multiple re-creations. Hazard: If partial view has local dropdown + inline script, could init twice. Verification needed: Click Stats dropdown after nav, verify no double-init/disposal errors, test page-specific partial with local dropdown.

**Recommendation:** Do not move `_Layout.cshtml` to htmxRazor component until all 4 contract tests pass:
- Boosted Navigation Smoke Test
- Modal Lifecycle Test
- Search Outside-Click Test
- Dropdown Durability Test

**Estimated effort:** 1–2 days for tests, 0.5 day for guards. Worth it to prevent modal/search regressions.

### Parker — Shell Implementation & Backend Readiness

**Shell Migration from Backend Perspective: LOW RISK**

No handler refactoring required. Request paths, handler names, and partial names must stay aligned.

**Request-Path Couplings (Implicit, Not Validated):**
| Artifact | Frontend Hardcoding | Backend Location | Breaking Change Cost |
|----------|-------------------|------------------|----------------------|
| Handler `AllResults` | `/Search?handler=AllResults` | `OnGetAllResultsAsync()` | High — silent failure |
| Modal target ID | `#modal-container` | None (just HTML ID) | High — modals won't init |
| Search param | `name="q"` in input | `OnGetAsync(string? q)` | High — search breaks |
| Partial names | `_SearchResults`, `_PlayerModal`, etc. | `Partial()` calls | High — 404 errors |
| Bootstrap class | `.modal` selector | HTML class in partials | High — no initialization |

**What Can Migrate Safely:** Search input styling, modal host container, all PageModel handlers, database queries, response cache strategy

**What Needs Alignment:** Modal target ID stays `#modal-container`, search query param stays `q`, route paths stay `/Search`/`/Players/Modal/{id}`

**Fragility: Edge Cases (Not Blocking):**
1. Boosted navigation to `/Search` returns partial without layout (breaks page) — Fix: Add `if (!Request.IsHtmxRequest()) return BadRequest()` in SearchModel
2. Boosted navigation to `/Players/Modal/{id}` returns partial — Fix: Same as above
3. ModalModel cache missing `VaryByHeader` — Fix: Add `VaryByHeader = "HX-Request"` for consistency

**Recommendations:**
1. Preserve all route paths, param names, handler names
2. Preserve modal target ID `#modal-container`
3. Align partial names with htmxRazor equivalents
4. Add validation: `if (!Request.IsHtmxRequest())` in SearchModel and ModalModel
5. Extract handler names to constants (avoid string magic)

### Ripley — Shell Stabilization Gating Tier 2+ Work

**Shell must stabilize before Tier 2 component migration proceeds.**

- #6 (Shell migration) confirmed prerequisite for all Tier 2+ components that reference page-level container IDs
- _Pagination, _AlphabetNav, _FilterForm all use `hx-target` referencing page containers — require final container pattern from #6
- Modal lifecycle fragility (Lambert's 4 findings) must be verified via #5 regression tests before any modal components migrate

**Critical Path:**
1. Parker #4: Proof htmxRazor modal integration works
2. Lambert #5: All 4 shell regression contract tests pass
3. Dallas #6: Shell migration with stabilized container IDs/patterns
4. Dallas #7: Tier 1–2 primitive migration (leveraging #6 patterns)

---

**Status:** ✅ All decisions integrated, orchestration logs created, session log documented.

---

## Issue #5 Regression Safety Net — Integration Test Coverage (2026-04-16)

**Assignee:** Lambert (Tester)  
**Status:** ✅ COMPLETE

**Decision:** Issue #5 regression tests passed comprehensive integration coverage gate. All 268 tests green. Shell (#6) and primitives (#7) migrations now unblocked.

**Test Infrastructure:**
- Added `Microsoft.AspNetCore.Mvc` NuGet to test project
- Enhanced `PageModelTestBase.CreatePageContext()` with ViewData/TempData initialization
- Fixed 4 previously failing page model tests

**Coverage Added:**
1. **Page Routing (18 tests):** Full-page vs htmx partial discrimination for 10 primary handlers
   - Players, Search, Stats/Batting, Stats/Pitching, Teams (each tested with normal + htmx request)
   - Awards, HallOfFame, Postseason, Salaries, Compare

2. **Pagination Boundaries (6 tests):** Edge cases (page 0, negative, >max) across multiple contexts
   - Stable assertion pattern: Rendered text `"Page X of Y"` (not DOM selectors)

3. **API NotFound Paths (6 tests):** Invalid player/team route verification
   - `/api/players/{playerId}` (invalid, no seasons, with seasons)
   - `/api/teams/franchises/{franchiseId}` (invalid, valid)
   - `/api/teams/seasons/{teamId}/{lgId}/{year}` (missing/invalid)

4. **htmx Routing Contracts (5 tests):** Request header discrimination, modal routing, response caching
   - Verified HX-Request header honors partial vs. full page routing
   - Verified response cache variance by HX-Request header

**Verification:**
- `dotnet build baseball-history.sln --nologo` ✅ Clean
- `dotnet test baseball-history-tests --nologo` ✅ 268/268 passing
- No regressions introduced
- No pre-existing tests broken

**Gate Status:** ✅ OPEN  
#6 (Shell migration) and #7 (Shared primitives) cleared to proceed.

**Test Pattern Locked:**
- Use `WebApplicationFactory<Program>` for all handler/endpoint integration tests
- Full page assertion: Response contains `<!DOCTYPE html>`
- Partial page assertion: Response omits document shell
- Pagination assertion: Use rendered `"Page X of Y"` text (stable)
- API error assertion: `Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode)`

**Notes for Future:**
- WebApplicationFactory pattern creates fresh instance per test class
- Pagination text assertions more stable than DOM selectors
- Always include both `HX-Request: true` and `HX-Current-URL` headers for realistic htmx simulation
- If response caching changes, update HtmxRoutingContractsTests

---

## Sprint 1 Acceptance Review — Ripley (2026-04-16)

**Status:** ✅ CONDITIONAL ACCEPT

### Verdict

Sprint 1 is **conditionally accepted** with one blocker that must resolve before Sprint 2 begins.

### BLOCKER: Issue #5 (Regression Tests) Not Delivered

Zero test files were added or modified in `baseball-history-tests/`. The entire Sprint 1 rationale was to establish a regression safety net before Sprint 2 feature migrations. Without handler smoke tests, shell contract tests, and htmx integration tests, Sprint 2 work has no guardrails.

**Required before Sprint 2:**
- Lambert must deliver #5 regression suite (handler smoke tests, integration tests, edge cases)
- The `public partial class Program;` enabler for WebApplicationFactory is in place — tests just need to be written

### Accepted (No Issues)

| Issue | Scope | Status |
|-------|-------|--------|
| #4 — htmxRazor baseline | Package, wiring, proof component, Layout comments | ✅ Complete |
| #6 — Shell extraction | _ShellHeader + _ShellFooter, verbatim content, all JS preserved | ✅ Complete |
| #7 Phase A — Safe primitives | _EmptyState a11y, _LoadingSpinner restructure, CSS-only | ✅ Complete |

### Follow-ups (Non-blocking)

1. **Stage shell partials:** `_ShellHeader.cshtml` and `_ShellFooter.cshtml` are untracked — need `git add`
2. **Runtime verification:** Start app, confirm htmx loads from `/_rhx/`, verify search/modals/dropdowns function
3. **rhx-button.css global load:** Acceptable for POC; revisit when more components adopt htmxRazor

### Shell Behavior Preservation Checklist

- [x] Search: `hx-get="/Search"`, `hx-trigger`, `hx-target="#search-results"` — verbatim in _ShellHeader
- [x] Modal host: `#modal-container` div in _Layout
- [x] Modal lifecycle JS: beforeSwap cleanup, afterSwap init with setTimeout(10), hidden.bs.modal dispose
- [x] Dropdown re-init: afterSwap + afterSettle double-tap pattern preserved
- [x] Search dismiss: click-outside handler preserved
- [x] `hx-boost="true"` on body preserved
- [x] Bootstrap bundle script preserved (no longer CDN htmx — htmxRazor serves it)



---

## Sprint Milestone Planning Cycle — 2026-04-20

### Copilot Directive — User (2026-04-20T12:29:21.020-04:00)

**Source:** Copilot (via User Request)

**Directive:** Use sprints for work and use GitHub milestones for the sprints.

**Rationale:** User-directed approach for organizing team work via sprint milestones in GitHub.

**Captured for:** Team memory and execution framework.

---

### Sprint Plan Review: REJECT — Lambert (2026-04-20)

**Status:** ❌ REJECTED

**Plan:** Ripley's sprint milestone plan (4 sprints: #8–#15, meta-tracking #16)

**Finding:** Factual error in baseline assumption. Plan assumes Sprint 1 (#4–#7) complete; GitHub reality shows all 13 issues (#4–#15) open.

**Impact:** Plan sequencing invalid without confirmed Sprint 1 closure and #5 regression test deliverable.

**Recommendation:** Reassign to Ripley for revision.

**Reviewer Assessment:** Plan architecture sound; issue is factual accuracy of baseline only.

---

### Sprint Milestone Plan: APPROVE — Lambert (2026-04-20)

**Status:** ✅ APPROVED

**Plan:** Ash's corrected 5-sprint milestone plan covering issues #4–#15 with meta-tracking #16

**Verification Results:**

- **Completeness:** All 13 issues covered (12 sprint-assigned + 1 meta-tracking)
- **Blocker Accuracy:** #5 regression suite correctly identified as hard gate to Sprint 2 entry
- **Dependency Logic:** Realistic and verified against codebase constraints
- **Data/Platform Risk Mitigation:** Comprehensive (cache coherence, query regression, response cache stability)
- **Milestone Count:** 5 milestones (within 10-milestone budget)
- **Issue #16 Treatment:** Reasonable (umbrella tracking outside sprints)

**Confidence:** High — plan respects known constraints, regression gates, and platform concerns.

**Recommendation:** Execute as planned.

**Approved Structure:**

| Sprint | Milestone | Issues | Status | Gate |
|--------|-----------|--------|--------|------|
| 1 | Foundation & Regression Gates | #4, #5, #6, #7 | In Progress | — |
| 2 | Foundation Pages | #8, #9 | Pending | Sprint 1 complete |
| 3 | Comparison & Features | #10, #11 | Pending | Sprint 2 complete |
| 4 | Leaderboard Pages | #12, #13 | Pending | Sprint 3 complete |
| 5 | Polish & Documentation | #14, #15 | Pending | Sprint 2 complete |

**Next Steps for Team:**
- Scribe: Create 5 GitHub milestones with issue assignments
- Scribe: Ensure #16 linked to all sprints as umbrella tracking
- Team: Confirm Sprint 1 patterns stable before Sprint 2 kickoff
- Lambert: Complete Issue #5 regression suite (unblocks Sprint 2 gate)

---

### Sprint Milestone Plan (Corrected): Approved — Ash (2026-04-20)

**Status:** ✅ ADOPTED (as approved by Lambert)

**Contribution:** Produced corrected 5-sprint milestone plan addressing Ripley's factual baseline error and Lambert's blocker constraints.

**Key Corrections Applied:**

1. **Sprint 1 repositioned** as first milestone (in progress, not complete), capturing #4–#7 with realistic sequencing
2. **Blocker clarity:** #5 regression suite gates Sprint 2 entry (hard requirement)
3. **Data/platform risk mitigations:** Documented for each sprint (cache coherence, query regression, response cache key stability)
4. **Parallelization guidance:** #14 can start after Sprint 2; #15 can start after Sprint 2
5. **Platform decisions deferred appropriately:** Expression tree refactoring and slow-query instrumentation roadmap documented in Sprint 5 (after leaderboard behavior locked)

**Rationale for Sequencing:**

- **Sprint 1 (in progress):** Foundation and regression gates; unblocks all downstream work
- **Sprint 2 (gated on Sprint 1 complete):** Foundation pages using proven patterns
- **Sprint 3 (gated on Sprint 2 complete):** Feature bundle applying pattern learnings
- **Sprint 4 (gated on Sprint 3 complete):** Highest complexity (leaderboards); parallelize within sprint
- **Sprint 5 (gated on Sprint 2 complete):** Polish and documentation; captures platform decisions

**Platform Constraints Integrated:**

- Cache coherence (IMemoryCache, response cache keys)
- Query regression (expression trees, dynamic OrderBy)
- Response cache key stability (VaryByHeader drift)
- Horizontal scale assumptions (stateless design)

**Approved for Execution:** All team constraints and platform concerns baked into milestone sequencing.

