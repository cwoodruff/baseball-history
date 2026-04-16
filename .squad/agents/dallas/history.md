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

