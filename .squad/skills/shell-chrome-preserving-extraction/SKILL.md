---
name: "shell-chrome-preserving-extraction"
description: "Safely extract shared Razor shell chrome without disturbing htmx, search, or Bootstrap modal contracts."
domain: "frontend"
confidence: "high"
source: "earned"
---

## Context
Use this when a Razor Pages layout needs a first migration slice that improves structure but cannot risk changing global htmx or Bootstrap behavior.

## Patterns
1. Extract only static or shell-chrome markup first: navbar/footer wrappers are good seams.
2. Preserve load-bearing shell contracts in place unless the task explicitly covers them:
   - `<body hx-boost="true">`
   - `#modal-container`
   - inline/global modal lifecycle listeners
   - search contracts like `/Search`, `name="q"`, and `#search-results`
3. When extracting header chrome, move the search markup intact with the navbar so IDs, targets, and triggers stay byte-for-byte stable where possible.
4. Prefer `@await Html.PartialAsync(...)` seams in `_Layout.cshtml` for a narrow, reviewable diff.

## Examples
- `Pages/Shared/_Layout.cshtml` delegates header/footer rendering to `_ShellHeader.cshtml` and `_ShellFooter.cshtml` while retaining modal host and JS behavior.
- `Pages/Shared/_ShellHeader.cshtml` keeps the existing search input attributes and dropdown host unchanged.

## Anti-Patterns
- Moving modal lifecycle JS out of the layout during the first slice.
- Renaming `#search-results` or changing search input `name="q"`.
- Changing modal target IDs or introducing new shell wrapper semantics in the same PR.
