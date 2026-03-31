# API Reference

This document describes the page models, view models, data flow patterns,
and the REST API used in the application.

## Page Models

### Index Page

**File**: `Pages/Index.cshtml.cs`

| Property           | Type | Description                |
|--------------------|------|----------------------------|
| TotalPlayers       | int  | Count of all players       |
| TotalFranchises    | int  | Count of franchises        |
| HallOfFamers       | int  | Count of HOF inductees     |
| TotalSeasons       | int  | Number of seasons covered  |
| FirstYear          | int  | Earliest year in database  |
| LastYear           | int  | Latest year in database    |
| CareerHrLeaders    | List | Top 10 career HR leaders   |
| CareerWinsLeaders  | List | Top 10 career wins leaders |
| RecentHofInductees | List | Recent HOF inductees       |

---

### Players/Index

**File**: `Pages/Players/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| letter | string | "A" | Last name initial filter |
| page | int | 1 | Page number |

**Response**: `PlayerListViewModel`

---

### Players/Modal

**File**: `Pages/Players/Modal.cshtml.cs`

**Route**: `/Players/Modal/{id}`

**Response**: Partial view `_PlayerModal` with `PlayerDetailViewModel`

---

### Teams/Index

**File**: `Pages/Teams/Index.cshtml.cs`

**Response**: List of `FranchiseSummary`

---

### Teams/Franchise

**File**: `Pages/Teams/Franchise.cshtml.cs`

**Route**: `/Teams/Franchise/{id}`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number |

**Response**: `FranchiseViewModel`

---

### Teams/Season

**File**: `Pages/Teams/Season.cshtml.cs`

**Route**: `/Teams/{teamId}/{lgId}/{year:int}`

**Response**: `TeamSeasonViewModel`

---

### Stats/Batting

**File**: `Pages/Stats/Batting.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| stat | string | "hr" | Statistic to rank by |
| fromYear | int? | null | Start year filter |
| toYear | int? | null | End year filter |
| league | string | null | League filter (AL/NL) |
| minAb | int | 0 | Minimum at-bats |
| singleSeason | bool | false | Single season vs career |
| page | int | 1 | Page number |

**Response**: `LeaderboardViewModel` with `BattingLeaders`

---

### Stats/Pitching

**File**: `Pages/Stats/Pitching.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| stat | string | "w" | Statistic to rank by |
| fromYear | int? | null | Start year filter |
| toYear | int? | null | End year filter |
| league | string | null | League filter |
| minIp | int | 0 | Minimum innings pitched |
| singleSeason | bool | false | Single season vs career |
| page | int | 1 | Page number |

**Response**: `LeaderboardViewModel` with `PitchingLeaders`

---

### HallOfFame/Index

**File**: `Pages/HallOfFame/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| year | int? | null | Induction year filter |
| category | string | null | Category filter |
| page | int | 1 | Page number |

**Response**: `HallOfFameViewModel`

---

### Search

**File**: `Pages/Search.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| q | string | Search query |

**Response**: Partial view `_SearchResults` with `SearchViewModel`

---

## View Models

### PlayerListViewModel

```csharp
public class PlayerListViewModel
{
    public List<PlayerSummary> Players { get; set; }
    public List<char> AvailableLetters { get; set; }
    public string CurrentLetter { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalPlayers { get; set; }
    public int PageSize { get; set; }
}
```

### PlayerSummary

```csharp
public class PlayerSummary
{
    public string PlayerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName { get; set; }
    public string? BirthYear { get; set; }
    public string? DebutYear { get; set; }
    public string? FinalYear { get; set; }
    public bool IsInHallOfFame { get; set; }
    public int? TotalGames { get; set; }
    public int? TotalHits { get; set; }
    public int? TotalHomeRuns { get; set; }
    public string? LastTeamId { get; set; }
}
```

### PlayerDetailViewModel

```csharp
public class PlayerDetailViewModel
{
    public string PlayerId { get; set; }
    public string FullName { get; set; }
    public string? GivenName { get; set; }
    public string? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? DeathDate { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public string? Bats { get; set; }
    public string? Throws { get; set; }
    public string? Debut { get; set; }
    public string? FinalGame { get; set; }
    public int? CareerYears { get; set; }
    public bool IsInHallOfFame { get; set; }
    public short? HofInductionYear { get; set; }

    public CareerBattingStats? BattingStats { get; set; }
    public CareerPitchingStats? PitchingStats { get; set; }
    public List<SeasonBattingRecord> BattingSeasons { get; set; }
    public List<TeamRecord> Teams { get; set; }
    public List<AwardRecord> Awards { get; set; }
    public List<AllStarRecord> AllStarAppearances { get; set; }
}
```

### CareerBattingStats

```csharp
public class CareerBattingStats
{
    public int Games { get; set; }
    public int AtBats { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int Rbi { get; set; }
    public int StolenBases { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }

    // Calculated properties
    public string FormattedAvg => AtBats > 0
        ? ((double)Hits / AtBats).ToString(".000")
        : ".000";
    public string FormattedOps => CalculateOps();
}
```

### LeaderboardViewModel

```csharp
public class LeaderboardViewModel
{
    public LeaderboardType Type { get; set; }
    public string StatColumn { get; set; }
    public string StatLabel { get; set; }
    public string Title { get; set; }

