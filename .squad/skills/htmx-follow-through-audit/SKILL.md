---
name: "htmx-follow-through-audit"
description: "Audit htmxRazor asset imports and cache behavior after a migration slice without breaking shell-owned runtime contracts."
domain: "migration-polish"
confidence: "high"
source: "earned"
---

## When to use

Use this after a Razor Pages + htmxRazor migration lands and the team needs to clean up runtime/docs safely.

## Checklist

1. Search for live `rhx-*` usage before touching `/_rhx/css/components/*` imports.
2. Keep component CSS centralized in `Pages/Shared/_Layout.cshtml` so styles survive `hx-boost` body swaps.
3. Remove a JS/CSS import only when you can prove the asset is empty or has zero remaining references.
4. Do not move shell-owned modal, dropdown, or search lifecycle logic out of `_Layout.cshtml` during cleanup.
5. Document the cache stack together:
   - `IMemoryCache` lookups (24h in this repo)
   - warmed background cache (`PlayerCacheService`)
   - `[ResponseCache(..., VaryByHeader = "HX-Request")]` for full-page vs partial separation
6. If shared-helper extraction would unify code paths with different aliases or stat coverage, defer it and record the parity risk explicitly.

## Proven pattern in this repo

- Safe removal: the `_Layout.cshtml` import for `~/js/site.js` because the file was empty and all shell runtime behavior already lived inline in layout.
- Required retained assets:
  - `/_rhx/css/components/rhx-button.css` for `Pages/About.cshtml`
  - `/_rhx/css/components/rhx-badge.css` for team and leaderboard migrations

## Validation

- `dotnet build baseball-history.sln`
- `dotnet test baseball-history-tests`
- Spot-check one full-page request and one HTMX partial path on a migrated page if runtime verification is needed.
