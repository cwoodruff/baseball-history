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
