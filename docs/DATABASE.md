# Database Design

This document describes the database schema and Entity Framework Core
configuration for the Baseball History application.

## Overview

The application uses
the [Lahman Baseball Database](https://www.seanlahman.com/baseball-archive/statistics/),
a comprehensive historical database of Major League Baseball statistics dating
from 1871 to the present.

## Database Technology

- **Database Engine**: PostgreSQL
- **ORM**: Entity Framework Core 10.0
- **Connection Key**: `ConnectionStrings:Lahman`

## Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────┐       ┌─────────────────┐
│   People    │       │   Batting   │       │     Teams       │
├─────────────┤       ├─────────────┤       ├─────────────────┤
│ playerID PK │◄──────│ playerID FK │       │ teamID      PK  │
│ nameFirst   │       │ yearID   PK │──────►│ lgID        PK  │
│ nameLast    │       │ stint    PK │       │ yearID      PK  │
│ debut       │       │ teamID   FK │──────►│ franchID    FK  │
│ finalGame   │       │ lgID     FK │       │ name            │
│ birthYear   │       │ G, AB, H... │       │ W, L, R...      │
└─────────────┘       └─────────────┘       └─────────────────┘
      │                                              │
      │                                              │
      ▼                                              ▼
┌─────────────┐       ┌─────────────┐       ┌─────────────────┐
│  Pitching   │       │  Managers   │       │ TeamsFranchises │
├─────────────┤       ├─────────────┤       ├─────────────────┤
│ playerID FK │       │ playerID FK │       │ franchID    PK  │
│ yearID   PK │       │ yearID   PK │       │ franchName      │
│ stint    PK │       │ teamID   FK │       │ active          │
│ teamID   FK │       │ lgID     FK │       └─────────────────┘
│ W, L, SO... │       │ inseason PK │
└─────────────┘       └─────────────┘

┌─────────────┐       ┌─────────────┐       ┌─────────────────┐
│ HallOfFame  │       │AwardsPlayers│       │  AllstarFull    │
├─────────────┤       ├─────────────┤       ├─────────────────┤
│ playerID FK │       │ playerID FK │       │ playerID    FK  │
│ yearid   PK │       │ yearID   PK │       │ yearID      PK  │
│ votedBy  PK │       │ awardID  PK │       │ lgID        PK  │
│ inducted    │       │ lgID     PK │       │ teamID      PK  │
│ category    │       │ notes       │       │ gameID      PK  │
└─────────────┘       └─────────────┘       └─────────────────┘
```

## Core Entities

### People (Players)

The central entity containing biographical information for all players,
managers, and umpires.

```csharp
public class People
{
    public string PlayerId { get; set; }      // Primary key (e.g., "ruthba01")
    public string? NameFirst { get; set; }
    public string? NameLast { get; set; }
    public string? NameGiven { get; set; }
    public DateOnly? Debut { get; set; }      // First MLB game
    public DateOnly? FinalGame { get; set; }  // Last MLB game
    public string? BirthYear { get; set; }
    public string? BirthCity { get; set; }
    public string? Bats { get; set; }         // L/R/B
    public string? Throws { get; set; }       // L/R

    // Navigation properties
    public virtual ICollection<Batting> Battings { get; set; }
    public virtual ICollection<Pitching> Pitchings { get; set; }
    public virtual ICollection<HallOfFame> HallOfFames { get; set; }
    // ... many more
}
```

### Batting

Season-by-season batting statistics for each player.

```csharp
public class Batting
{
    // Composite primary key
    public string PlayerId { get; set; }
    public short YearId { get; set; }
    public byte Stint { get; set; }           // Multiple teams in same year
    public string TeamId { get; set; }
    public string LgId { get; set; }

    // Statistics
    public short? G { get; set; }             // Games
    public short? Ab { get; set; }            // At Bats
    public short? R { get; set; }             // Runs
    public short? H { get; set; }             // Hits
    public short? _2b { get; set; }           // Doubles
    public short? _3b { get; set; }           // Triples
    public short? Hr { get; set; }            // Home Runs
    public string? Rbi { get; set; }          // RBI (string in source)
    public string? Sb { get; set; }           // Stolen Bases
    public short? Bb { get; set; }            // Walks

    // Navigation
    public virtual People Player { get; set; }
    public virtual Teams Team { get; set; }
}
```

### Pitching

Season-by-season pitching statistics.

```csharp
public class Pitching
{
    public string PlayerId { get; set; }
    public short YearId { get; set; }
    public byte Stint { get; set; }
    public string? TeamId { get; set; }
    public string? LgId { get; set; }

    public short? W { get; set; }             // Wins
    public short? L { get; set; }             // Losses
    public short? G { get; set; }             // Games
    public short? Gs { get; set; }            // Games Started
    public short? Sv { get; set; }            // Saves
    public short? Ipouts { get; set; }        // Outs recorded (IP * 3)
    public short? H { get; set; }             // Hits allowed
    public short? Er { get; set; }            // Earned Runs
    public string? Hr { get; set; }           // Home Runs (string)
    public short? Bb { get; set; }            // Walks
    public short? So { get; set; }            // Strikeouts

    public virtual People Player { get; set; }
    public virtual Teams Team { get; set; }
}
```

### Teams

Season records for each team.

```csharp
public class Teams
{
    public string TeamId { get; set; }        // e.g., "NYA"
    public string LgId { get; set; }          // e.g., "AL"
    public short YearId { get; set; }
    public string? FranchId { get; set; }     // Franchise ID

    public string? Name { get; set; }         // Full team name
    public short? W { get; set; }             // Wins
    public short? L { get; set; }             // Losses
    public byte? Rank { get; set; }           // Final standing
    public string? DivWin { get; set; }       // Division winner (Y/N)
    public string? Wswin { get; set; }        // World Series winner

    public virtual TeamsFranchises Franchise { get; set; }
    public virtual ICollection<Batting> Battings { get; set; }
    public virtual ICollection<Pitching> Pitchings { get; set; }
}
```

## Entity Framework Configuration

### DbContext Setup

```csharp
public class BaseballDbContext : DbContext
{
    public DbSet<People> People { get; set; }
    public DbSet<Batting> Batting { get; set; }
    public DbSet<Pitching> Pitching { get; set; }
    public DbSet<Teams> Teams { get; set; }
    public DbSet<TeamsFranchises> TeamsFranchises { get; set; }
    public DbSet<HallOfFame> HallOfFame { get; set; }
    // ... 20+ more DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
    }
}
```

### Composite Keys

Many entities use composite primary keys:

```csharp
modelBuilder.Entity<Batting>(entity =>
{
    entity.HasKey(e => new { e.PlayerId, e.YearId, e.Stint, e.TeamId, e.LgId });
});

