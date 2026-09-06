# Features Documentation

This document provides detailed information about each feature of the Baseball
History application.

## Feature Overview

| Feature            | Description                           | URL                             |
|--------------------|---------------------------------------|---------------------------------|
| Home Dashboard     | Overview with quick stats and leaders | `/`                             |
| Player Browser     | Alphabetical player listing           | `/Players`                      |
| Player Modal       | Detailed player view                  | `/Players/Modal/{id}`           |
| Player Comparison  | Head-to-head 2-player comparison      | `/Compare`                      |
| Team Browser       | Franchise listing                     | `/Teams`                        |
| Franchise History  | Team history by franchise             | `/Teams/Franchise/{id}`         |
| Team Season        | Single season team view               | `/Teams/{teamId}/{lgId}/{year}` |
| Ballpark Browser   | Ballpark listing with filters         | `/Parks`                        |
| Ballpark Detail    | Park history and attendance trends    | `/Parks/{parkKey}`              |
| Negro Leagues Hub  | The seven recognized leagues          | `/NegroLeagues`                 |
| League Detail      | Seasons and clubs for one league      | `/NegroLeagues/{lgId}`          |
| League Season      | Standings and leaders for one season  | `/NegroLeagues/{lgId}/{year}`   |
| Managers Browser   | Manager listing with career records   | `/Managers`                     |
| Manager Career     | Season-by-season managerial record    | `/Managers/{playerId}`          |
| Batting Leaders    | Batting statistical leaders           | `/Stats/Batting`                |
| Pitching Leaders   | Pitching statistical leaders          | `/Stats/Pitching`               |
| Hall of Fame       | HOF inductee browser                  | `/HallOfFame`                   |
| Awards & Voting    | Award winners and voting breakdowns   | `/Awards`                       |
| Postseason         | Playoff series results                | `/Postseason`                   |
| Salary Explorer    | Player salary data and team payrolls  | `/Salaries`                     |
| Search             | Global search                         | `/Search`                       |
| Surviving Records  | Segregation-era record survival story | `/SurvivingRecords`             |
| Stats Glossary     | Advanced stat definitions and caveats | `/Glossary`                     |
| API Documentation  | REST API reference                    | `/ApiDocs`                      |
| REST API           | JSON API (30+ endpoints)              | `/api/*`                        |

---

## Home Dashboard

**URL**: `/`

The home page provides an overview of the baseball history database.

### Components

1. **Hero Section**
    - Application title and branding
    - Total years of history covered
    - Quick navigation buttons

2. **Stats Overview**
    - Total players in database
    - Total franchises
    - Hall of Fame inductees count

3. **Career Leaders**
    - Top 10 career home run leaders
    - Top 10 career wins leaders
    - Click player name to view modal

4. **Recent HOF Inductees**
    - Most recent Hall of Fame inductees
    - Shows name, career years

5. **Quick Links**
    - Cards linking to main sections

---

## Player Browser

**URL**: `/Players`

Browse all players alphabetically with career statistics.

### Features

- **Alphabet Navigation**: Click letters A-Z to filter
- **Player Cards**: Grid of player cards with:
    - Player initials
    - Full name
    - Career years
    - Key stats (H, HR, G)
    - HOF badge if applicable
- **Pagination**: 48 players per page
- **Click to View**: Click card to open player modal

### Query Parameters

| Parameter | Description                 | Example     |
|-----------|-----------------------------|-------------|
| `letter`  | Filter by last name initial | `?letter=R` |
| `page`    | Page number                 | `?page=2`   |

---

## Player Modal

**URL**: `/Players/Modal/{playerId}`

Detailed player information displayed in a modal overlay.

### Sections

1. **Player Info**
    - Full name
    - Birth date and location
    - Death date (if applicable)
    - Height/Weight
    - Bats/Throws
    - Debut and final game dates
    - Career length

2. **Teams**
    - List of teams played for
    - Years with each team