    // Filters
    public int? FromYear { get; set; }
    public int? ToYear { get; set; }
    public string? League { get; set; }
    public int MinimumAtBats { get; set; }
    public int MinimumInningsPitched { get; set; }
    public bool SingleSeason { get; set; }

    // Pagination
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalEntries { get; set; }
    public int PageSize { get; set; } = 100;

    // Data
    public List<int> AvailableYears { get; set; }
    public List<string> AvailableLeagues { get; set; }
    public Dictionary<string, string> AvailableStats { get; set; }
    public List<BattingLeaderEntry> BattingLeaders { get; set; }
    public List<PitchingLeaderEntry> PitchingLeaders { get; set; }
}
```

### TeamSeasonViewModel

```csharp
public class TeamSeasonViewModel
{
    public string TeamId { get; set; }
    public string? TeamName { get; set; }
    public string LgId { get; set; }
    public short Year { get; set; }
    public string? FranchiseId { get; set; }
    public string? FranchiseName { get; set; }
    public string? DivId { get; set; }

    public short Wins { get; set; }
    public short Losses { get; set; }
    public byte? Rank { get; set; }
    public bool WonDivision { get; set; }
    public bool WonPennant { get; set; }
    public bool WonWorldSeries { get; set; }

    public string? ParkName { get; set; }
    public int? Attendance { get; set; }

    public TeamBattingStats? Batting { get; set; }
    public TeamPitchingStats? Pitching { get; set; }
    public List<RosterPlayer> Batters { get; set; }
    public List<RosterPlayer> Pitchers { get; set; }
    public List<ManagerInfo> Managers { get; set; }
    public List<short> AvailableYears { get; set; }

    // Calculated properties
    public string Record => $"{Wins}-{Losses}";
    public string FormattedWinPct => (Wins + Losses) > 0
        ? ((double)Wins / (Wins + Losses)).ToString(".000")
        : ".000";
}
```

### HallOfFameViewModel

```csharp
public class HallOfFameViewModel
{
    public List<HallOfFameInductee> Inductees { get; set; }
    public List<int> AvailableYears { get; set; }
    public int? SelectedYear { get; set; }
    public string? SelectedCategory { get; set; }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalInductees { get; set; }
    public int PageSize { get; set; } = 50;

    // Category counts
    public int TotalPlayers { get; set; }
    public int TotalManagers { get; set; }
    public int TotalPioneers { get; set; }
    public int TotalUmpires { get; set; }
}
```

### SearchViewModel

```csharp
public class SearchViewModel
{
    public string Query { get; set; }
    public List<SearchResult> Players { get; set; }
    public List<SearchResult> Teams { get; set; }

    public bool HasResults => Players.Any() || Teams.Any();
}

public class SearchResult
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Subtitle { get; set; }
    public string Initials { get; set; }
    public bool IsInHallOfFame { get; set; }
    public string? TeamId { get; set; }
}
```

---

## Static Data

### LeaderboardStats

```csharp
public static class LeaderboardStats
{
    public static readonly Dictionary<string, string> BattingStats = new()
    {
        ["hr"] = "Home Runs",
        ["h"] = "Hits",
        ["avg"] = "Batting Average",
        ["r"] = "Runs",
        ["rbi"] = "RBI",
        ["sb"] = "Stolen Bases",
        ["bb"] = "Walks",
        ["ops"] = "OPS",
        ["2b"] = "Doubles",
        ["3b"] = "Triples"
    };

