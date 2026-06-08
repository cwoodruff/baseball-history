# Project Context

- **Owner:** dallas
- **Project:** baseball-history
- **Role Summary:** UI and component owner: shell extraction, safe primitives, and page migrations.

## Core Context

dallas has been contributing in their role: UI and component owner: shell extraction, safe primitives, and page migrations. Key facts condensed: regression safety & guardrails are authoritative for sprint gates; shell/primitives extraction progressed under guarded reviews; Lahman Postgres export artifacts produced and validated (2026-06-08).

## Recent Updates

- Same test structure (full-page, non-boosted, boosted variants)
- Same response cache preservation (`VaryByHeader="HX-Request"`)
- Same shell authority over `#modal-container`

### Key Learnings
- **Parameterized partials work well** — `_ComparePlayerCard` accepts `(Player?, Side, OtherPlayerId, Gradient)` tuple for flexible reuse
- **Dual-target pattern scales** — Two independent htmx targets in same view without collision
- **Query string preservation** — Search URLs dynamically construct `player1`/`player2` params to preserve other selection
- **Conditional rendering** — `@if (Model.BothSelected)` in partial keeps comparison tables hidden until both players selected
- **Empty state in partial** — Player card partial handles both empty (search) and loaded (player detail) states cleanly

### Files Created
- `baseball-history-web/Pages/Compare/_CompareMain.cshtml`
- `baseball-history-web/Pages/Compare/_CompareHeader.cshtml`
- `baseball-history-web/Pages/Compare/_ComparePlayerCard.cshtml`

### Files Modified
- `baseball-history-web/Pages/Compare/Index.cshtml` (wrapper only)
- `baseball-history-web/Pages/Compare/Index.cshtml.cs` (htmx detection added)
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs` (+5 tests)

### Quality Gates
- ✅ Build: Clean, no warnings
- ✅ Tests: 350 total, all passing
- ✅ Full-page behavior: Preserved
- ✅ htmx partial behavior: Working correctly
- ✅ Modal integration: Unchanged
- ✅ Search contracts: Stable
- ✅ Response cache: Preserved

### Status
✅ Sprint 3 Compare migration COMPLETE. Ready for team review and merge.


## Sprint 5 Support Surfaces (2026-04-21)

- Shared page-header chrome is a safe polish seam for top-level support/info pages because it changes layout consistency without touching route behavior.
- Homepage cleanup is safest as page-local partial decomposition: keep every homepage link and player modal trigger literal while splitting the surface into boring, reviewable chunks.
- Search dropdown and search-all modal stay aligned best when both render through one shared result-row component that preserves player modal vs. franchise navigation behavior.

## Sprint 5 Issue #14 Completion (2026-04-21)

**Status:** ✅ COMPLETED

Homepage, search surfaces, and support/info pages successfully migrated to htmx/Razor pattern. All shell-owned contracts preserved exactly.

### Files Migrated
- `Pages/Index.cshtml` — homepage with links and player modal triggers
- `Pages/Search.cshtml` — search shell endpoint, partial-only
- `Pages/_SearchResults.cshtml` — dropdown results partial
- `Pages/_SearchAllResultsModal.cshtml` — full results modal partial
- `Pages/About.cshtml`, `Pages/ApiDocs.cshtml`, `Pages/Error.cshtml`, `Pages/Privacy.cshtml`, `Pages/Health.cshtml` — support pages

### Quality Gates Met
- Tests: 337 → 344 (+7 new integration tests)
- Build: Passed
- Search behavior: Dropdown + modal routing contracts preserved
- Shell wiring: Global search input, `#search-results`, `#modal-container` unchanged
- Homepage cache: Preserved (no HX-Request split)
- All support routes functional and correct

### Preserved Contracts
- `/Search?q={query}` → dropdown partial
- `/Search?handler=AllResults&q={query}` → modal partial
- Player links → `#modal-container`
- Team links → `/Teams/Franchise/{id}`
- All homepage/support routes unchanged

### Sprint 5 Gate Achievement
#14 complete. All shell contracts preserved. Regression gate cleared by Lambert (344/344 tests).

## ApiDocs Markup Repair (2026-04-21)

**Status:** ✅ COMPLETE

Surgical markup repair on `ApiDocs.cshtml`: fixed unmatched closing tag. Page behavior and content preserved. Zero regression impact.

**File:** `baseball-history-web/Pages/ApiDocs.cshtml`

This was a post-sprint hygiene fix to ensure all page markup is well-formed.
