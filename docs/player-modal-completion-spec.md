# Spec: Complete the Player Modal

**Batch**: Postseason player stats + Fielding tab + All-Star display upgrade
**Status**: Implemented (2026-09-04)
**Date**: 2026-09-04

> Implementation notes: shipped as specced, with two deviations. The
> string-column defense test runs against `LahmanNumbers.ParseIntOrZero`
> directly instead of seeded rows (tests hit the shared PostgreSQL database,
> not a seedable one), and Babe Ruth renders Pitching as the first tab since
> the pitchers-first ordering rule keys off `IsPitcher`.

## Summary

The player modal (and the full player page, which shares the same partials) shows
batting and pitching seasons, awards, teams, and an All-Star count badge. Three
gaps remain, all backed by Lahman tables that are already modeled in
`BaseballDbContext` and already exposed through the REST API:

1. **Postseason stats** — `BattingPost` / `PitchingPost` have API endpoints
   (`/api/players/{id}/postseason/batting|pitching`) but no UI. Fans ask "was he
   clutch?" and the modal can't answer.
2. **Fielding stats** — `Fielding` has an API endpoint
   (`/api/players/{id}/fielding`) but no UI. Position players show only a batting
   table; there is no answer to "what position did he play?"
3. **All-Star display** — the count badge exists but counts `AllstarFull` rows,
   not seasons. In 1959–1962 MLB played two All-Star Games per year, so e.g.
   Hank Aaron shows "25× All-Star" in the modal while the Compare page (which
   counts distinct years, `Compare/Index.cshtml.cs:355`) correctly shows 21.
   The badge also gives no indication of *when* the selections happened.

Because `Modal.cshtml` and `Details.cshtml` both render `_PlayerModalOverview`,
`_PlayerCareerSummary`, and the seasons tables from one `PlayerDetailViewModel`
loaded by `PlayerDetailService`, every change below lands on **both surfaces
at once**. No new pages, no new routes, no schema changes, no API changes.

## Current architecture (what we build on)

| Piece | File | Role |
|---|---|---|
| Service | `baseball-history-web/Services/PlayerDetailService.cs` | Single query pipeline for the whole view model |
| View model | `baseball-history-web/ViewModels/PlayerDetailViewModel.cs` | Bio, career stats, seasons, awards, All-Star records |
| Modal shell | `Pages/Players/_PlayerModal.cshtml` | Bootstrap modal, tab logic for two-way players |
| Full page | `Pages/Players/Details.cshtml` | Same partials, same tab logic (duplicated markup) |
| Overview column | `Pages/Players/_PlayerModalOverview.cshtml` | Bio card, Teams card, Honors card (All-Star badge lives here) |
| Season tables | `_PlayerBattingSeasonsTable.cshtml`, `_PlayerPitchingSeasonsTable.cshtml` | `table-baseball` pattern, team links, scrollable 400px card |
| API reference | `Api/Endpoints/PlayerEndpoints.cs` | `GetPlayerFielding`, `GetPlayerPostseasonBatting/Pitching` — reuse these exact query shapes |
| Round names | `ViewModels/PostseasonViewModel.cs` (`RoundNames`) | `WS` → "World Series" etc. — reuse, do not duplicate |

**Lahman data quirk** (already handled in the API layer, must be handled again in
the service): several numeric columns are stored as strings and may be empty
rather than null — `Fielding.Po/A/E/Dp/InnOuts/Gs`, `BattingPost.So/Cs`,
`Appearances.G*`. Follow the `ParseIntOrZero` pattern from
`PlayerEndpoints.cs:293` — parse defensively in memory, never `int.Parse` inside
the EF query.

---

## Feature 1: Postseason tab

### Data

Two new queries in `PlayerDetailService.GetPlayerDetailAsync`, mirroring the
API endpoint shapes:

- `BattingPost` where `PlayerId == id`, ordered by `YearId` desc then round
  chronology (see below), projected to a new `PostseasonBattingRecord`:
  Year, Round, TeamId, TeamName (via `b.Team.Name` nav), LgId, G, AB, R, H,
  2B, 3B, HR, RBI, SB, BB. AVG as a computed property (same
  `FormattedAvg` pattern as `SeasonBattingRecord`).
- `PitchingPost` where `PlayerId == id`, same ordering, projected to
  `PostseasonPitchingRecord`: Year, Round, TeamId, TeamName, LgId, G, GS, W,
  L, SV, IP (`Ipouts / 3.0`), H, ER, BB, SO. ERA computed
  (`ER * 27.0 / Ipouts`), formatted like `SeasonPitchingRecord`.

Both record types get `RoundName =>
PostseasonViewModel.RoundNames.GetValueOrDefault(Round, Round)`.

