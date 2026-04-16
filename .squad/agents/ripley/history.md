# Project Context

- **Owner:** Woody
- **Project:** Baseball History migration to htmxRazor
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Created:** 2026-04-16T10:57:49Z

## Learnings

- **Architecture:** Codebase is well-structured with proven htmx patterns, clean separation of concerns (Models/ViewModels/Pages), and comprehensive test coverage (247 tests).
- **Data Access:** EF Core is correctly configured with NoTracking globally and projection patterns preventing N+1 queries. Composite keys on Batting/Pitching properly handled.
- **Caching Strategy:** Thoughtful, not excessive. Pre-warmed player cache for hot path (first page), 24-hour TTL on memory cache entries, strategic use of `[ResponseCache]` with `VaryByHeader = "HX-Request"`.
- **htmx Integration:** Production-proven with consistent `IsHtmxNonBoostedRequest()` detection, proper partial response handling, and working modal patterns.
- **Page Models:** Largest is 246 lines (Players/Modal), manageable. Clear dependency injection via primary constructors. No performance anti-patterns.
- **Testing:** xUnit tests are production-grade with ViewModel transformations, DB integration, and HTMX extension verification. Tests enable confident refactoring.
- **htmxRazor Readiness:** No blockers. Migration path is clear: extract shared components first (Pagination, AlphabetNav), then migrate pages by priority (Players → Teams → Stats).
- **Component Strategy:** Page-by-page rollout (not component-by-shared-component) reduces review complexity and allows per-page rollback.
- **Key Risks:** Leaderboard expression trees duplicated (API vs Pages), but isolated. Page model sizes will grow — spike needed on query service extraction post-migration.
- **Team Alignment:** Well-chartered roles (Dallas for UI, Parker for PageModels, Ash for data, Lambert for tests). Ready to begin.

## Codebase Review Output (2026-04-16)

**Consolidated all team findings into decisions.md**

- Dallas identified filter form duplication (HIGH priority extraction)
- Parker confirmed clean backend seams, htmx-aware caching intact
- Lambert flagged page model/API test gaps (regression risk)
- Ash documented cache invalidation SOP as missing action item
- Team alignment: page-by-page migration path approved, ready to begin

## Sprint 1 Planning (2026-04-16)

