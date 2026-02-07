# Features Documentation

This document provides detailed information about each feature of the Baseball
History application.

## Feature Overview

| Feature           | Description                           | URL                             |
|-------------------|---------------------------------------|---------------------------------|
| Home Dashboard    | Overview with quick stats and leaders | `/`                             |
| Player Browser    | Alphabetical player listing           | `/Players`                      |
| Player Modal      | Detailed player view                  | `/Players/Modal/{id}`           |
| Team Browser      | Franchise listing                     | `/Teams`                        |
| Franchise History | Team history by franchise             | `/Teams/Franchise/{id}`         |
| Team Season       | Single season team view               | `/Teams/{teamId}/{lgId}/{year}` |
| Batting Leaders   | Batting statistical leaders           | `/Stats/Batting`                |
| Pitching Leaders  | Pitching statistical leaders          | `/Stats/Pitching`               |
| Hall of Fame      | HOF inductee browser                  | `/HallOfFame`                   |
| Search            | Global search                         | `/Search`                       |

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
    - All-Star appearance count
    - Awards list (MVP, Cy Young, etc.)

4. **Career Batting** (if applicable)
    - Games, AVG, Hits, HR, RBI, OPS

5. **Career Pitching** (if applicable)
    - W-L, ERA, Games, SO, SV, WHIP

6. **Season by Season**
    - Year-by-year batting statistics table

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
| Min AB        | 0, 100, 500, 1000, 3000 | Minimum at-bats              |
| Single Season | Checkbox                | Individual seasons vs career |

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
| Min IP        | 0, 50, 100, 500, 1000 | Minimum innings              |
| Single Season | Checkbox              | Individual seasons vs career |

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
