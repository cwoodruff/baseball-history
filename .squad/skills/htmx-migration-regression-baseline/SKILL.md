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

## Concrete integration pattern

1. Use `WebApplicationFactory<Program>` and create the client with `AllowAutoRedirect = false`.
2. Split the suite by contract type instead of feature sprawl:
   - routing contracts in `Pages/PageRoutingIntegrationTests.cs`
   - pagination boundaries in `Pages/PagePaginationIntegrationTests.cs`
   - representative API edges in `ApiEdgeIntegrationTests.cs`
3. Assert stable response markers, not snapshots:
   - full page: contains `<!DOCTYPE html>` plus a page wrapper like `id="filter-form"` or `id="team-list"`
   - non-boosted htmx: omits `<!DOCTYPE html>` and page wrappers, but still contains the partial-specific title/content
4. For pagination, parse the rendered `Page X of Y` summary and assert clamping (`page <= 0` → 1, oversized page → `TotalPages`).
5. Start API error coverage with the routes most likely to drift during migration work: entity detail endpoints and their immediate subroutes.

## Example targets

- `/Stats/Batting?stat=ops&page=1`
- `/Stats/Pitching?stat=era&page=1`
- `/Teams?league=AL`
- `/api/players/not-a-real-player`
- `/api/teams/seasons/ZZZ/AL/2099`

## Scope gate for safe slices

1. Read the issue scope before reading test output.
2. Compare the changed-file set to the expected blast radius.
   - Example: a primitives slice should mainly touch shared component/filter files, not `_Layout`, search host, modal host, or package wiring.
3. Only after the diff matches the intended lane should green tests count toward approval.
4. If the implementation is green but belongs to a different lane (#4/#6 shell work vs #7 primitives), reject for scope drift and require a split revision.

## Anti-Patterns

- Approving a “safe primitives” slice because tests passed even though the diff mostly changes shell/layout files.
- Using search/modal/pagination regressions as approval evidence when the review explicitly carved those surfaces out of scope.
- Treating package wiring or proof-of-integration changes as equivalent to reusable primitive extraction.
