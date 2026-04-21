---
name: "minimal-leaderboard-migration"
description: "Migrate leaderboard pages to htmxRazor by replacing badges only, preserving all backend and frontend contracts."
domain: "frontend-backend"
confidence: "high"
source: "earned"
---

## Context

Use this pattern when migrating leaderboard-style pages that already follow best practices: projection-first queries, expression-tree ordering, response caching, filter forms with htmx wiring, and pagination. The page works correctly and needs minimal htmxRazor adoption.

## When to Use

✅ **Use this pattern for:**
- Leaderboard pages with filter forms and sortable tables
- Pages that use expression trees for dynamic ordering
- Pages with complex query logic that should not be refactored
- Pages where backend contracts are stable and well-tested
- Pages where only presentational components (badges, cards) need updating

❌ **Don't use this pattern for:**
- Pages with broken or inefficient queries (fix first)
- Pages without existing test coverage (add tests first)
- Pages where filter form or table structure needs redesign
- Pages where backend handler needs refactoring

## Migration Steps

### 1. Identify Badge Replacement Opportunities

Look for Bootstrap badges that can be replaced with htmxRazor equivalents:

```razor
<!-- BEFORE -->
<span class="badge bg-light text-dark">@count players</span>
<span class="hof-badge">HOF</span>

<!-- AFTER -->
<rhx-badge rhx-variant="neutral">@count players</rhx-badge>
<rhx-badge rhx-variant="warning" rhx-size="sm">HOF</rhx-badge>
```

**Badge Variant Guide:**
- `neutral` — Non-interactive metadata (counts, totals)
- `warning` — Achievements, awards, special status (HOF, All-Star)
- `success` — Active/positive status
- `brand` — Team/league affiliation

### 2. Preserve All Backend Contracts

**DO NOT change:**
- Route paths (`/Stats/Batting`)
- Handler signatures (`OnGetAsync` parameters)
- Query parameter names (`stat`, `fromYear`, `toYear`, `league`, `minAb`, `singleSeason`, `page`)
- Response cache attributes (`[ResponseCache(Duration=3600, VaryByHeader="HX-Request")]`)
- Cache key names (`batting_years`, `batting_leagues`, `hof_player_ids`)
- Query logic (expression trees, aggregations, projections)
- Pagination size or calculation

### 3. Preserve All Frontend Contracts

**DO NOT change:**
- Filter form ID (`#filter-form`)
- Target container IDs (`#leaderboard`, `#loading-indicator`)
- htmx attributes (`hx-get`, `hx-include`, `hx-target`, `hx-indicator`, `hx-push-url`)
- Table structure (columns, sorting links)
- Pagination target and query param preservation
- Modal link contracts (`hx-get="/Players/Modal/{id}"` with `hx-target="#modal-container"`)

### 4. Verify with Existing Tests

**DO NOT add new tests** unless you're changing behavior. Badge replacements are presentational only.

Run existing test suite:
```bash
dotnet build baseball-history.sln --no-restore
dotnet test baseball-history-tests --no-build
```

Expected: All tests pass (350+), no regressions.

## Examples

### Batting Leaders (Issue #12)

**Changed:**
- HOF badge: `<span class="hof-badge">` → `<rhx-badge rhx-variant="warning" rhx-size="sm">`
- Count badge: `<span class="badge bg-light text-dark">` → `<rhx-badge rhx-variant="neutral">`

**Preserved:**
- 15 stat ordering expressions (HR, H, R, RBI, SB, 2B, 3B, BB, G, AB, AVG, OBP, SLG, OPS, TB)
- Single-season vs career query paths
- Filter form with 6 controls
- Pagination with filter preservation
- Expression tree helpers (DynExpr, DynComputedExpr, DynSlgExpr, DynOpsExpr, DynTbExpr)

**Result:** 350/350 tests passing, zero regressions

### Pitching Leaders (Issue #13)

**Changed:**
- Badge 1: `<span class="hof-badge ms-1">HOF</span>` → `<rhx-badge rhx-variant="warning" rhx-size="sm" class="ms-1">HOF</rhx-badge>`
- Badge 2: `<span class="badge bg-light text-dark">` → `<rhx-badge rhx-variant="neutral">`

