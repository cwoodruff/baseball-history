# Foreign Key Relationships Plan for Lahman Baseball Database

This document outlines the primary keys and proposed foreign key relationships
for the lahman.db SQLite database.

## Primary Keys Summary

| Table               | Primary Key                                     |
|---------------------|-------------------------------------------------|
| People              | `playerID`                                      |
| Teams               | `(teamID, lgID, yearID)`                        |
| TeamsFranchises     | `franchID`                                      |
| Schools             | `schoolID`                                      |
| Parks               | `ID` (with UNIQUE on `parkkey`)                 |
| Batting             | `(playerID, yearID, stint, teamID, lgID)`       |
| BattingPost         | `(yearID, round, playerID, teamID, lgID)`       |
| Pitching            | `(playerID, yearID, stint)`                     |
| PitchingPost        | `(playerID, yearID, round)`                     |
| Fielding            | `(playerID, yearID, stint, teamID, lgID, POS)`  |
| FieldingOF          | `(playerID, yearID, stint)`                     |
| FieldingOFsplit     | `(playerID, yearID, stint, teamID, lgID, POS)`  |
| FieldingPost        | `(playerID, yearID, teamID, lgID, round, POS)`  |
| Appearances         | `(playerID, lgID, teamID, yearID)`              |
| Salaries            | `(playerID, teamID, lgID, yearID)`              |
| Managers            | `(playerID, yearID, teamID, lgID, inseason)`    |
| ManagersHalf        | `(playerID, yearID, teamID, lgID, half)`        |
| AllstarFull         | `(playerID, yearID, lgID, teamID, gameID)`      |
| HallOfFame          | `(playerID, yearid, votedBy)`                   |
| AwardsPlayers       | `(playerID, yearID, awardID, lgID, tie, notes)` |
| AwardsManagers      | `(playerID, awardID, yearID, lgID, tie)`        |
| AwardsSharePlayers  | `(playerID, lgID, yearID, awardID)`             |
| AwardsShareManagers | `(playerID, lgID, yearID, awardID)`             |
| CollegePlaying      | `(playerID, schoolID, yearID)`                  |
| TeamsHalf           | `(teamID, lgID, yearID, Half)`                  |
| SeriesPost          | `(teamIDwinner, lgIDwinner, yearID, round)`     |
| HomeGames           | `(yearkey, leaguekey, teamkey, parkkey)`        |

## Foreign Key Relationships

### Category 1: Player References (playerID → People.playerID)

All tables with `playerID` column reference the `People` table. Data integrity
verified: **0 orphan records**.

```sql
-- Batting → People
ALTER TABLE Batting ADD CONSTRAINT FK_Batting_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- BattingPost → People
ALTER TABLE BattingPost ADD CONSTRAINT FK_BattingPost_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- Pitching → People
ALTER TABLE Pitching ADD CONSTRAINT FK_Pitching_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- PitchingPost → People
ALTER TABLE PitchingPost ADD CONSTRAINT FK_PitchingPost_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- Fielding → People
ALTER TABLE Fielding ADD CONSTRAINT FK_Fielding_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- FieldingOF → People
ALTER TABLE FieldingOF ADD CONSTRAINT FK_FieldingOF_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- FieldingOFsplit → People
ALTER TABLE FieldingOFsplit ADD CONSTRAINT FK_FieldingOFsplit_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- FieldingPost → People
ALTER TABLE FieldingPost ADD CONSTRAINT FK_FieldingPost_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- Appearances → People
ALTER TABLE Appearances ADD CONSTRAINT FK_Appearances_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- Salaries → People
ALTER TABLE Salaries ADD CONSTRAINT FK_Salaries_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- Managers → People
ALTER TABLE Managers ADD CONSTRAINT FK_Managers_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- ManagersHalf → People
ALTER TABLE ManagersHalf ADD CONSTRAINT FK_ManagersHalf_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- AllstarFull → People
ALTER TABLE AllstarFull ADD CONSTRAINT FK_AllstarFull_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- HallOfFame → People
ALTER TABLE HallOfFame ADD CONSTRAINT FK_HallOfFame_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- AwardsPlayers → People
ALTER TABLE AwardsPlayers ADD CONSTRAINT FK_AwardsPlayers_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- AwardsManagers → People
ALTER TABLE AwardsManagers ADD CONSTRAINT FK_AwardsManagers_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- AwardsSharePlayers → People
ALTER TABLE AwardsSharePlayers ADD CONSTRAINT FK_AwardsSharePlayers_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- AwardsShareManagers → People
ALTER TABLE AwardsShareManagers ADD CONSTRAINT FK_AwardsShareManagers_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);

-- CollegePlaying → People
ALTER TABLE CollegePlaying ADD CONSTRAINT FK_CollegePlaying_People
    FOREIGN KEY (playerID) REFERENCES People(playerID);
```

### Category 2: Team References (teamID, lgID, yearID → Teams)

Tables referencing Teams with the composite key. Data integrity verified: **0
orphan records** (except AllstarFull - see notes).

