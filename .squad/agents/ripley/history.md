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