modelBuilder.Entity<Teams>(entity =>
{
    entity.HasKey(e => new { e.TeamId, e.LgId, e.YearId });
});
```

### Relationships

Foreign key relationships are configured for navigation:

```csharp
// Batting -> People
modelBuilder.Entity<Batting>()
    .HasOne(b => b.Player)
    .WithMany(p => p.Battings)
    .HasForeignKey(b => b.PlayerId)
    .HasPrincipalKey(p => p.PlayerId);

// Batting -> Teams
modelBuilder.Entity<Batting>()
    .HasOne(b => b.Team)
    .WithMany(t => t.Battings)
    .HasForeignKey(b => new { b.TeamId, b.LgId, b.YearId })
    .HasPrincipalKey(t => new { t.TeamId, t.LgId, t.YearId });

// Teams -> TeamsFranchises
modelBuilder.Entity<Teams>()
    .HasOne(t => t.Franchise)
    .WithMany(f => f.Teams)
    .HasForeignKey(t => t.FranchId)
    .HasPrincipalKey(f => f.FranchId);
```

### Value Converters

Custom converters handle data type mismatches between PostgreSQL column types
and the app's existing model surface:

```csharp
// DateOnly converter for varchar-backed dates
var dateOnlyConverter = new ValueConverter<DateOnly?, string?>(
    v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null,
    v => string.IsNullOrWhiteSpace(v) ? null : DateOnly.Parse(v));

