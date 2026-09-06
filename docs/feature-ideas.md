# Feature Ideas — Next Wave

Candidate features for Baseball History, grounded in data the database
already holds but the site doesn't yet surface, plus natural extensions of
shipped features. Drafted 2026-09-05; tracked as GitHub issues.

| # | Feature | Data source | Effort |
|---|---------|-------------|--------|
| 1 | Advanced player statistics in player details | New SQL (provided separately) | TBD |
| 2 | Ballparks browser | `Parks`, `HomeGames` (API already exists) | Medium |
| 3 | Managers browser | `Managers`, `ManagersHalf`, `AwardsManagers` | Medium |
| 4 | All-Star roster browser | `AllstarFull` | Small–Medium |
| 5 | College & school origins | `Schools`, `CollegePlaying` | Medium |
| 6 | Negro Leagues hub | `Teams`/`Batting`/`Pitching` league IDs | Medium–Large |
| 7 | Player similarity scores | Computed from career stats | Medium |
| 8 | Franchise postseason head-to-head | `SeriesPost` | Small–Medium |
| 9 | "This day in baseball" home widget | `People` debut/final/birth dates | Small |
| 10 | Leaderboard CSV export | Existing leaderboard queries | Small |
| 11 | MCP tool expansion | Existing postseason/fielding/parks queries | Medium |

---

## 1. Advanced player statistics in player details

Incorporate the shared SQL query layer in
[`qualification_and_league_index.sql`](./qualification_and_league_index.sql)
into the player modal and details page. The layer defines views for:

- **Season-relative qualification** — thresholds derived from actual team
  schedule length (3.1 PA × weighted team games; 1 IP per team game for
  pitchers), so short-schedule seasons (Negro Leagues, 19th century,
  strike years) qualify against the schedules actually played.
- **New batting rates** — PA, TB, ISO, BABIP, BB%, K%, and `ops_index`
  ("OPS vs. League", 100 = league average; league-relative, *not*
  park-adjusted, so never labeled OPS+).
- **Per-162 normalization** — HR/H/RBI per 162 team games, making a
  74-game season legible next to a 154-game one.
- **Career thresholds** — career qualification as the sum of season
  thresholds faced (Gibson's documented PAs qualify; a flat 3,000-AB
  floor erases him).
- **Pitching equivalents** — K/9, BB/9, season-relative IP qualification.

One code path: website, API, and MCP server all read these views so
"qualified" cannot diverge between surfaces. Ship with a glossary page
covering the file's documented caveats (no park factors; league baseline
includes pitchers; Negro Leagues team games reflect documented play;
early-era OBP approximations). Note: the SQL assumes lowercase unquoted
identifiers and needs a quoting pass for this database's mixed-case
schema before it can be applied.

**User benefit**: richer answers on the page users visit most — and rate
stats that finally treat short-schedule careers fairly.

## 2. Ballparks browser

The REST API already serves `/api/parks` (list + detail with attendance)
but there is no UI. Add a park browser and park detail pages: which teams
called it home and when (`HomeGames`), season attendance trends, park
lifespan. Link park names from team season pages.

**User benefit**: "what was the park, and who played there" is a common
research path with zero UI today; the backend work is already done.

## 3. Managers browser

`Managers`, `ManagersHalf`, and `AwardsManagers` are completely unused.
Add manager career pages (seasons, teams, W-L, pennants/titles), a
managers index, Manager of the Year with full voting via
`AwardsShareManagers`, and player-manager identification (many early
figures were both — link from their player page).

**User benefit**: an entire dimension of baseball history the site
currently pretends doesn't exist.

## 4. All-Star roster browser

Explicitly deferred from the player-modal completion batch. Browse
All-Star rosters by year with league splits, handling the 1959–1962
two-games-per-year seasons correctly (the badge already counts seasons,
not games). Link each player's badge years to the roster pages.

**User benefit**: completes the All-Star story the badge started.

## 5. College & school origins

`Schools` and `CollegePlaying` are unused. School pages listing every
major leaguer produced, years attended, a "top baseball factories"
leaderboard, and a school line on player pages.

**User benefit**: "who else went to my school" is one of the stickiest
casual-fan questions in baseball.

## 6. Negro Leagues hub

The transparency work (partial-record badges, /SurvivingRecords, data
scope) tells users *about* the record; a hub would help them *browse* it:
filter by league (NNL, ECL, NAL, …), league season pages, team pages for
the great clubs (Grays, Monarchs, Crawfords), linked prominently from
/SurvivingRecords.

**User benefit**: turns the site's honesty story into a discovery
experience; strongest differentiator on the list.

## 7. Player similarity scores

Compute career-shape similarity (Bill James-style similarity scores or a
simpler stat-distance) offline or at query time, and show "most similar
players" on player details with one-click compare links.

**User benefit**: drives exploration loops ("who was the poor man's
Rickey Henderson?") and feeds the Compare feature organically.

## 8. Franchise postseason head-to-head

Aggregate `SeriesPost` into franchise-vs-franchise postseason records
(series and games, by round, all time). Add to franchise pages and the
Postseason browser; Yankees–Dodgers alone justifies it.

**User benefit**: settles a whole genre of arguments with one page.

## 9. "This day in baseball" home widget

Home-dashboard widget for debuts, final games, and birthdays matching
today's date, each linking to the player. Cheap query, rotating daily
content.

**User benefit**: gives returning visitors something new every day; makes
the home page a daily habit rather than a jump-off point.

## 10. Leaderboard CSV export

The Compare page already exports CSV; leaderboards don't. Add a
"Download CSV" button to batting/pitching leaderboards honoring the
active filters, with attribution headers per the licensing page.

**User benefit**: researchers stop hand-copying tables; pairs naturally
with the API for the non-programmer audience.

## 11. MCP tool expansion

The MCP server exposes 12 tools but none for postseason stats, fielding,
parks, or managers — data the site/API already query. Add
`get_player_postseason`, `get_player_fielding`, `get_park`, and (once #3
ships) manager tools, updating the workflow-guide resource.

**User benefit**: the AI-assistant audience — including the SABR beta
testers — gets the full dataset, not the 2024 subset of it.
