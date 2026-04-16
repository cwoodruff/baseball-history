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

## Codebase Review Output (2026-04-16)

**Component extraction opportunities identified**

- Filter form duplication across 3-5 pages (2-3 hour extraction)
- Loading overlay standardization candidate
- Ripley approved page-by-page rollout strategy
- Parker's caching strategy will support component reuse
- Team aligned on quick wins (FilterForm + LoadingOverlay)