entity.Property(e => e.Debut)
    .HasConversion(dateOnlyConverter);

// Legacy string properties backed by numeric PostgreSQL columns
// use value converters so page/view-model formatting code stays stable.
```

## Data Type Considerations

### Legacy String Fields Requiring Parsing

Some application-facing properties intentionally remain strings even though the
PostgreSQL database stores them in numeric columns. EF Core converters bridge
that gap so existing formatting code still works.

| Entity     | Field          | Stored As | Parse Method     |
|------------|----------------|-----------|------------------|
| Batting    | Rbi            | string    | `int.TryParse()` |
| Batting    | Sb, Cs, So     | string    | `int.TryParse()` |
| Pitching   | Hr             | string    | `int.TryParse()` |
| HallOfFame | Votes, Ballots | string    | `int.TryParse()` |

**Important**: Always parse these fields in memory after fetching from the
database, not within LINQ queries:

```csharp
// WRONG - won't translate to SQL
var sum = await _context.Batting
    .SumAsync(b => Convert.ToInt32(b.Rbi));

// CORRECT - fetch first, parse in memory
var data = await _context.Batting.ToListAsync();
var sum = data.Sum(b => int.TryParse(b.Rbi, out var rbi) ? rbi : 0);
```

## Common Queries

### Player with Career Stats

```csharp
var battingData = await _context.Batting
    .Where(b => b.PlayerId == id)
    .ToListAsync();

var careerStats = new CareerBattingStats
{
    Games = battingData.Sum(b => b.G ?? 0),
    AtBats = battingData.Sum(b => b.Ab ?? 0),
    Hits = battingData.Sum(b => b.H ?? 0),
    HomeRuns = battingData.Sum(b => b.Hr ?? 0),
    Rbi = battingData.Sum(b => int.TryParse(b.Rbi, out var r) ? r : 0)
};
```

### Team Season with Roster

```csharp
var team = await _context.Teams
    .Include(t => t.Franchise)
    .FirstOrDefaultAsync(t => t.TeamId == teamId
        && t.LgId == lgId
        && t.YearId == year);

var batters = await _context.Batting
    .Include(b => b.Player)
    .Where(b => b.TeamId == teamId && b.LgId == lgId && b.YearId == year)
    .OrderByDescending(b => b.Ab ?? 0)
    .ToListAsync();
```

### Leaderboard with Filters

```csharp
var query = _context.Batting
    .Include(b => b.Player)
    .AsQueryable();

if (fromYear.HasValue)
    query = query.Where(b => b.YearId >= fromYear.Value);

if (!string.IsNullOrEmpty(league))
    query = query.Where(b => b.LgId == league);

var seasonData = await query
    .Where(b => b.Ab >= minAb)
    .Select(b => new { ... })
    .ToListAsync();
```

### Hall of Fame Check

```csharp
var hofPlayerIds = await _context.HallOfFame
    .Where(h => h.Inducted == "Y")
    .Select(h => h.PlayerId)
    .Distinct()
    .ToHashSetAsync();

// Use in display
IsInHallOfFame = hofPlayerIds.Contains(playerId)
```

## Database Tables Reference

| Table           | Description              | Records (approx) |
|-----------------|--------------------------|------------------|
| People          | Player biographical data | 20,000+          |
| Batting         | Season batting stats     | 110,000+         |
| Pitching        | Season pitching stats    | 50,000+          |
| Fielding        | Season fielding stats    | 140,000+         |
| Teams           | Team season records      | 2,900+           |
| TeamsFranchises | Franchise information    | 120+             |
| HallOfFame      | HOF voting records       | 4,500+           |
| AllstarFull     | All-Star appearances     | 5,500+           |
| AwardsPlayers   | Player awards            | 6,500+           |
| Managers        | Manager records          | 3,500+           |
| Salaries        | Player salaries          | 26,000+          |
| Appearances     | Games by position        | 110,000+         |
