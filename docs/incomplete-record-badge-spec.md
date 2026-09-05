# Spec: Mark Partial-Name / Incomplete Player Records (#71)

**Batch**: Incomplete-record detection rule + badge on player surfaces
**Status**: Implemented (2026-09-05)

> Implementation notes: shipped as specced, with additions found during
> implementation. Three more inline `PlayerSummary`/`SearchResult`
> construction sites needed the flag (Players index, Search page, Compare
> page), and the optional API field was included. One Razor gotcha: partials
> referenced from views inside `Pages/Shared/Components` must use the
> `Components/` path prefix — a bare name throws view-not-found at runtime.
**Issue**: [#71](https://github.com/cwoodruff/baseball-history/issues/71), milestone *Data Transparency & Historical Context*

## Summary

453 of the 24,270 players in the database carry only a fragment of a name:
413 have no first name at all (`nameFirst` empty — Malcolm, Fresnell, Omeada,
eight different Smiths) and 40 more have a bare initial ("W. Cobb"). They
currently render exactly like fully documented players, so users read them as
data bugs instead of what they are: records where only a surname survived the
original box scores.

The era distribution confirms the historical story. Of the 413 surname-only
players, 362 debuted between 1910 and 1949 — the segregation era, where Black
baseball's box scores and rosters were unevenly preserved — and the remaining
~50 come from the 1870s–1900s NL/AA era (e.g. `smith01`, debut 1884). Badge
copy must therefore say "mostly", not "exclusively", segregation-era.

This spec adds a single server-side detection rule and renders a badge with an
explanation everywhere a player name appears as an identity (headers, cards,
list rows). Issue #72's feature page will later become the badge's click-through
destination; this spec leaves a placeholder for that link.

## Detection rule

A player is a **partial record** when, after trimming, `nameFirst` is:

- empty or null, **or**
- a single initial: one letter optionally followed by a period
  (regex `^[A-Za-z]\.?$`).

Explicitly *not* part of the rule: missing `birthYear` (978 players),
missing `debut` (3,030), or missing bats/throws (2,847). Badging those would
sweep in ~12% of all players and dilute the badge to noise; general data gaps
are #70's data-scope statement, not this badge. (In practice the rule already
correlates: 411 of the 413 surname-only players also lack a birth year.)

The rule lives in one place:

```csharp
// baseball-history-web/Services/PlayerRecordFacts.cs
public static class PlayerRecordFacts
{
    /// <summary>
    /// True when only a surname or a bare initial survived in the source
    /// record (nameFirst empty or a single initial like "W.").
    /// </summary>
    public static bool IsPartialName(string? nameFirst) { ... }
}
```

Same shared-static pattern as `LahmanNumbers`. No schema changes, no new
queries — every consuming view model already loads `NameFirst`.

## View model changes

Both factories compute the flag once; views never call the helper directly.

| View model | Change |
|---|---|
| `PlayerDetailViewModel` | add `bool IsPartialRecord`, set in `FromEntity` |
| `PlayerSummary` (`PlayerListViewModel.cs`) | add `bool IsPartialRecord`, set in `FromPeople` |
| `PlayerCacheService` (builds `FullName` itself) | populate the same flag |
| Compare page player lookups | reuse `PlayerRecordFacts.IsPartialName` |

## UI

Two render weights, one shared partial `_PartialRecordBadge.cshtml`:

1. **Full badge** — small muted-amber chip reading **"Partial record"**,
   rendered beside the name in `Details.cshtml` (page header),
   `_PlayerModal.cshtml` (modal header), and `_ComparePlayerCard.cshtml`.
   Tooltip (Bootstrap `title`/`data-bs-toggle="tooltip"`, falling back to the
   native `title` attribute) carries the explanation text below.
2. **Compact marker** — a superscript dagger (`†`) with the same tooltip and
   an `aria-label`, for dense rows: `_PlayerCard.cshtml` (players list /
   search results), `_CompareSearchResults.cshtml`, and leaderboard rows if a
   partial-record player appears there.

Both variants get `aria-label` text identical to the tooltip so screen
readers announce it; the dagger alone is never the only signal (the chip
appears on every click-through surface).

### Explanation text (needs approval — acceptance criterion)

> **Historically incomplete record.** Only a partial name survived in the
> original sources for this player — most such records come from
> segregation-era Black baseball, where box scores and rosters were unevenly
> preserved; a few date to the 19th century. Documented statistics may
> understate this player's actual career.

When #72 ships, append: *"Learn why →"* linking to the feature page.

## API (optional, additive)

Add `isPartialRecord: boolean` to the player list and detail DTOs in
`PlayerEndpoints.cs`. Non-breaking, one line per DTO, and it lets the MCP
server and API consumers surface the same honesty later. Skip if we want the
diff minimal; include if we're touching the DTOs anyway.

## Tests

Unit (`PlayerRecordFactsTests`):
- null, `""`, `"   "` → partial
- `"W."`, `"W"`, lowercase `"w."` → partial
- `"Walter"`, `"Wm"` (two letters), `"J.R."` → not partial

Integration/page:
- `smith01` details page and modal render the badge; `ruthba01` does not.
- Players list row for a partial-record player carries the dagger with
  `aria-label`.
- Badge text matches the approved copy exactly.

## Out of scope (explicitly)

- Badging missing birth data / debut alone (covered by #70's scope statement)
- The #72 feature page itself (badge link lands in that issue)
- MCP tool/resource changes
- Any data edits — the point is to present the gaps, not fill them
- Cross-referencing Seamheads/Retrosheet to *resolve* identities

## Implementation order

1. `PlayerRecordFacts` + unit tests (no UI risk).
2. View model flags (`PlayerDetailViewModel`, `PlayerSummary`,
   `PlayerCacheService`).
3. `_PartialRecordBadge.cshtml` partial; wire into Details header, modal
   header, `_PlayerCard`, Compare surfaces.
4. Copy approval on the explanation text (tracked on #71).
5. Optional API field; integration tests; `docs/FEATURES.md` update.

Steps 1–2 are pure back-end and independently mergeable; 3 is the only
visual change and the main review focus.