### Key Findings
- **htmxRazor integration already done:** Package in csproj, wired in Program.cs, Tag Helpers registered in _ViewImports.cshtml, asset strategy documented in _Layout.cshtml (lines 15–16, 28–29)
- **Working branch state:** 19 page models, 44 Razor views, 6 shared components, 18 test files
- **Filter form duplication confirmed:** Batting, Pitching, Awards, HallOfFame, Postseason all have identical filter-select markup — high-priority extraction target for #7
- **Test baseline:** 247 tests passing, but 0/19 page handlers tested (Lambert's #5 will fix this)
- **Parallel opportunities:** #5 + #6 can run in parallel after #4 lands; Dallas owns both #6 and #7 in sequence

### Sprint 1 Structure
1. **#4 (Parker, 3–5 days):** Prove htmxRazor works with minimal component on About.cshtml. Already 90% done (just needs one rhx-* component rendered and _Layout comment).
2. **#5 (Lambert, parallel with #6 after #4 lands):** Add 19 PageModel smoke tests, 8+ integration tests for htmx routing, 5+ edge-case tests. Becomes project's regression gate.
3. **#6 (Dallas, sequential within her work):** Migrate _Layout, nav, footer, search shell, modal host. Coordinate with #5 to catch breaks immediately.
4. **#7 (Dallas, after #6):** Migrate Pagination, AlphabetNav, FilterForm (new extraction), card components, loading spinner. Reuse from #6.

### Critical Risk Mitigations
- **#5 regression suite gates #6/#7:** No shell/primitive changes land without test verification
- **Filter form extraction in #7:** Reduces duplication in Batting/Pitching/Awards/HallOfFame/Postseason
- **Bootstrap interop preserved:** Non-migrated pages stay functional during transition (pre-requisite for feature team work)
- **Fallback for #4 failure:** Revert to main, narrow scope, defer Sprint 1

### Files Likely to Change by Issue
- **#4:** Program.cs (done), _ViewImports.cshtml (done), _Layout.cshtml (needs doc), About.cshtml (new component), csproj (done)
- **#5:** New test file(s) in baseball-history-tests/ for PageModel and integration coverage
- **#6:** Pages/Shared/_Layout.cshtml, Pages/Shared/_Navigation (or lines in _Layout), Pages/Shared/_Footer, Pages/Shared/_ModalHost
- **#7:** Pages/Shared/Components/ (all 6 files reworked), new _FilterForm.cshtml extraction, possible new _LoadingOverlay.cshtml

### Sequencing Rationale
- #4 first: Proves foundation before #5/#6/#7 touch code
- #5 parallel with #6: Tests provide immediate feedback as shell changes
- #6 before #7: Shell defines layout context, primitives build on it
- After Sprint 1: Feature migrations (#8–#15) unblocked, feature teams can reference completed shared primitives and regression suite

## 2026-04-16 Sprint 1 Brief Delivered

Ripley orchestrated Sprint 1 execution brief with exact dependency order, parallelization guidance, and risk mitigations. Brief now integrated into decisions.md.

### Output
- Dependency order: #4 → #5/#6 parallel → #7
- Parallelization: #5+#6 after #4, then #7 after #5
- Coverage targets: 19 handlers + 8+ integration + 5+ edge cases
- Risk assessment: LOW (infrastructure wired), MEDIUM (test gaps, signature drift)

### Status
✅ Integrated. Parker ready for #4 immediately.

## Issue #6 Shell Investigation (2026-04-16)

### Shell Analysis Complete

Investigated _Layout.cshtml shared shell for #6 migration. Key findings:

**Shell Responsibilities:**
- Document shell (html/head/body with hx-boost)
- Asset loading (Bootstrap + htmxRazor CSS/JS)
- Navigation with Bootstrap dropdown + hx-boost links
- Global search (input → #search-results via htmx)
- Modal host (`#modal-container` receives all modals)
- 85-line inline JS block for Bootstrap re-initialization

**Fragile Areas Identified:**
- Double-tap dropdown re-init (afterSwap + afterSettle) indicates timing sensitivity
- `setTimeout(10)` in modal init is a race condition workaround
- Search clear uses `setTimeout(0)` hack
- Modal backdrop cleanup is complex, multiple cleanup paths

**Migration Order Recommendation:**
1. SAFE: Footer, static nav links, LoadingSpinner
2. CAREFUL: Pagination, Stats dropdown, search wiring
3. DEFER: Modal lifecycle JS (85 lines), Bootstrap interop

**htmxRazor Component Fit:**
- HIGH: _Pagination, _AlphabetNav, _LoadingSpinner, _EmptyState, _FilterForm (new)
- MEDIUM: _PlayerCard, _TeamCard (need slot support for content)
- LOW: Modal host (complex JS lifecycle, defer assessment)

### Output
Execution-ready brief delivered to `.squad/decisions/inbox/ripley-shell-brief.md` with:
- Exact shell responsibilities
- Safe → risky migration order
- Fragile behavior documentation
- htmxRazor fit assessment
- Explicit deferrals
- Test coverage requirements for #5

### Status
✅ Brief delivered. Ready for Dallas to execute #6 scope.

---

## 2026-04-16 Issue #7 Discovery: Prioritized Migration Inventory

### Discovery Completed
- Synthesized Dallas (UI) + Parker (backend) discoveries into prioritized, effort-estimated roadmap
- Created 3-tier classification: SAFE-NOW (5 components, 7-10h), WAIT-FOR-SHELL (2 components, 6-9h), DEFER (3 groups + complex pages, 50+h)
- Documented shell stabilization as blocker for Tier 2+ work
- Created decision matrix: Tier 1 requires zero new assets, Tier 2+ depends on #6 container patterns

### Prioritization Delivered
**Phase A (Safe-Now, 7-10h):**
1. _EmptyState (9 consumers, proof primitive)
2. _LoadingSpinner (dormant, reference pattern)
3. _Pagination (7 consumers, highest leverage)
4. _PlayerCard (1 consumer, modal-coupled)
5. _TeamCard (2 consumers, navigation-only)

**Phase B (Wait-for-Shell, 6-9h + integration):**
6. _AlphabetNav (1 consumer, Players page fixture required)
7. _PlayerModal (complex aggregation, deferred until modal pattern proven)

**Phase C (Duplication, 8-10h):**
8. _FilterForm extraction (5 pages)
9. _LoadingOverlay extraction (5 pages)
10. Compare Cards extraction (1h)

**Phase D (Complex, 50+ hours, deferred):**
- Stats/Batting, Stats/Pitching, Awards, Compare, Salaries page migrations

### Critical Path Verified
1. Parker #4: Proof htmxRazor modal integration works
2. Lambert #5: All 4 shell regression contract tests pass
3. Dallas #6: Shell migration with stabilized container IDs
4. Dallas #7: Tier 1–2 primitive migration

### Status
✅ Consolidated roadmap delivered. All findings synthesized into execution-ready priorities. Ready for Sprint 2 planning and approval.

### Next Steps
1. Woody: Review Tier 1 scope and approve Phase A for Sprint 2
2. Parker: Deliver #4 proof-of-concept (modal component)
3. Lambert: Execute #5 regression suite (Phase 1–5)
4. Dallas: Prepare Tier 1 component migrations
5. Ripley: Track blocker status, adjust Sprint 2 scope as #4/#5 progress

---

## 2026-04-16 Issue #7 Review: Safe Primitives Scope Gate

### Review Completed

Reviewed issue #7 scope: _EmptyState, _LoadingSpinner hardening + shared filter/overlay extraction. Scoped into 3 tiers with defer conditions.

### Key Findings

**Phase A — Safe Now (Immediate):**
- _EmptyState: 9 consumers with stable factory methods (NoPlayers, NoTeams, NoStats). Model contract inviolate.
- _LoadingSpinner: Dormant (0 consumers), ultra-low risk. String-only model must stay immutable.
- Htmx usage: 24 instances across codebase, all consistent with Request.IsHtmxNonBoostedRequest() pattern
- No pagination/alphabet nav/modal changes. Scope locked to two components + docs.

**Phase B — Defer to After #6 (FilterForm Extraction):**
- Duplication found: Batting, Pitching, Awards, HallOfFame, Postseason all have identical filter-select patterns (~50 lines each)
- Blocker: FilterForm lives inside shell container with htmx indicators. Must freeze shell IDs from #6 before extraction.
- Guard: Extract _FilterForm only after #6 shell review complete.

**Phase C — Defer Until Pattern Emerges (LoadingOverlay):**
- 5 pages have custom overlay markup; too divergent to extract yet
- Decision: Revisit after FilterForm extraction when pattern stabilizes
- Condition: Only extract if 3+ pages have identical markup

### Rejection Gates

❌ WILL REJECT if:
- EmptyStateModel signature changes without atomic 9-page consumer updates
- LoadingSpinner model changes
- Pagination/AlphabetNav/Modal changes slip into scope
- FilterForm lands before #6 shell review

### Approval Status

✅ **APPROVED** (Phase A only)
- Dallas may proceed with Phase A scope immediately
- Must confirm scope locked before starting implementation
- Lambert to add EmptyState factory tests to #5 regression suite
- Ripley gates Phase B/C with shell stabilization verification

### Output

Scope gate decision written to `.squad/decisions/inbox/ripley-safe-primitives.md` with exact approval conditions, rejection gates, and phase sequencing.

### Learnings

- **Component contract stability is the lever.** EmptyState is safe because factory methods don't change; FilterForm is risky because shell container IDs aren't yet stable.
- **Three-tier extraction strategy avoids double-refactor.** Phase A defines baseline, #6 stabilizes container, Phase B extracts forms confidently, Phase C emerges from pattern.
- **Duplication patterns are visible early.** All 5 leaderboard pages have identical filter markup; this is the high-signal extraction target, not generic loading overlay.
- **Regression gap remains.** EmptyState factory methods must get test coverage added in #5; otherwise pagination changes won't be verified under Phase C.


