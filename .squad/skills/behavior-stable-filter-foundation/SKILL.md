---
name: "behavior-stable-filter-foundation"
description: "Extract shared filter and loading primitives without changing HTMX or handler contracts."
domain: "htmx-migration"
confidence: "high"
source: "earned"
---

## Context
Use this when several Razor Pages repeat the same filter card and loading overlay structure, but the migration must not disturb existing HTMX request flow or PageModel expectations.

## Patterns
- Keep the slice view-only when possible: prefer shared partial markup and CSS helpers over handler or route changes.
- Preserve every existing `hx-get`, `hx-target`, `hx-include`, `hx-push-url`, route, and query parameter name.
- Reuse a shared loading partial inside the existing `#loading-indicator` wrapper so indicator IDs and HTMX hooks remain unchanged.
- Standardize filter card structure with additive CSS classes (`filter-shell`, `filter-card`, `filter-row`, `filter-actions`) instead of changing request wiring.
- Add `hx-indicator` only to existing filter triggers that already hit the same handler/target.
- Backstop with page-routing integration tests that assert full-page responses still render the filter host and HTMX requests still return only the list partial.

## Examples
- `baseball-history-web/Pages/Awards/Index.cshtml`
- `baseball-history-web/Pages/Postseason/Index.cshtml`
- `baseball-history-web/Pages/Salaries/Index.cshtml`
- `baseball-history-web/Pages/HallOfFame/Index.cshtml`
- `baseball-history-web/Pages/Shared/Components/_LoadingSpinner.cshtml`
- `baseball-history-web/wwwroot/css/site.css`

## Anti-Patterns
- Do not rename indicator IDs, list target IDs, routes, or query parameters as part of a cosmetic primitive pass.
- Do not move filter behavior into new handlers unless the existing request contracts are explicitly being redesigned.
- Do not fold pagination, modal host, alphabet navigation, or search host work into the same slice.