```sql
-- Batting → Teams
ALTER TABLE Batting ADD CONSTRAINT FK_Batting_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- BattingPost → Teams
ALTER TABLE BattingPost ADD CONSTRAINT FK_BattingPost_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- Pitching → Teams
ALTER TABLE Pitching ADD CONSTRAINT FK_Pitching_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- PitchingPost → Teams
ALTER TABLE PitchingPost ADD CONSTRAINT FK_PitchingPost_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- Fielding → Teams
ALTER TABLE Fielding ADD CONSTRAINT FK_Fielding_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- FieldingOFsplit → Teams
ALTER TABLE FieldingOFsplit ADD CONSTRAINT FK_FieldingOFsplit_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- FieldingPost → Teams
ALTER TABLE FieldingPost ADD CONSTRAINT FK_FieldingPost_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- Appearances → Teams
ALTER TABLE Appearances ADD CONSTRAINT FK_Appearances_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- Salaries → Teams
ALTER TABLE Salaries ADD CONSTRAINT FK_Salaries_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- Managers → Teams
ALTER TABLE Managers ADD CONSTRAINT FK_Managers_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- ManagersHalf → Teams
ALTER TABLE ManagersHalf ADD CONSTRAINT FK_ManagersHalf_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- TeamsHalf → Teams
ALTER TABLE TeamsHalf ADD CONSTRAINT FK_TeamsHalf_Teams
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID);
```

### Category 3: Franchise References

```sql
-- Teams → TeamsFranchises
ALTER TABLE Teams ADD CONSTRAINT FK_Teams_TeamsFranchises
    FOREIGN KEY (franchID) REFERENCES TeamsFranchises(franchID);
```

### Category 4: School References

```sql
-- CollegePlaying → Schools
ALTER TABLE CollegePlaying ADD CONSTRAINT FK_CollegePlaying_Schools
    FOREIGN KEY (schoolID) REFERENCES Schools(schoolID);
```

### Category 5: Park References

```sql
-- HomeGames → Parks
ALTER TABLE HomeGames ADD CONSTRAINT FK_HomeGames_Parks
    FOREIGN KEY (parkkey) REFERENCES Parks(parkkey);

-- HomeGames → Teams
ALTER TABLE HomeGames ADD CONSTRAINT FK_HomeGames_Teams
    FOREIGN KEY (teamkey, leaguekey, yearkey) REFERENCES Teams(teamID, lgID, yearID);
```

### Category 6: SeriesPost References

```sql
-- SeriesPost winner → Teams
ALTER TABLE SeriesPost ADD CONSTRAINT FK_SeriesPost_TeamsWinner
    FOREIGN KEY (teamIDwinner, lgIDwinner, yearID) REFERENCES Teams(teamID, lgID, yearID);

-- SeriesPost loser → Teams
ALTER TABLE SeriesPost ADD CONSTRAINT FK_SeriesPost_TeamsLoser
    FOREIGN KEY (teamIDloser, lgIDloser, yearID) REFERENCES Teams(teamID, lgID, yearID);
```

## Data Integrity Notes

### AllstarFull Table

The AllstarFull table has **689 records** that reference teams not in the Teams
table. These are from historical leagues:

- EAS (Eastern Colored League)
- WES (Western League)
- Teams like CAG, KCM, HG, MRS, NE, BEG, NYC, PS, PC, BBB

**Recommendation**: Either add these historical teams to the Teams table or
exclude AllstarFull from the Teams foreign key constraint.

## SQLite Implementation Note

SQLite does not support `ALTER TABLE ADD CONSTRAINT` for foreign keys. To
implement these relationships in SQLite, you must:

1. Create new tables with foreign key constraints
2. Copy data from old tables
3. Drop old tables
4. Rename new tables

Example for Batting table:

```sql
-- Step 1: Create new table with foreign keys
CREATE TABLE Batting_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    stint tinyint NOT NULL,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    G smallint,
    AB smallint,
    R smallint,
    H smallint,
    "2B" smallint,
    "3B" smallint,
    HR smallint,
    RBI smallint,
    SB smallint,
    CS smallint,
    BB smallint,
    SO smallint,
    IBB smallint,
    HBP smallint,
    SH smallint,
    SF smallint,
    GIDP smallint,
    PRIMARY KEY(playerID, yearID, stint, teamID, lgID),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

-- Step 2: Copy data
INSERT INTO Batting_new SELECT * FROM Batting;

-- Step 3: Drop old table
DROP TABLE Batting;

-- Step 4: Rename new table
ALTER TABLE Batting_new RENAME TO Batting;
```

## Entity Relationship Summary

```
People (playerID)
    ├── Batting
    ├── BattingPost
    ├── Pitching
    ├── PitchingPost
    ├── Fielding
    ├── FieldingOF
    ├── FieldingOFsplit
    ├── FieldingPost
    ├── Appearances
    ├── Salaries
    ├── Managers
    ├── ManagersHalf
    ├── AllstarFull
    ├── HallOfFame
    ├── AwardsPlayers
    ├── AwardsManagers
    ├── AwardsSharePlayers
    ├── AwardsShareManagers
    └── CollegePlaying

Teams (teamID, lgID, yearID)
    ├── Batting
    ├── BattingPost
    ├── Pitching
    ├── PitchingPost
    ├── Fielding
    ├── FieldingOFsplit
    ├── FieldingPost
    ├── Appearances
    ├── Salaries
    ├── Managers
    ├── ManagersHalf
    ├── TeamsHalf
    ├── HomeGames
    └── SeriesPost (winner & loser)

TeamsFranchises (franchID)
    └── Teams

Schools (schoolID)
    └── CollegePlaying

Parks (parkkey)
    └── HomeGames
```