    public static readonly Dictionary<string, string> PitchingStats = new()
    {
        ["w"] = "Wins",
        ["so"] = "Strikeouts",
        ["sv"] = "Saves",
        ["era"] = "ERA",
        ["whip"] = "WHIP",
        ["g"] = "Games",
        ["gs"] = "Games Started",
        ["cg"] = "Complete Games",
        ["sho"] = "Shutouts",
        ["ip"] = "Innings Pitched"
    };
}
```

---

## Pagination Model

```csharp
public class PaginationModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public string BaseUrl { get; set; }
    public string Target { get; set; }
    public Dictionary<string, string> QueryParams { get; set; }
}
```

Usage in partial:

```html
@model PaginationModel

@if (Model.TotalPages > 1)
{
    <nav>
        <ul class="pagination pagination-baseball">
            @for (var pageNum = 1; pageNum <= Model.TotalPages; pageNum++)
            {
                <li class="page-item @(pageNum == Model.CurrentPage ? "active" : "")">
                    <a class="page-link"
                       hx-get="@BuildUrl(pageNum)"
                       hx-target="@Model.Target"
                       hx-push-url="true">
                        @pageNum
                    </a>
                </li>
            }
        </ul>
    </nav>
}
```

---

## HTMX Response Headers

The application doesn't use custom HTMX response headers, but the following
could be added for advanced scenarios:

| Header      | Purpose                         |
|-------------|---------------------------------|
| HX-Push-Url | Override URL to push to history |
| HX-Redirect | Redirect the browser            |
| HX-Refresh  | Full page refresh               |
| HX-Retarget | Change the target element       |
| HX-Trigger  | Trigger client-side events      |

---

## Additional Page Models

### Awards/Index

**File**: `Pages/Awards/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| award | string | null | Filter by award type (MVP, Cy Young, etc.) |
| year | int? | null | Filter by year |
| league | string | null | Filter by league (AL, NL) |
| page | int | 1 | Page number |

**Response**: `AwardVotingViewModel` with winners list and optional voting detail

When a specific award+year+league is selected that has voting data, the response
includes an `AwardRaceDetail` with full vote breakdowns.

---

### Postseason/Index

**File**: `Pages/Postseason/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| year | int? | null | Filter by year |
| round | string | null | Filter by round (WS, ALCS, NLCS, etc.) |
| page | int | 1 | Page number |

**Response**: `PostseasonViewModel`

---

### Salaries/Index

**File**: `Pages/Salaries/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| year | int? | null | Filter by year |
| team | string | null | Filter by team ID |
| page | int | 1 | Page number |

**Response**: `SalaryViewModel` — includes team payroll total when team+year selected

---

### Compare/Index

**File**: `Pages/Compare/Index.cshtml.cs`

**Query Parameters**:
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| player1 | string | null | Player 1 ID |
| player2 | string | null | Player 2 ID |

**Handlers**:
- `OnGetAsync` — loads comparison page with player data
- `OnGetSearchAsync` — player search typeahead (params: `q`, `side`)

**Response**: `CompareViewModel` with `Player1` and `Player2`

---

## REST API (Minimal APIs)

The application exposes a JSON REST API under `/api` for programmatic access.
All endpoints are GET requests (read-only database). No authentication required.

### Configuration

- OpenAPI spec: `/openapi/v1.json`
- Interactive docs: `/scalar/v1` (development mode only)
- In-app reference: `/ApiDocs`

Endpoints are registered via `ApiEndpointExtensions.MapApiEndpoints()` in
`Program.cs`. Each domain has a static endpoint class in `Api/Endpoints/`.

### Pagination Envelope

List endpoints return:
```json
{
  "data": [ ... ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1234,
  "totalPages": 50
}
```

### Players — `/api/players`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/players` | List players by last name letter (`letter`, `page`, `pageSize`) |
| GET | `/api/players/{playerId}` | Full player detail with career stats and teams |
| GET | `/api/players/{playerId}/batting` | Season-by-season batting |
| GET | `/api/players/{playerId}/pitching` | Season-by-season pitching |
| GET | `/api/players/{playerId}/fielding` | Season-by-season fielding by position |
| GET | `/api/players/{playerId}/awards` | Player awards |
| GET | `/api/players/{playerId}/postseason/batting` | Postseason batting stats |
| GET | `/api/players/{playerId}/postseason/pitching` | Postseason pitching stats |

### Teams — `/api/teams`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/teams/franchises` | List franchises (`league`, `activeOnly`) |
| GET | `/api/teams/franchises/{franchiseId}` | Franchise detail with all seasons |
| GET | `/api/teams/seasons/{teamId}/{lgId}/{year}` | Team season with full roster |

### Leaders — `/api/leaders`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/leaders/batting` | Batting leaderboard (`stat`, `fromYear`, `toYear`, `league`, `minAb`, `singleSeason`, `page`, `pageSize`) |
| GET | `/api/leaders/pitching` | Pitching leaderboard (`stat`, `fromYear`, `toYear`, `league`, `minIp`, `singleSeason`, `page`, `pageSize`) |

### Hall of Fame — `/api/hall-of-fame`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/hall-of-fame` | List inductees (`year`, `category`, `page`, `pageSize`) |
| GET | `/api/hall-of-fame/{playerId}/voting` | Full voting history for a player |

### Search — `/api/search`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/search` | Search players and franchises (`q`, `limit`) |

### Salaries — `/api/salaries`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/salaries/players/{playerId}` | Player salary history with career total |
| GET | `/api/salaries/teams/{teamId}/{year}` | Team payroll for a season |
| GET | `/api/salaries/leaders` | Highest-paid players (`year`, `page`, `pageSize`) |

### Parks — `/api/parks`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/parks` | List ballparks (`state`, `page`, `pageSize`) |
| GET | `/api/parks/{parkKey}` | Park detail with season attendance history |

### Postseason — `/api/postseason`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/postseason/series` | Postseason series results (`year`, `round`, `page`, `pageSize`) |
| GET | `/api/postseason/series/{year}` | All series in a given year |

### Awards — `/api/awards`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/awards/winners` | Award winners (`awardId`, `year`, `lgId`, `page`, `pageSize`) |
| GET | `/api/awards/voting/{awardId}/{year}/{lgId}` | Full voting breakdown for an award race |

### DTOs

API DTOs are record types in `Api/Dtos/`, separate from Razor Page ViewModels:

| File | Types |
|------|-------|
| `PagedResponse.cs` | `PagedResponse<T>` — generic pagination wrapper |
| `PlayerDtos.cs` | `PlayerListItem`, `PlayerDetail`, `CareerBattingDto`, `CareerPitchingDto`, `SeasonBattingDto`, `SeasonPitchingDto`, `SeasonFieldingDto`, `PlayerAwardDto`, `PlayerTeamDto`, `PostseasonBattingDto`, `PostseasonPitchingDto` |
| `TeamDtos.cs` | `FranchiseListItem`, `FranchiseDetail`, `FranchiseSeasonItem`, `TeamSeasonDetail`, `ApiTeamBattingDto`, `ApiTeamPitchingDto`, `RosterBatterDto`, `RosterPitcherDto`, `ApiManagerDto` |
| `LeaderDtos.cs` | `BattingLeaderDto`, `PitchingLeaderDto` |
| `HallOfFameDtos.cs` | `HallOfFameInducteeDto`, `HallOfFameVotingHistoryDto`, `VotingYearDto` |
| `SearchDtos.cs` | `SearchResponse`, `PlayerSearchResult`, `FranchiseSearchResult` |
| `SalaryDtos.cs` | `SalaryDto`, `PlayerSalaryHistoryDto`, `SalarySeasonDto`, `TeamSalaryDto` |
| `ParkDtos.cs` | `ParkDto`, `ParkDetailDto`, `ParkSeasonDto` |
| `PostseasonDtos.cs` | `PostseasonSeriesDto` |
| `AwardDtos.cs` | `AwardWinnerDto`, `AwardVotingDto`, `AwardVoteDto` |
