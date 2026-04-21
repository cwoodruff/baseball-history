---
name: "shell-owned-search-surface-contracts"
description: "Preserve the global search dropdown + all-results modal contracts while migrating Razor/htmx page markup around a shell-owned search surface."
domain: "frontend"
confidence: "high"
source: "earned"
---

## Context
Use this when migrating or reviewing the Baseball History global search UI. The visible search surface lives in the shared shell, but the rendered results come from page-level partials and handlers. That makes the boundary easy to break with seemingly small naming or routing changes.

## Patterns
1. Treat the shell as the authority for search chrome:
   - `_ShellHeader.cshtml` owns the search input, `name="q"`, `hx-get="/Search"`, and `#search-results`
   - `_Layout.cshtml` owns click-outside cleanup, modal lifecycle JS, and `#modal-container`
2. Keep the Search endpoint behaviorally stable during markup migration:
   - `/Search?q=...` returns `_SearchResults`
   - `/Search?handler=AllResults&q=...` returns `_SearchAllResultsModal`
3. Preserve result-type behavior:
   - player results open `/Players/Modal/{id}` into `#modal-container`
   - franchise/team results navigate normally to `/Teams/Franchise/{id}`
4. Treat `Search.cshtml` as a shell-adjacent endpoint, not a place to invent a new full-page search experience unless the task explicitly includes that redesign and tests
5. Use routing/integration tests as the contract source of truth when reviewing search changes

## Examples
- `baseball-history-web/Pages/Shared/_ShellHeader.cshtml`
- `baseball-history-web/Pages/Shared/_Layout.cshtml`
- `baseball-history-web/Pages/Search.cshtml.cs`
- `baseball-history-web/Pages/_SearchResults.cshtml`
- `baseball-history-web/Pages/_SearchAllResultsModal.cshtml`
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs`

## Anti-Patterns
- Renaming `q`, `#search-results`, `_SearchResults`, or `_SearchAllResultsModal`
- Moving modal ownership away from `_Layout.cshtml`
- Combining UI migration with search ranking/query refactors in one step
- Turning `Search.cshtml` into a new full-page UX without updated tests and an explicit design decision

## Implementation Note
- When dropdown and modal search views need cleanup, prefer a shared result-row partial/component so both surfaces keep the same player-modal and franchise-navigation wiring. In this repo that seam is `Pages/Shared/Components/_SearchResultLink.cshtml` with `SearchResultLinkModel`.