**Preserved:**
- 13 stat ordering expressions (W, L, SO, SV, CG, SHO, IP, ERA, WHIP, K9, BB9, WPct, G, GS)
- ERA/WHIP ascending sort semantics with ↑ arrows (lower is better)
- Single-season vs career query paths
- Filter form with 6 controls
- All backend contracts (routes, handlers, cache keys)
- All frontend contracts (targets, indicators, pagination)

**Result:** 306/306 tests passing, zero regressions

### Pitching Leaders (Issue #13 - COMPLETED)

**Changes made:**
- HOF badge in `_PitchingLeaders.cshtml`: Replaced `<span class="hof-badge ms-1">` with `<rhx-badge rhx-variant="warning" rhx-size="sm" class="ms-1">`
- Count badge in card header: Replaced `<span class="badge bg-light text-dark">` with `<rhx-badge rhx-variant="neutral">`

**Preserved:**
- 13 stat ordering expressions (W, L, ERA, SO, SV, CG, SHO, IP, WHIP, K9, BB9, WPct, G, GS)
- ERA/WHIP/BB9 ascending sort semantics (lower is better)
- Single-season vs career query paths
- Filter form structure
- All backend contracts (routes, handlers, queries, caching)
- All frontend contracts (targets, indicators, pagination)

## Anti-Patterns

### ❌ Refactoring Backend While Migrating UI

```csharp
// WRONG — Don't extract shared expression tree helpers during migration
public static class SharedOrderingHelpers { ... }

// RIGHT — Preserve existing duplication, defer refactor to separate task
private static IOrderedQueryable<T> ApplyBattingOrder<T>(...) { ... }
```

### ❌ Changing Filter Form Structure

```razor
<!-- WRONG — Don't consolidate or redesign filters during migration -->
<rhx-filter-group> ... </rhx-filter-group>

<!-- RIGHT — Keep existing filter form, only replace badges -->
<form id="filter-form">
    <select class="form-select" name="stat" hx-get="..." ...>
```

### ❌ Adding New Features

```razor
<!-- WRONG — Don't add new filters, sorting modes, or export buttons -->
<button hx-get="/Stats/Batting/Export">Export CSV</button>

<!-- RIGHT — Migrate existing features only -->
```

### ❌ Removing Loading Overlays

```razor
<!-- WRONG — Don't remove existing loading behavior -->
<!-- (no loading indicator) -->

<!-- RIGHT — Preserve existing loading overlay and spinner -->
<div id="loading-indicator" class="htmx-indicator loading-overlay">
    @await Html.PartialAsync("Components/_LoadingSpinner", "Loading stats...")
</div>
```

## Quality Gates

Before declaring migration complete:

1. ✅ Build passes without warnings
2. ✅ All existing tests pass (no new test failures)
3. ✅ Route paths unchanged (manually verify in browser or integration tests)
4. ✅ Query parameters preserved (check URL bar after filter changes)
5. ✅ htmx targets still working (check Network tab for partial responses)
6. ✅ Pagination preserves filters (navigate to page 2, verify filters in URL)
7. ✅ Response cache still varies by HX-Request header (check response headers)
8. ✅ Leaderboard ordering correct (spot-check first/last entries for a stat)

## Migration Decision Template

```markdown
## Issue #XX: [Page Name] Leaders Migration

**Changed:**
- Badge 1: [old markup] → `<rhx-badge rhx-variant="...">` 
- Badge 2: [old markup] → `<rhx-badge rhx-variant="...">` 

**Preserved:**
- N stat ordering expressions ([list])
- [Query mode descriptions]
- Filter form with N controls
- All backend contracts (routes, handlers, cache keys)
- All frontend contracts (targets, indicators, pagination)

**Result:** [N]/[N] tests passing, zero regressions

**Blockers for next page:** NONE
```

## When to Deviate from This Pattern

If you encounter any of these, **do not use this minimal pattern**:

- ❌ Tests failing before migration (fix tests first)
- ❌ Query uses `.Include()` without projection (refactor to projection-first first)
- ❌ Response cache missing `VaryByHeader="HX-Request"` (add it first)
- ❌ Filter form doesn't preserve selections on pagination (fix filter contracts first)
- ❌ Leaderboard ordering produces incorrect results (fix expression trees first)
- ❌ Page has N+1 query problems (fix query patterns first)

**Principle:** Only migrate pages that are already well-architected. Fix underlying issues before applying cosmetic htmxRazor adoption.
