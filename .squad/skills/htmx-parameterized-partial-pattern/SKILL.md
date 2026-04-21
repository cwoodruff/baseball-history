# Skill: htmx Parameterized Partial Pattern

**Category:** Frontend Architecture  
**Applies to:** Razor Pages with htmx  
**Created:** 2026-04-21 (Sprint 3, Compare migration)  
**Owner:** Dallas

## Problem

When a page needs to render similar UI regions with slight variations (different colors, targets, or behavior), duplicating markup creates maintenance burden and regression risk. Traditional approach would copy-paste the card markup for Player 1 and Player 2, leading to drift.

## Solution

Use tuple-based parameterized partials that accept multiple arguments to control variation.

### Pattern

```razor
@* Parent view: _CompareMain.cshtml *@
<div class="row">
    <div class="col-md-6">
        @await Html.PartialAsync("_ComparePlayerCard", 
            (p1, 1, p2Id, "135deg, #1a2744 0%, #2c3e6b 100%"))
    </div>
    <div class="col-md-6">
        @await Html.PartialAsync("_ComparePlayerCard", 
            (p2, 2, p1Id, "135deg, #8b1a2b 0%, #c62d42 100%"))
    </div>
</div>

@* Partial: _ComparePlayerCard.cshtml *@
@model (ComparePlayer? Player, int Side, string? OtherPlayerId, string Gradient)
@{
    var player = Model.Player;
    var side = Model.Side;
    var otherPlayerId = Model.OtherPlayerId;
    var gradient = Model.Gradient;
    var resultsDivId = $"search-results-{side}";
}

<div class="card">
    @if (player != null)
    {
        <div style="background: linear-gradient(@gradient);">
            <!-- Player detail markup -->
        </div>
    }
    else
    {
        <input hx-target="#@resultsDivId" ... />
        <div id="@resultsDivId"></div>
    }
</div>
```

## When to Use

- **Dual or multi-region layouts** where regions share structure but differ in:
  - Visual styling (colors, gradients, icons)
  - htmx target IDs (`#search-results-1` vs `#search-results-2`)
  - Query string parameters (`side=1` vs `side=2`)
  - Empty vs loaded state rendering
  
- **Comparison interfaces** (side-by-side player/team/product cards)
- **Multi-step wizards** with similar card structure but different data/behavior
- **Split views** with left/right asymmetry

## Benefits

1. **Single source of truth** — One partial for all variations
2. **Type-safe parameters** — Tuple provides compile-time safety
3. **Flexible styling** — Pass colors, gradients, or CSS classes as params
4. **Independent htmx targets** — Each instance can have unique target IDs
5. **Easier refactoring** — Change markup once, all instances update

## Constraints

- **Parameter order matters** — Document tuple structure clearly in partial header
- **Keep params minimal** — Too many params (>5) suggests over-parameterization
- **Name params explicitly** — Use destructuring in partial to clarify intent
- **Test both variants** — Integration tests should cover all parameter combinations

## Example from Compare Page

**Use case:** Two player selection cards with independent search, different colors, bidirectional query preservation.

**Tuple parameters:**
1. `ComparePlayer? Player` — null for empty (search mode), populated for loaded (detail mode)
2. `int Side` — 1 or 2, controls search handler `side` param and result div ID
3. `string? OtherPlayerId` — preserves the other player's selection in query strings
4. `string Gradient` — CSS gradient string for card background (blue for P1, red for P2)

**Dynamic behavior:**
- When empty: renders search input with `hx-target="#search-results-{side}"`
- When loaded: renders player detail with modal link
- Change button URL preserves other player: `?player1={otherPlayerId}` or `?player2={otherPlayerId}`
- Search URL includes both player params: `&player1={p1Id}&player2={p2Id}`

## Files

**Example implementation:**
- `baseball-history-web/Pages/Compare/_ComparePlayerCard.cshtml` (partial)
- `baseball-history-web/Pages/Compare/_CompareMain.cshtml` (caller)

## Alternative Approaches Considered

1. **Separate partials** (`_Player1Card.cshtml`, `_Player2Card.cshtml`) — rejected due to duplication
2. **Anonymous type params** — rejected, tuples provide better readability in caller
3. **Record type params** — considered but overkill for 4 simple params
4. **ViewData dictionary** — rejected, no compile-time safety

## Related Patterns

- `htmx-partial-pattern` — Base pattern for returning partials from PageModel
- `page-local-modal-decomposition` — Decomposing complex UI into page-local partials
- `htmx-response-cache-partial-pattern` — Caching partials with VaryByHeader

## Testing Guidance

When testing parameterized partials:
1. **Test both/all variants** — Compare page tests verify both player 1 and 2 search targets exist
2. **Test empty and loaded states** — Verify partial handles null player correctly
3. **Test parameter interaction** — Verify query strings preserve other selection
4. **Test htmx contracts** — Verify each instance has correct unique target IDs

## Decision Rationale

Parameterized partials chosen for Compare dual-search because:
- **DRY principle** — 80+ lines of markup reused instead of duplicated
- **Maintenance** — Future card changes happen once, not twice
- **Consistency** — Both cards guaranteed to have identical structure
- **Flexibility** — Easy to add third player in future by calling partial again with different params
- **Testability** — Test partial logic once, parameter variations separately

## Status

✅ Pattern validated in Sprint 3 Compare migration. All 350 tests pass. Pattern is production-ready and recommended for similar dual-region interfaces.