**Round ordering within a year**: rounds must read in playoff chronology, not
alphabetically (the API currently sorts `ALDS1` before `ALWC`; the UI should
not). Add a small static rank — WC (incl. `ALWC`/`NLWC`) = 0, DS
(`ALDS1/2`, `NLDS1/2`, `*DIV`) = 1, CS (`ALCS`/`NLCS`, historic `CS`) = 2,
`WS` = 3, unknown = 4 — and sort by Year desc, then rank ascending, in memory
after the query. Put the rank next to `RoundNames` so the two stay together.

**Career totals**: computed properties on `PlayerDetailViewModel`
(`PostseasonBattingTotals`, `PostseasonPitchingTotals`) that aggregate the
loaded rows in memory into the existing `CareerBattingStats` /
`CareerPitchingStats` types — no extra queries, and the formatted-stat helpers
come for free.

### UI

New partial `_PlayerPostseasonTable.cshtml`, one tab (see "Tab restructure"
below). Inside the tab, stacked cards in the established
`card-header-mlb` / `table-baseball` style:

1. **Batting card** (if any `BattingPost` rows): totals row pinned at top
   (`<tfoot>` moved to a bolded first row labeled "Career"), then one row per
   year+round. Columns: Year, Round, Team, G, AB, H, HR, RBI, AVG.
2. **Pitching card** (if any `PitchingPost` rows): Career totals row, then
   Year, Round, Team, G, W–L, SV, IP, SO, ERA.

Card order follows `IsPitcher` (pitchers see pitching first). Round cell links
to the existing Postseason browser: `/Postseason?year={Year}` (the page already
supports year + round filters via `SelectedRound`). Team cell links to
`/Teams/Season/{TeamId}/{LgId}/{Year}` exactly like the regular-season tables.

### Edge cases

- No postseason appearances → tab is not rendered at all (famously: Ernie
  Banks). No empty state needed; absence of the tab is the state.
- 19th-century rounds (`CS`) fall through `RoundNames` and display the raw
  code — acceptable, already the Postseason page's behavior.
- `BattingPost.So` is a string column: parse with `ParseIntOrZero` if SO is
  displayed (v1 columns above omit it; totals that need it must parse).

---

## Feature 2: Fielding tab

### Data

One query in `PlayerDetailService` against `Fielding` (same shape as
`GetPlayerFielding` in `PlayerEndpoints.cs:266`), pulled into memory then
aggregated **by position** for the primary display:

- `CareerFieldingRecord` (new): Pos, G, PO, A, E, DP, plus computed
  `FieldingPercentage => (PO + A + E) > 0 ? (PO + A) / (double)(PO + A + E) : 0`
  and `FormattedPct` (".000"-style, matching `FormattedAvg`).
- Ordered by G descending — the player's main position reads first.
- Season-by-season detail: `SeasonFieldingRecord` (Year, TeamId, TeamName,
  LgId, Pos, G, PO, A, E, DP, FPct), ordered Year desc then Pos. Rows for the
  same year+position across stints may simply repeat (matches how
  `BattingSeasons` handles stints today).

PO/A/E/DP are string columns — parse in memory (`ParseIntOrZero`). To get
`TeamName` without N+1 lookups, select `f.Team.Name` in the projection like
`BattingSeasons` does.

### UI

New partial `_PlayerFieldingTable.cshtml`:

1. **Career by Position card** — small table: Pos, G, PO, A, E, DP, FPct.
   This is the headline answer ("Ozzie Smith: SS, 2511 G, .978").
2. **Season by Season card** — the existing scrollable 400px pattern: Year,
   Team (linked), Pos, G, PO, A, E, DP, FPct.

Position codes (`P`, `C`, `1B`, `SS`, `OF`…) display as-is — they are
universally understood; no friendly-name mapping needed.

### Edge cases

