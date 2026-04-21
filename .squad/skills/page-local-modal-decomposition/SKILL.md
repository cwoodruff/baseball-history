---
name: "page-local-modal-decomposition"
description: "Split a large Razor modal into local partials without changing shell-owned modal routing or htmx contracts."
domain: "frontend"
confidence: "high"
source: "earned"
---

## Context
Use this when a feature has a large modal partial that is hard to maintain, but the shell already owns the modal host, lifecycle JavaScript, and htmx target.

## Patterns
- Keep the route and htmx contract unchanged (`/Feature/Modal/{id}`, `hx-target="#modal-container"`).
- Leave Bootstrap modal initialization and cleanup in the shared layout when that shell already coordinates all modals.
- Split the modal into folder-local partials for stable sections like overview/sidebar, summary cards, and season/result tables.
- Prefer local partials over a new shared component when the markup is feature-specific and reuse is not yet proven.
- Treat the migration as structural: rendered HTML and dismissal controls should stay effectively the same.

## Examples
- `baseball-history-web/Pages/Players/_PlayerModal.cshtml`
- `baseball-history-web/Pages/Players/_PlayerModalOverview.cshtml`
- `baseball-history-web/Pages/Players/_PlayerCareerSummary.cshtml`
- `baseball-history-web/Pages/Players/_PlayerBattingSeasonsTable.cshtml`
- `baseball-history-web/Pages/Players/_PlayerPitchingSeasonsTable.cshtml`

## Anti-Patterns
- Do not move `#modal-container` ownership out of `Pages/Shared/_Layout.cshtml` as part of a feature migration.
- Do not repoint player cards or change modal routes while only decomposing markup.
- Do not promote highly feature-specific modal sections into shared components before real reuse exists.
