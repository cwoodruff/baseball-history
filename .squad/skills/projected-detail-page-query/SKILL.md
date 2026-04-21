---
name: "projected-detail-page-query"
description: "Build Razor Page detail screens from projected records and dedicated view models instead of passing entities or PageModels into partials."
domain: "backend"
confidence: "high"
source: "earned"
---

## Context
Use this when a read-only Razor Page has a rich detail view with HTMX partial rendering and the component layer should not depend on EF entities or PageModel state.

## Patterns
- Filter to the requested row first, then project only the needed columns into a flat record.
- Convert string-heavy database fields into display types in a view-model factory (`FromRecord`) after materialization.
- Project roster/history collections directly into rendering models instead of using `Include()` and then traversing entities in views.
- Pass dedicated view models to partials; do not use the PageModel itself as the partial contract.
- Keep the existing `Request.IsHtmxNonBoostedRequest()` split and `ResponseCache` behavior untouched while changing the projection seam.

## Examples
- `baseball-history-web/Pages/Teams/Season.cshtml.cs`
- `baseball-history-web/ViewModels/TeamSeasonViewModel.cs`
- `baseball-history-web/Pages/Teams/Franchise.cshtml.cs`
- `baseball-history-web/ViewModels/FranchiseDetailViewModel.cs`

## Anti-Patterns
- Loading an EF entity with `Include()` just to hand it to a partial.
- Letting a component or partial reach back into a PageModel for data it should receive explicitly.
- Mixing parse/format logic into Razor markup when the page can normalize it once in the view model.