3. **Honors**
    - All-Star selection count (seasons, not games — 1959–1962 held two
      All-Star Games per year) with selection years shown as ranges
    - Awards list (MVP, Cy Young, etc.)

4. **Career Batting** (if applicable)
    - Games, AVG, Hits, HR, RBI, OPS

5. **Career Pitching** (if applicable)
    - W-L, ERA, Games, SO, SV, WHIP

6. **Stats Tabs** (each tab appears only when the player has data for it;
   pitchers see Pitching first)
    - **Batting**: year-by-year batting statistics, plus an Advanced Batting
      table (PA, ISO, BABIP, BB%, K%, OPS vs Lg, HR/162, season-relative
      qualification marks) from the shared query layer
    - **Pitching**: year-by-year pitching statistics, plus an Advanced
      Pitching table (IP, K/9, BB/9, WHIP, qualification marks)
    - **Fielding**: career totals by position, then season-by-season
      records (G, PO, A, E, DP, fielding percentage)
    - **Postseason**: batting and pitching lines per year and round with
      career totals; rounds link to the Postseason browser

---

## Partial-Record Badge

Cross-cutting annotation for historically incomplete player records
([#71](https://github.com/cwoodruff/baseball-history/issues/71)).

453 players carry only a fragment of a name — 413 with no first name at all
(mostly segregation-era Black baseball, some 19th-century) and 40 with a bare
initial like "W. Cobb". These records are flagged rather than smoothed over:
incompleteness is presented as a historical fact of how box scores survived.

### Detection

A player is a partial record when `nameFirst` is empty or a single initial
(`PlayerRecordFacts.IsPartialName`). Missing birth data alone does not
trigger the badge.

### Display

- **"Partial record" chip**: player page header, player modal header,
  comparison player cards; links to `/SurvivingRecords`, the feature page
  on why these records survived as fragments
  ([#72](https://github.com/cwoodruff/baseball-history/issues/72))
- **Dagger marker (†)**: player browser cards, search results (global and
  compare search)
- Both carry an explanation via tooltip and `aria-label`; the copy lives in
  `PlayerRecordFacts.PartialRecordExplanation`

### API

Player list and detail endpoints include an `isPartialRecord` boolean.

---

## Team Browser

**URL**: `/Teams`

Browse all MLB franchises.

### Display

- **Franchise Cards**: Grid showing:
    - Team logo placeholder (initials)
    - Franchise name
    - Years active
    - Total seasons
    - Active/Inactive status
- **Sorting**: Sorted by franchise name
- **Click to View**: Opens franchise history

---

## Franchise History

**URL**: `/Teams/Franchise/{franchId}`

View complete history of a franchise.

### Components

1. **Franchise Header**
    - Franchise name
    - Years active (first to last)
    - Total seasons count
    - World Series championships count

2. **Season History Table**
    - All seasons for the franchise
    - Year, team name, league
    - Win-loss record
    - Championship indicators
    - Click year to view season details

### Query Parameters

| Parameter | Description | Example   |
|-----------|-------------|-----------|
| `page`    | Page number | `?page=2` |

---

## Team Season

**URL**: `/Teams/{teamId}/{lgId}/{year}`

View details for a specific team season.

### Components

1. **Season Header**
    - Team name and year
    - League and division
    - Win-loss record
    - Win percentage
    - Final standing
    - Championship badges

2. **Team Stats**
    - Runs scored
    - Home runs
    - Team ERA

3. **Managers**
    - Manager name(s)
    - Win-loss record

4. **Batting Roster**
    - Top 25 batters by at-bats
    - Stats: G, AB, H, HR, RBI, AVG
    - HOF badges
    - Click name for player modal

5. **Pitching Roster**
    - Top 25 pitchers by innings
    - Stats: G, W-L, ERA, SO, SV
    - HOF badges
    - Click name for player modal

6. **Season Navigation**
    - Links to other seasons for this franchise

7. **Home Park**
    - Park name linked to the ballpark detail page (`/Parks/{parkKey}`)
    - Resolved through `HomeGames` (primary park = most home games that season)

---

## Ballpark Browser

**URL**: `/Parks`

Browse all 345 ballparks from the Lahman `Parks` table. Tracks issue #86.

### Display

- **Park Table**: Name (linked), former names, location, years active
  (from `HomeGames`), and count of home teams
- **Sorting**: Sorted by park name
- **Empty State**: Shown when filters match nothing

### Query Parameters

| Parameter | Description                        | Example      |
|-----------|------------------------------------|--------------|
| `q`       | Filter by name, former name, city  | `?q=fenway`  |
| `state`   | Filter by state code               | `?state=MA`  |
| `page`    | Page number (30 per page)          | `?page=2`    |

---

## Ballpark Detail

**URL**: `/Parks/{parkKey}` (e.g. `/Parks/BOS07`)

Complete history of one ballpark from `Parks` + `HomeGames`.

### Components

1. **Park Header**
    - Name, location, former names
    - Lifespan (first–last season), season count
    - Home teams, total home games, total and peak attendance

2. **Attendance History Chart**
    - Chart.js line chart of season attendance, summed across tenants

3. **Home Teams Table**
    - Each team tenure: years, seasons, home games
    - Linked to the team's most recent season at the park

4. **Season History Table**
    - Every team-season: games, openings, attendance
    - Linked to team season pages

---

## Negro Leagues Hub

**URL**: `/NegroLeagues`

Browse the seven Negro Leagues (1920–1948) that MLB recognized as major leagues
in December 2020. Tracks issue #90. League scope is defined once, in
`Services/NegroLeagues.cs`; earlier independent Black baseball (lgIDs `IND`,
`EAS`, `WES`, `NAC`) is out of scope and covered narratively on
`/SurvivingRecords`.

### Components

1. **League Cards** — name, lgID, years, club count, team-season count, summary
2. **Transparency framing** — links to `/SurvivingRecords` and `/About#data-scope`
3. **"Before the leagues"** — pre-1920 context with Seamheads link

---

## League Detail

**URL**: `/NegroLeagues/{lgId}` (e.g. `/NegroLeagues/NN2`; case-insensitive)

### Components

1. **League Header** — name, years, seasons, clubs, team-seasons
2. **Seasons Table** — year (links to league season page), team count, pennant
   winner (links to team season page)
3. **Clubs Table** — every club's tenure: years, seasons, W-L record, PCT,
   pennants; sorted by pennants then seasons

---

## League Season

**URL**: `/NegroLeagues/{lgId}/{year}` (e.g. `/NegroLeagues/NN2/1943`)

### Components

1. **Standings** — rank, W-L, PCT, games behind, documented games; pennant
   badge; teams link to team season pages; prev/next season navigation
2. **Leaders** — top 5 in AVG (qualified), HR, ERA (qualified), W via
   `ILeaderboardQueryService` with `League` + `SingleSeason` filters, using the
   season-relative qualification thresholds
3. **Data scope notes** — the standard `_DataScopeNote` plus standings caveat

---

## Managers Browser

**URL**: `/Managers`

Browse all 931 managers with career records aggregated from `Managers`.
Tracks issue #87.

### Query Parameters

| Parameter | Description                     | Example      |
|-----------|---------------------------------|--------------|
| `q`       | Filter by name                  | `?q=mack`    |
| `sort`    | `wins` (default) or `name`      | `?sort=name` |
| `page`    | Page number (50 per page)       | `?page=2`    |

---

## Manager Career

**URL**: `/Managers/{playerId}` (e.g. `/Managers/mackco01`)

### Components

1. **Career Header** — years, seasons, W-L record, PCT, pennants, WS titles;
   HOF badge; "Player Page" link when the manager also played
2. **Manager Awards** — from `AwardsManagers`, with "View Race" links into
   `/Awards?scope=managers` where voting data exists
3. **Season History** — every stint (team linked to team season pages), with
   Pennant/WS/Player-Mgr badges; footnote that team flags may span managers
4. **Split Seasons** — `ManagersHalf` rows (1892, 1981 only)

Cross-links: player pages gain a **Managing** tab (`_PlayerManagingTable`) when
the player managed, with manager awards merged into the Awards card; the team
season managers card links each manager's career page.

**Awards scope**: `/Awards` accepts `scope=players` (default) or
`scope=managers`, switching the winners/voting queries between the
`AwardsPlayers`/`AwardsSharePlayers` and `AwardsManagers`/`AwardsShareManagers`
tables. Filter dropdowns, "View Race" links, and pagination all carry the scope.

---

## Batting Leaders

**URL**: `/Stats/Batting`

View batting statistical leaders with filters.

### Filters

| Filter        | Options                 | Description                  |
|---------------|-------------------------|------------------------------|
| Stat          | HR, H, AVG, R, etc.     | Statistic to rank by         |
| From Year     | Year dropdown           | Start year for range         |
| To Year       | Year dropdown           | End year for range           |
| League        | AL, NL, All             | Filter by league             |
| Qualified     | Toggle (default ON)     | Season-relative qualification for rate stats |
| Min AB        | 0, 100, 500, 1000, 3000 | Explicit minimum at-bats (overrides qualification) |
| Single Season | Checkbox                | Individual seasons vs career |

### Qualification Logic

When the **Qualified** toggle is ON (default), rate-stat leaderboards (AVG, OBP, SLG, OPS) apply a season-relative qualification threshold:

- **Threshold**: Total plate appearances (PA) must equal or exceed **3.1 PA per team-game** across all career stints
- **Sanity floor**: The threshold cannot drop below **100 PA**, preventing small-sample noise from players with incomplete or anomalous team-game data
- **Why**: This season-relative approach allows players from shorter-season eras (Negro Leagues with 60-80 game schedules, 19th century) to qualify naturally without arbitrary career totals that would exclude them
- **Effect**: With qualification enabled, leaders like Ty Cobb, Josh Gibson, Oscar Charleston, Turkey Stearnes, and Cool Papa Bell appear naturally on career batting average leaderboards, alongside modern stars

Turn **Qualified** OFF to see all players including small-sample outliers, or use **Min AB** to set an explicit threshold.

### Display

- **Leaderboard Table**
    - Rank
    - Player name (click for modal)
    - HOF badge
    - Year/Team (single season mode)
    - Statistics: G, AB, H, 2B, 3B, HR, R, BB, AVG, OPS

- **Pagination**: 100 per page

### Sorting

- Most stats: Descending (higher is better)
- Stats are highlighted based on selection

---

## Pitching Leaders

**URL**: `/Stats/Pitching`

View pitching statistical leaders with filters.

### Filters

| Filter        | Options               | Description                  |
|---------------|-----------------------|------------------------------|
| Stat          | W, SO, SV, ERA, etc.  | Statistic to rank by         |
| From Year     | Year dropdown         | Start year for range         |
| To Year       | Year dropdown         | End year for range           |
| League        | AL, NL, All           | Filter by league             |
| Qualified     | Toggle (default ON)   | Season-relative qualification for rate stats |
| Min IP        | 0, 50, 100, 500, 1000 | Explicit minimum innings (overrides qualification) |
| Single Season | Checkbox              | Individual seasons vs career |

### Qualification Logic

When the **Qualified** toggle is ON (default), rate-stat leaderboards (ERA, WHIP, K9, BB9, WPCT) apply a season-relative qualification threshold:

- **Threshold**: Total outs pitched must equal or exceed **3.0 outs per team-game** across all career stints (equivalent to 1 inning per team-game)
- **Sanity floor**: The threshold cannot drop below **90 outs (30 innings)**, preventing small-sample noise from players with incomplete or anomalous team-game data
- **Why**: This season-relative approach allows pitchers from shorter-season eras (Negro Leagues, 19th century) to qualify naturally without arbitrary career totals that would exclude them
- **Effect**: With qualification enabled, the leaderboards surface real career leaders without small-sample outliers dominating the top ranks

Turn **Qualified** OFF to see all pitchers including small-sample outliers, or use **Min IP** to set an explicit threshold.

### Display

- **Leaderboard Table**
    - Rank
    - Player name (click for modal)
    - HOF badge
    - Year/Team (single season mode)
    - Statistics: G, GS, W, L, SV, IP, SO, BB, ERA, WHIP

- **Pagination**: 100 per page

### Sorting

- Most stats: Descending
- ERA, WHIP: Ascending (lower is better)

---

## Hall of Fame

**URL**: `/HallOfFame`

Browse Hall of Fame inductees.

### Filters

| Filter   | Options               | Description              |
|----------|-----------------------|--------------------------|
| Year     | Year dropdown         | Filter by induction year |
| Category | Player, Manager, etc. | Filter by category       |

### Display

- **Inductee Table**
    - Name (click for modal)
    - HOF badge
    - Induction year
    - Category badge
    - Voted by (BBWAA, Committee, etc.)
    - Vote percentage
    - Career years

- **Category Stats**
    - Total players
    - Total managers
    - Total pioneers/executives
    - Total umpires

- **Pagination**: 50 per page

---

## Search

**URL**: `/Search?q={query}`

Global search across players and teams.

### Features

- **Live Search**: Results appear as you type (300ms debounce)
- **Dropdown Results**: Shows in navbar dropdown
- **Player Results**:
    - Player initials
    - Full name
    - HOF badge
    - Career years
    - Click to open modal
- **Team Results**:
    - Team initials
    - Franchise name
    - Active status
    - Click to open franchise page

### Implementation

```html
<input type="search"
       hx-get="/Search"
       hx-trigger="input changed delay:300ms"
       hx-target="#search-results">
```

---

## Loading States

All data-heavy operations show loading indicators:

### Spinning Baseball

```html
<div class="loading-baseball">
    <div class="loading-baseball-icon">&#9918;</div>
    <div class="loading-baseball-text">Loading stats...</div>
</div>
```

Shown during:

- Filter changes on leaderboards
- Pagination
- Search queries

---

## Responsive Behavior

| Breakpoint | Behavior                     |
|------------|------------------------------|
| < 576px    | Single column, smaller fonts |
| 576-768px  | 2-column grids               |
| 768-992px  | 3-column grids               |
| > 992px    | Full layout, 4-column grids  |

All tables are horizontally scrollable on mobile.

---

## Player Comparison

**URL**: `/Compare`

Head-to-head comparison of two players with side-by-side layout.

### Layout

Two equal columns, each an independent player selection region:

- **Left side (Player 1)**: Navy gradient card when selected
- **Right side (Player 2)**: Red gradient card when selected
- Each side has its own search box with htmx typeahead (300ms debounce)

### Flow

1. Two empty cards with search boxes and placeholder icons
2. Search in either side — results appear in that side's dropdown
3. Select a player — that side fills with the player card, other side preserved
4. Select second player — comparison tables appear below both cards
5. "Change Player" button clears one side; "Start Over" resets both

### URL Scheme

`/Compare?player1={id}&player2={id}` — shareable, each side tracked independently.

### Comparison Tables (shown when both selected)

Mirror layout: Player 1 values right-aligned | Stat label centered | Player 2 values left-aligned.

- **Awards & Honors**: All-Star, MVP, Gold Glove, Silver Slugger, Total Awards, HOF status
- **Career Batting**: G, AB, R, H, 2B, 3B, HR, RBI, SB, BB, SO, AVG, OBP, SLG, OPS
- **Career Pitching** (if either player has pitching stats): W-L, G, GS, SV, CG, SHO, IP, SO, BB, ERA, WHIP

Better values highlighted in green (with correct lower-is-better logic for ERA, WHIP, walks, strikeouts).

### Implementation

Pure htmx — no custom JavaScript. Search uses `hx-get`/`hx-target` for typeahead; player selection uses `hx-boost="false"` for full page navigation.

---

## Awards & Voting

**URL**: `/Awards`

Browse award winners and full voting race breakdowns.

### Filters

| Filter | Options              | Description          |
|--------|----------------------|----------------------|
| Award  | MVP, Cy Young, etc.  | Filter by award type |
| Year   | Year dropdown        | Filter by year       |
| League | AL, NL, All          | Filter by league     |

### Display

- **Winners Table**: Player name (click for modal), award badge, year, league, notes
- **"View Race" Button**: Appears when voting data is available for that award/year/league
- **Voting Detail** (when specific award+year+league selected):
    - Full vote breakdown table: rank, player, points won, max points, 1st-place votes, vote share %
    - Visual progress bars for vote share
    - Winner row highlighted in green with "Winner" badge
    - HOF badges throughout

### Pagination

50 per page with standard pagination component.

---

## Postseason

**URL**: `/Postseason`

Browse playoff series results from all eras.

### Filters

| Filter | Options                              | Description      |
|--------|--------------------------------------|------------------|
| Year   | Year dropdown (all postseason years) | Filter by year   |
| Round  | WS, ALCS, NLCS, ALDS, NLDS, ALWC, NLWC | Filter by round |

### Display

- **Series Table**: Year, round name, winner (with league), series result (e.g. 4-2), loser (with league)
- World Series rows highlighted with gold background
- Click a year to filter to all series that postseason
- Round names mapped from codes (e.g. "WS" → "World Series")

### Pagination

50 per page with standard pagination component.

---

## Salary Explorer

**URL**: `/Salaries`

Explore player salary data from 1985 onward.

### Filters

| Filter | Options       | Description                            |
|--------|---------------|----------------------------------------|
| Year   | Year dropdown | Filter by year (1985+)                 |
| Team   | Team dropdown | Filter by team (updates based on year) |

### Display

- **Team Payroll Banner** (when filtering by team+year): Shows total payroll in styled header
- **Salary Table**: Rank, player (click for modal), year, team, salary (formatted as currency)
- HOF badges on Hall of Famers

### Pagination

50 per page with standard pagination component.

---

## API Documentation

**URL**: `/ApiDocs`

Interactive reference page for the REST API.

### Content

- Base URL and pagination envelope documentation
- All 9 endpoint groups with parameter tables and curl examples
- Response codes reference
- Data notes (ID conventions, computed stats, date ranges)
- Links to Scalar API explorer (`/scalar/v1`) and OpenAPI spec (`/openapi/v1.json`)

---

## REST API

**Base URL**: `/api`

JSON API with 30+ endpoints for programmatic access to all baseball data. No authentication required. All endpoints are GET requests (read-only database).

### Endpoint Groups

| Group           | Base Path          | Endpoints | Description                              |
|-----------------|--------------------|-----------|------------------------------------------|
| Players         | `/api/players`     | 8         | List, detail, batting/pitching/fielding, awards, postseason |
| Teams           | `/api/teams`       | 3         | Franchises, franchise detail, team season |
| Leaders         | `/api/leaders`     | 2         | Batting and pitching leaderboards        |
| Hall of Fame    | `/api/hall-of-fame`| 2         | Inductees list, voting history           |
| Search          | `/api/search`      | 1         | Cross-entity search                      |
| Salaries        | `/api/salaries`    | 3         | Player history, team payrolls, leaders   |
| Parks           | `/api/parks`       | 2         | Park list, detail with attendance        |
| Postseason      | `/api/postseason`  | 2         | Series results by year/round             |
| Awards          | `/api/awards`      | 2         | Winners, full voting breakdowns          |

### Pagination

List endpoints return a standard envelope:

```json
{
  "data": [ ... ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1234,
  "totalPages": 50
}
```

### Documentation

- Interactive explorer: `/scalar/v1` (development mode)
- OpenAPI spec: `/openapi/v1.json`
- In-app reference: `/ApiDocs`
