# HTMX Migration Regression Baseline

## Use when

- A Razor Pages + htmx app is about to refactor layout or shared partials/components.
- The codebase has unit/integration coverage for helpers or models, but little or no handler/API coverage.

## Pattern

1. **Lock a green baseline first.** Record the existing suite result before adding migration coverage.
2. **Test contracts, not markup snapshots.** Prioritize:
   - full page vs non-boosted HTMX partial result type
   - pagination boundary clamping
   - representative `NotFound` API paths
   - shell lifecycle flows (boosted nav, modal host, global search host)
3. **Map blast radius by shared surface.**
   - Shell: layout, search host, modal container, global listeners
   - Primitives: pagination, alphabet nav, cards, filter/loading patterns
4. **Split migration lanes by file ownership.**
   - Shell changes in one lane
   - Shared primitives in another
   - Coverage expansion in a tests-only lane
5. **Gate merges on representative coverage, then expand opportunistically.**

## Why it works

The highest regression risk in htmx migrations is behavioral drift at the partial/full-page boundary and in shared shell listeners. A small contract suite catches those failures sooner than brittle full-HTML snapshots and keeps parallel workstreams reviewable.
