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

## 2026-06-08 Issue #18 Assignment: Salaries Page Currency Formatting

**Status:** 🎯 READY TO PICKUP

Received assignment of GitHub issue #18 (Salaries page missing dollar sign on salary amounts) from Ripley's triage. Issue classified as LOW-risk Razor UI/display-layer fix.

**Scope:** Add currency formatting across all salary displays:
- Salaries page player salary displays
- Player salary history views
- Team payroll displays
- Highest-paid leaders leaderboards

**Suggested Implementation:** `@salary.ToString("C")` in Salaries.cshtml and related partials.

**Coordination:** Lambert available for regression verification post-fix.

**Decision Reference:** `.squad/decisions/decisions.md` — Ripley: Issue #18 Triage & Routing

## Learnings

- Salary displays on the Salaries surface should use an explicit dollar-sign formatter (`$` + `N0`) instead of culture-sensitive `"C0"` so full-page and htmx partial responses render stable USD strings across environments.

## Leaderboard Minimum-Selector Investigation (2026-08-08)

**Context:** User feedback reports that rate-stat leaderboards (AVG, OBP, SLG, ERA, WHIP) default to "No minimum," surfacing small-sample players (e.g., 1.000 BA with 1 AB) instead of qualified leaders like Cobb/Hornsby/Gibson. The future plan is to switch from a career AB floor to season-relative qualification (3.1 PA/team-game), which changes qualification semantics, especially for Negro Leagues players.

### Current UI Structure

**Main Leaderboard Pages:**
- `/Pages/Stats/Batting.cshtml` and `/Pages/Stats/Pitching.cshtml` — full-page wrappers with filter form (`#filter-form`)
- `/Pages/Stats/_BattingLeaders.cshtml` and `/Pages/Stats/_PitchingLeaders.cshtml` — partials showing table + pagination

**Minimum Selector Control:**
- **Location:** Batting/Pitching main pages, inside `#filter-form`, under `<div class="col-md-2">`
- **Control type:** `<select>` dropdown with `name="minAb"` (batting) or `name="minIp"` (pitching)
- **Options (Batting):**
  - `value="0"` → "No minimum" **(currently the default)**
  - `value="100"` → "100 AB"
  - `value="500"` → "500 AB"
  - `value="1000"` → "1000 AB"
  - `value="3000"` → "3000 AB"
- **Options (Pitching):**
  - `value="0"` → "No minimum" **(currently the default)**
  - `value="50"` → "50 IP"
  - `value="100"` → "100 IP"
  - `value="500"` → "500 IP"
  - `value="1000"` → "1000 IP"

**Server-side mapping:** 
- `int minAb = 0` / `int minIp = 0` are method parameter defaults in both `.cshtml.cs` PageModels (Batting.cshtml.cs, Pitching.cshtml.cs)
- "No minimum" = `0` → no filter applied, so all rows with any AB/IP are included
- No server-side logic currently distinguishes rate stats from counting stats — all stats use the same minimum threshold

### Rate Stats vs. Counting Stats

**Rate stats (require qualification to be meaningful):**
- Batting: `avg`, `obp`, `slg`, `ops`
- Pitching: `era`, `whip`, `k9`, `bb9`, `wpct`

**Counting stats (no qualification needed):**
- Batting: `hr`, `h`, `r`, `rbi`, `sb`, `2b`, `3b`, `tb`, `bb`, `g`, `ab`
- Pitching: `w`, `so`, `sv`, `cg`, `sho`, `ip`, `l`, `g`, `gs`

Currently **all stats share the same minimum selector** and all default to `minAb=0` / `minIp=0` regardless of whether the selected stat is a rate or counting stat.

### How the UI Detects Stat Type

**No client-side detection** — the stat dropdown triggers an htmx request to the server with `stat=<key>`, and the server-side code handles ordering/display logic:
- Pitching page already detects ascending vs. descending via `var isAscending = stat.ToLower() is "era" or "whip";`
- Batting page does not currently flag rate stats explicitly

The same partial (`_BattingLeaders.cshtml` or `_PitchingLeaders.cshtml`) renders all stats, so distinguishing rate vs. counting would need to be either:
1. **Page-level context** passed into the ViewModel (preferred)
2. **Client-side conditional rendering** in the partial based on `Model.StatColumn`

### UX Implications of Changing the Default

**If we default rate-stat leaderboards to "Qualified" (season-relative 3.1 PA/team-game):**

**Pros:**
- Solves the immediate bug: users see Cobb/Hornsby/Gibson on page one, not 1.000 BA players with 1 AB
- Aligns with MLB standard qualification
- Season-relative qualification is fairer for Negro Leagues players (who often have fewer career ABs due to incomplete records but still achieved qualified single-season performances)

