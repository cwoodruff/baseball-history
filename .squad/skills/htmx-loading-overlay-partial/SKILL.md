---
name: "htmx-loading-overlay-partial"
description: "Safely extract repeated HTMX filter loading overlays into a shared Razor partial without changing page contracts."
domain: "frontend"
confidence: "high"
source: "earned"
---

## Context
Use this when several Razor Pages repeat the same loading overlay structure around HTMX filter forms, but their request wiring is already stable and should not be refactored broadly.

## Patterns
- Keep each page's existing `hx-indicator` id and the surrounding `position-relative` filter container.
- Move the repeated spinner body markup into `Pages/Shared/Components/_LoadingSpinner.cshtml`.
- Keep the page-owned overlay wrapper (`id="loading-indicator"`, `class="htmx-indicator loading-overlay"`) in place for the first safe slice.
- If you want a shared filter-row foundation, add only extra classes that sit beside existing Bootstrap classes.
- Skip pages that do not already have loading-overlay behavior; adding a new overlay is a contract change, not a primitive extraction.

## Examples
- `Pages/Stats/Batting.cshtml`
- `Pages/Stats/Pitching.cshtml`
- `Pages/Awards/Index.cshtml`
- `Pages/Postseason/Index.cshtml`
- `Pages/Salaries/Index.cshtml`

## Anti-Patterns
- Do not repoint `hx-target`, `hx-indicator`, or `hx-push-url` while extracting the partial.
- Do not fold modal, search host, pagination, or alphabet-nav behavior into the same slice.
- Do not introduce a new loading overlay on pages like Hall of Fame unless the task explicitly includes that behavior change.