- Pitchers have Fielding rows too (as `P`) — the tab shows for them as well;
  that is correct and interesting (Greg Maddux's fielding).
- `OF` vs `LF/CF/RF`: Lahman splits outfield into `LF/CF/RF` only from 1954
  (`FieldingOfsplit`); earlier years use `OF`. v1 shows whatever `Fielding`
  contains and does not merge — note it in the card footer if desired, but no
  logic.
- DH-only seasons produce no Fielding rows — nothing to special-case.
- Catcher-specific columns (PB, SB, CS) are **out of scope** for v1.

---

## Feature 3: All-Star display upgrade

### Changes

1. **Count seasons, not rows.** In `PlayerDetailService`, the badge count
   becomes `AllStarAppearances.Select(a => a.Year).Distinct().Count()`.
   Simplest form: add `AllStarSelectionCount` computed property on
   `PlayerDetailViewModel` and switch `_PlayerModalOverview.cshtml:110` to it.
   This aligns the modal with Compare (`Compare/Index.cshtml.cs:355`), which
   already counts distinct years.
2. **Show the years.** Under the badge in the Honors card, add a muted small
   line listing selection years compressed into ranges:
   `1955–1975` for continuous runs, comma-separated otherwise
   (`1936–1942, 1946–1951` — war-service gaps are real and informative).
   Add a `AllStarYearRanges` computed property on the view model that
   produces this string; it is pure logic and unit-testable.
3. Keep `AllStarAppearances` (per-game rows) untouched — the API and any
   future All-Star page still want game-level data.

---

## Shared change: tab restructure

`_PlayerModal.cshtml` and `Details.cshtml` currently duplicate a conditional:
tabs only when `IsTwoWayPlayer`, otherwise a single bare table. With four
possible sections this becomes untenable. Replace both copies with **one new
shared partial** `_PlayerStatsTabs.cshtml`:

- Build the tab list from availability:
  **Batting** (`BattingSeasons.Any()`), **Pitching** (`PitchingSeasons.Any()`),
  **Fielding** (`FieldingSeasons.Any()`), **Postseason**
  (`PostseasonBattingSeasons.Any() || PostseasonPitchingSeasons.Any()`).
- Tab order: pitchers see Pitching first, everyone else Batting first;
  Fielding then Postseason follow. First available tab is active.
- If only one tab would render, render its table bare with no tab strip —
  preserving today's look for e.g. a batter with no fielding or postseason
  data.
- Keep the existing Bootstrap tabs markup and per-player element IDs
  (`#batting-@Model.PlayerId` etc.) so multiple modals on one page never
  collide; new panes follow the same convention (`#fielding-…`,
  `#postseason-…`).

This nets out to *less* markup than today (the two-way conditional is deleted
from two files) and is the piece that makes the batch feel like one feature.

## Performance & caching

`GetPlayerDetailAsync` grows from ~9 queries to ~12 (postseason ×2, fielding
×1; All-Star is already loaded). All three are single-player indexed lookups
returning at most a few dozen rows — negligible. The modal already carries
`[ResponseCache(Duration = 3600)]` client caching, unchanged. Eager loading is
preferred over htmx lazy-loading tabs: it keeps `Details.cshtml` (server-
rendered, same partials) trivial, and the payload delta is a few KB of HTML.

## API and MCP

No changes. The REST API already exposes all three datasets; this batch brings
the UI to parity. Optionally, `docs/FEATURES.md` gains the new modal sections
and `ApiDocs` stays as-is. If the MCP server should expose
postseason/fielding tools, that is a separate batch.

## Testing

Follow the existing test project layout (`baseball-history-tests`):

**ViewModels** (pure unit tests, no DB — alongside `CareerBattingStatsTests`):
- `CareerFieldingRecordTests`: FPct math incl. divide-by-zero (0 chances → .000
  not NaN); ordering.
- `PostseasonRecordTests`: ERA/AVG math, `RoundName` mapping incl. unknown
  round fallback, round-chronology sort (WC < DS < CS < WS; `ALDS1` vs `ALWC`).
- `PlayerDetailViewModelTests` (extend): `AllStarSelectionCount` dedupes
  1959–62 double-game years; `AllStarYearRanges` compresses runs and preserves
  gaps; `PostseasonBattingTotals` aggregation.

**Pages** (integration, alongside `PlayerDetailsPageTests`):
- Modal for a postseason-rich player (Yogi Berra `berrayo01`) renders the
  Postseason tab with a World Series row and a Career totals row.
- Modal for a no-postseason player (Ernie Banks `bankser01`) renders **no**
  Postseason tab.
- Two-way player (Babe Ruth `ruthba01`) renders all four tabs in order and
  batting is active.
- Fielding tab for `smithoz01` shows SS as the top career-position row.
- String-column defense: a player with empty-string PO/A/E rows renders 0,
  no exception (seed via `TestDatabaseFactory`).
- All-Star badge for `aaronha01` reads 21, not 25.

## Out of scope (explicitly)

- All-Star roster browser page (future feature; data layer untouched here)
- Catcher PB/SB/CS and `FieldingOfsplit` LF/CF/RF merging
- Advanced fielding metrics (ZR is unreliable in Lahman)
- Postseason series win/loss context from `SeriesPost`
- MCP tool additions
- Compare-page postseason/fielding columns

## Implementation order

1. View model records + computed properties, with unit tests (no UI risk).
2. `PlayerDetailService` queries (+ move/share `ParseIntOrZero` and the round
   rank helper into the web project's shared space).
3. `_PlayerStatsTabs.cshtml` refactor; swap into `_PlayerModal.cshtml` and
   `Details.cshtml`; verify no regression for batter-only / pitcher-only /
   two-way players.
4. `_PlayerPostseasonTable.cshtml` + `_PlayerFieldingTable.cshtml`.
5. All-Star badge fix + year ranges in `_PlayerModalOverview.cshtml`.
6. Integration tests; update `docs/FEATURES.md`.

Steps 1–2, 4, and 5 are independent of each other; 3 is the only structural
change and the main review focus.