**Cons:**
- "No minimum" option still needs to exist for:
  - Counting-stat leaderboards (HR, Wins, etc.) — which should remain "No minimum" by default
  - Users who want to see all players including small samples (e.g., curiosity, outlier detection, completionist searches)
- Changing the default breaks existing URLs/bookmarks that relied on `minAb=0` surfacing rate stats without qualification
- Season-relative "Qualified" is conceptually different from a fixed AB floor — the UI needs to communicate this clearly

**Proposed UX direction:**
1. **Stat-aware defaults:** When user selects a rate stat (AVG, OBP, ERA, WHIP), the minimum selector should default to "Qualified" (or a sensible season-relative threshold). When user selects a counting stat (HR, Wins), it stays "No minimum."
2. **Keep "No minimum" as an option** so users can still view all players.
3. **Visual indicator for qualification type:** For players who qualify via season-relative standard vs. career total, consider a badge/tooltip that explains *how* they qualified — especially important for Negro Leagues players.

### Visual Indication for Negro Leagues Qualification

**Current state:** The UI already has a HOF badge (`<rhx-badge rhx-variant="warning" rhx-size="sm" class="ms-1">HOF</rhx-badge>`) for Hall of Fame players.

**Proposed addition:** A similar badge/tooltip for qualified players, e.g.:
- "✓ Qualified" badge (neutral variant)
- Tooltip: "Qualified via 3.1 PA per team game in 1924 season"
- Or a footnote below the table explaining the qualification standard in use

This is especially important for Negro Leagues players, where a season-relative standard is more appropriate than a career AB floor (since many Negro League records are incomplete, but single-season data can be complete and authoritative).

### Shared Components

**No dedicated leaderboard components** — the main pages and partials are self-contained. The only reusable components referenced are:
- `Components/_EmptyState.cshtml` (for "no results" state)
- `Components/_Pagination.cshtml` (for page navigation)
- `Components/_LoadingSpinner.cshtml` (htmx loading indicator)

All leaderboard-specific markup is in `_BattingLeaders.cshtml` and `_PitchingLeaders.cshtml`.

### Caching Considerations

**Current caching setup:**
- Page-level: `[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]`
- This varies by `HX-Request` header, so full-page and htmx partial responses are cached separately
- Filter options (years, leagues) are cached in `IMemoryCache` for 24 hours

**Impact of default change:**
- If we change the default minimum from `0` to a stat-aware value (e.g., "Qualified" for rate stats), the **URL signature changes** (e.g., `?stat=avg&minAb=502` instead of `?stat=avg&minAb=0`)
- Existing `VaryByHeader="HX-Request"` is sufficient — no new cache keys needed
- If server-side default logic is added (e.g., "when stat=avg, default minAb to 502 for 3.1 PA/game"), URLs without an explicit `minAb` parameter would resolve to the new default, which is fine for new requests but might surprise users with bookmarks to the old `?stat=avg` (which implicitly meant `minAb=0`)

**Recommendation:** Flag this for Ash/Parker — if default minimum changes, consider:
1. Adding cache invalidation for leaderboard pages on deploy
2. Documenting the URL parameter change in release notes
3. Possibly supporting both old and new URLs via a redirect or compatibility shim

### Files Involved

- `baseball-history-web/Pages/Stats/Batting.cshtml` — batting filter form + minimum selector
- `baseball-history-web/Pages/Stats/Batting.cshtml.cs` — batting PageModel with `int minAb = 0` default
- `baseball-history-web/Pages/Stats/Pitching.cshtml` — pitching filter form + minimum selector
- `baseball-history-web/Pages/Stats/Pitching.cshtml.cs` — pitching PageModel with `int minIp = 0` default
- `baseball-history-web/Pages/Stats/_BattingLeaders.cshtml` — batting table partial
- `baseball-history-web/Pages/Stats/_PitchingLeaders.cshtml` — pitching table partial
- `baseball-history-web/ViewModels/LeaderboardViewModel.cs` — view model with `MinimumAtBats` / `MinimumInningsPitched` properties and stat dictionaries

### Next Steps (for implementation)

1. **Server-side:** Add logic to detect rate stats and set stat-aware default minimums (in `Batting.cshtml.cs` and `Pitching.cshtml.cs`)
2. **UI:** Update minimum selector to reflect the new default (selected option should match the stat-aware default)
3. **UI:** Add a "Qualified" option to the minimum selector (alongside existing fixed thresholds) and map it to the season-relative calculation
4. **UI:** Add visual indicator (badge/tooltip) for qualified players, especially for Negro Leagues players
5. **Testing:** Ensure htmx partial and full-page responses both reflect the new default
6. **Caching:** Coordinate with Ash/Parker on cache invalidation and URL compatibility

