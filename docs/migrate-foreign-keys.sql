-- Foreign Key Migration Script for Lahman Baseball Database
-- This script recreates all tables with proper foreign key constraints

PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

-- ============================================
-- STEP 1: Recreate parent tables first (no changes needed, just for reference)
-- Parent tables: People, TeamsFranchises, Schools, Parks
-- These already have proper primary keys
-- ============================================

-- ============================================
-- STEP 2: Recreate Teams with foreign key to TeamsFranchises
-- ============================================

CREATE TABLE Teams_new (
    yearID smallint NOT NULL,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    franchID nvarchar(4) COLLATE NOCASE,
    divID nvarchar(2) COLLATE NOCASE,
    Rank tinyint,
    G smallint,
    Ghome smallint,
    W smallint,
    L smallint,
    DivWin nvarchar(1) COLLATE NOCASE,
    WCWin nvarchar(1) COLLATE NOCASE,
    LgWin nvarchar(1) COLLATE NOCASE,
    WSWin nvarchar(1) COLLATE NOCASE,
    R smallint,
    AB smallint,
    H smallint,
    "2B" smallint,
    "3B" smallint,
    HR smallint,
    BB smallint,
    SO smallint,
    SB smallint,
    CS smallint,
    HBP smallint,
    SF smallint,
    RA smallint,
    ER smallint,
    ERA float,
    CG smallint,
    SHO smallint,
    SV smallint,
    IPouts smallint,
    HA smallint,
    HRA smallint,
    BBA smallint,
    SOA smallint,
    E smallint,
    DP smallint,
    FP float,
    name nvarchar(100) COLLATE NOCASE,
    park nvarchar(255) COLLATE NOCASE,
    attendance INT,
    BPF tinyint,
    PPF tinyint,
    teamIDBR nvarchar(4) COLLATE NOCASE,
    teamIDlahman45 nvarchar(4) COLLATE NOCASE,
    teamIDretro nvarchar(4) COLLATE NOCASE,
    PRIMARY KEY(teamID, lgID, yearID),
    FOREIGN KEY (franchID) REFERENCES TeamsFranchises(franchID)
);

INSERT INTO Teams_new SELECT * FROM Teams;
DROP TABLE Teams;
ALTER TABLE Teams_new RENAME TO Teams;

-- ============================================
-- STEP 3: Recreate TeamsHalf with foreign key to Teams
-- ============================================

CREATE TABLE TeamsHalf_new (
    yearID smallint NOT NULL,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    Half tinyint NOT NULL,
    divID nvarchar(2) COLLATE NOCASE,
    DivWin nvarchar(1) COLLATE NOCASE,
    Rank tinyint,
    G smallint,
    W smallint,
    L smallint,
    PRIMARY KEY(teamID, lgID, yearID, Half),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO TeamsHalf_new SELECT * FROM TeamsHalf;
DROP TABLE TeamsHalf;
ALTER TABLE TeamsHalf_new RENAME TO TeamsHalf;

-- ============================================
-- STEP 4: Recreate Batting with foreign keys
-- ============================================

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

INSERT INTO Batting_new SELECT * FROM Batting;
DROP TABLE Batting;
ALTER TABLE Batting_new RENAME TO Batting;

-- ============================================
-- STEP 5: Recreate BattingPost with foreign keys
-- ============================================

CREATE TABLE BattingPost_new (
    yearID smallint NOT NULL,
    round nvarchar(10) NOT NULL COLLATE NOCASE,
    playerID nvarchar(20) NOT NULL COLLATE NOCASE,
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
    PRIMARY KEY(yearID, round, playerID, teamID, lgID),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO BattingPost_new SELECT * FROM BattingPost;
DROP TABLE BattingPost;
ALTER TABLE BattingPost_new RENAME TO BattingPost;

-- ============================================
-- STEP 6: Recreate Pitching with foreign keys
-- ============================================

CREATE TABLE Pitching_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    stint tinyint NOT NULL,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    W smallint,
    L smallint,
    G smallint,
    GS smallint,
    CG smallint,
    SHO smallint,
    SV smallint,
    IPouts smallint,
    H smallint,
    ER smallint,
    HR smallint,
    BB smallint,
    SO smallint,
    BAOpp float,
    ERA float,
    IBB smallint,
    WP smallint,
    HBP smallint,
    BK smallint,
    BFP smallint,
    GF smallint,
    R smallint,
    SH smallint,
    SF smallint,
    GIDP smallint,
    PRIMARY KEY(playerID, yearID, stint),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO Pitching_new SELECT * FROM Pitching;
DROP TABLE Pitching;
ALTER TABLE Pitching_new RENAME TO Pitching;

-- ============================================
-- STEP 7: Recreate PitchingPost with foreign keys
-- ============================================

CREATE TABLE PitchingPost_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    round nvarchar(10) NOT NULL COLLATE NOCASE,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    W smallint,
    L smallint,
    G smallint,
    GS smallint,
    CG smallint,
    SHO smallint,
    SV smallint,
    IPouts INT,
    H smallint,
    ER smallint,
    HR smallint,
    BB smallint,
    SO smallint,
    BAOpp float,
    ERA float,
    IBB smallint,
    WP smallint,
    HBP smallint,
    BK smallint,
    BFP smallint,
    GF smallint,
    R smallint,
    SH smallint,
    SF smallint,
    GIDP smallint,
    PRIMARY KEY(playerID, yearID, round),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO PitchingPost_new SELECT * FROM PitchingPost;
DROP TABLE PitchingPost;
ALTER TABLE PitchingPost_new RENAME TO PitchingPost;

-- ============================================
-- STEP 8: Recreate Fielding with foreign keys
-- ============================================

CREATE TABLE Fielding_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    stint tinyint NOT NULL,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    POS nvarchar(2) NOT NULL COLLATE NOCASE,
    G smallint,
    GS smallint,
    InnOuts smallint,
    PO smallint,
    A smallint,
    E smallint,
    DP smallint,
    PB smallint,
    WP smallint,
    SB smallint,
    CS smallint,
    ZR float,
    PRIMARY KEY(playerID, yearID, stint, teamID, lgID, POS),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO Fielding_new SELECT * FROM Fielding;
DROP TABLE Fielding;
ALTER TABLE Fielding_new RENAME TO Fielding;

-- ============================================
-- STEP 9: Recreate FieldingOF with foreign key to People
-- ============================================

CREATE TABLE FieldingOF_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    stint tinyint NOT NULL,
    Glf smallint,
    Gcf smallint,
    Grf smallint,
    PRIMARY KEY(playerID, yearID, stint),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO FieldingOF_new SELECT * FROM FieldingOF;
DROP TABLE FieldingOF;
ALTER TABLE FieldingOF_new RENAME TO FieldingOF;

-- ============================================
-- STEP 10: Recreate FieldingOFsplit with foreign keys
-- ============================================

CREATE TABLE FieldingOFsplit_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    stint tinyint NOT NULL,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    POS nvarchar(2) NOT NULL COLLATE NOCASE,
    G smallint,
    GS smallint,
    InnOuts smallint,
    PO smallint,
    A smallint,
    E smallint,
    DP smallint,
    PB smallint,
    WP smallint,
    SB smallint,
    CS smallint,
    ZR float,
    PRIMARY KEY(playerID, yearID, stint, teamID, lgID, POS),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO FieldingOFsplit_new SELECT * FROM FieldingOFsplit;
DROP TABLE FieldingOFsplit;
ALTER TABLE FieldingOFsplit_new RENAME TO FieldingOFsplit;

-- ============================================
-- STEP 11: Recreate FieldingPost with foreign keys
-- ============================================

CREATE TABLE FieldingPost_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    round nvarchar(10) NOT NULL COLLATE NOCASE,
    POS nvarchar(2) NOT NULL COLLATE NOCASE,
    G smallint,
    GS smallint,
    InnOuts smallint,
    PO smallint,
    A smallint,
    E smallint,
    DP smallint,
    TP smallint,
    PB smallint,
    SB smallint,
    CS smallint,
    PRIMARY KEY(playerID, yearID, teamID, lgID, round, POS),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO FieldingPost_new SELECT * FROM FieldingPost;
DROP TABLE FieldingPost;
ALTER TABLE FieldingPost_new RENAME TO FieldingPost;

-- ============================================
-- STEP 12: Recreate Appearances with foreign keys
-- ============================================

CREATE TABLE Appearances_new (
    yearID smallint NOT NULL,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    playerID nvarchar(20) NOT NULL COLLATE NOCASE,
    G_all smallint,
    GS smallint,
    G_batting smallint,
    G_defense smallint,
    G_p smallint,
    G_c smallint,
    G_1b smallint,
    G_2b smallint,
    G_3b smallint,
    G_ss smallint,
    G_lf smallint,
    G_cf smallint,
    G_rf smallint,
    G_of smallint,
    G_dh smallint,
    G_ph smallint,
    G_pr smallint,
    PRIMARY KEY(playerID, lgID, teamID, yearID),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO Appearances_new SELECT * FROM Appearances;
DROP TABLE Appearances;
ALTER TABLE Appearances_new RENAME TO Appearances;

-- ============================================
-- STEP 13: Recreate Salaries with foreign keys
-- ============================================

CREATE TABLE Salaries_new (
    yearID smallint NOT NULL,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    playerID nvarchar(20) NOT NULL COLLATE NOCASE,
    salary bigint,
    PRIMARY KEY(playerID, teamID, lgID, yearID),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO Salaries_new SELECT * FROM Salaries;
DROP TABLE Salaries;
ALTER TABLE Salaries_new RENAME TO Salaries;

-- ============================================
-- STEP 14: Recreate Managers with foreign keys
-- ============================================

CREATE TABLE Managers_new (
    playerID nvarchar(20),
    yearID smallint NOT NULL,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    inseason tinyint NOT NULL,
    G smallint,
    W smallint,
    L smallint,
    rank tinyint,
    plyrMgr nvarchar(1) COLLATE NOCASE,
    PRIMARY KEY(playerID, yearID, teamID, lgID, inseason),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO Managers_new SELECT * FROM Managers;
DROP TABLE Managers;
ALTER TABLE Managers_new RENAME TO Managers;

-- ============================================
-- STEP 15: Recreate ManagersHalf with foreign keys
-- ============================================

CREATE TABLE ManagersHalf_new (
    playerID nvarchar(20) NOT NULL,
    yearID smallint NOT NULL,
    teamID nvarchar(4) NOT NULL COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    inseason tinyint,
    half tinyint NOT NULL,
    G smallint,
    W smallint,
    L smallint,
    rank tinyint,
    PRIMARY KEY(playerID, yearID, teamID, lgID, half),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (teamID, lgID, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO ManagersHalf_new SELECT * FROM ManagersHalf;
DROP TABLE ManagersHalf;
ALTER TABLE ManagersHalf_new RENAME TO ManagersHalf;

-- ============================================
-- STEP 16: Recreate AllstarFull with foreign key to People only
-- (Teams FK excluded due to 689 orphan records from historical leagues)
-- ============================================

CREATE TABLE AllstarFull_new (
    playerID nvarchar(20),
    yearID smallint,
    gameNum tinyint,
    gameID nvarchar(20) COLLATE NOCASE,
    teamID nvarchar(4) COLLATE NOCASE,
    lgID nvarchar(3) COLLATE NOCASE,
    GP tinyint,
    startingPos nvarchar(10) COLLATE NOCASE,
    PRIMARY KEY(playerID, yearID, lgID, teamID, gameID),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO AllstarFull_new SELECT * FROM AllstarFull;
DROP TABLE AllstarFull;
ALTER TABLE AllstarFull_new RENAME TO AllstarFull;

-- ============================================
-- STEP 17: Recreate HallOfFame with foreign key to People
-- ============================================

CREATE TABLE HallOfFame_new (
    playerID nvarchar(20) NOT NULL,
    yearid smallint NOT NULL,
    votedBy nvarchar(255) NOT NULL COLLATE NOCASE,
    ballots smallint,
    needed smallint,
    votes smallint,
    inducted nvarchar(1) COLLATE NOCASE,
    category nvarchar(100) COLLATE NOCASE,
    needed_note nvarchar(1000) COLLATE NOCASE,
    PRIMARY KEY(playerID, yearid, votedBy),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO HallOfFame_new SELECT * FROM HallOfFame;
DROP TABLE HallOfFame;
ALTER TABLE HallOfFame_new RENAME TO HallOfFame;

-- ============================================
-- STEP 18: Recreate AwardsPlayers with foreign key to People
-- ============================================

CREATE TABLE AwardsPlayers_new (
    playerID nvarchar(20),
    awardID nvarchar(255) COLLATE NOCASE,
    yearID INT,
    lgID nvarchar(3) COLLATE NOCASE,
    tie nvarchar(1) COLLATE NOCASE,
    notes nvarchar(255) COLLATE NOCASE,
    PRIMARY KEY(playerID, yearID, awardID, lgID, tie, notes),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO AwardsPlayers_new SELECT * FROM AwardsPlayers;
DROP TABLE AwardsPlayers;
ALTER TABLE AwardsPlayers_new RENAME TO AwardsPlayers;

-- ============================================
-- STEP 19: Recreate AwardsManagers with foreign key to People
-- ============================================

CREATE TABLE AwardsManagers_new (
    playerID nvarchar(20) NOT NULL,
    awardID nvarchar(255) NOT NULL COLLATE NOCASE,
    yearID smallint NOT NULL,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    tie nvarchar(1) COLLATE NOCASE,
    notes nvarchar(255) COLLATE NOCASE,
    PRIMARY KEY(playerID, awardID, yearID, lgID, tie),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO AwardsManagers_new SELECT * FROM AwardsManagers;
DROP TABLE AwardsManagers;
ALTER TABLE AwardsManagers_new RENAME TO AwardsManagers;

-- ============================================
-- STEP 20: Recreate AwardsSharePlayers with foreign key to People
-- ============================================

CREATE TABLE AwardsSharePlayers_new (
    awardID nvarchar(255) NOT NULL,
    yearID smallint NOT NULL,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    playerID nvarchar(20) NOT NULL COLLATE NOCASE,
    pointsWon smallint,
    pointsMax smallint,
    votesFirst smallint,
    PRIMARY KEY(playerID, lgID, yearID, awardID),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO AwardsSharePlayers_new SELECT * FROM AwardsSharePlayers;
DROP TABLE AwardsSharePlayers;
ALTER TABLE AwardsSharePlayers_new RENAME TO AwardsSharePlayers;

-- ============================================
-- STEP 21: Recreate AwardsShareManagers with foreign key to People
-- ============================================

CREATE TABLE AwardsShareManagers_new (
    awardID nvarchar(255) NOT NULL,
    yearID smallint NOT NULL,
    lgID nvarchar(3) NOT NULL COLLATE NOCASE,
    playerID nvarchar(20) NOT NULL COLLATE NOCASE,
    pointsWon smallint,
    pointsMax smallint,
    votesFirst smallint,
    PRIMARY KEY(playerID, lgID, yearID, awardID),
    FOREIGN KEY (playerID) REFERENCES People(playerID)
);

INSERT INTO AwardsShareManagers_new SELECT * FROM AwardsShareManagers;
DROP TABLE AwardsShareManagers;
ALTER TABLE AwardsShareManagers_new RENAME TO AwardsShareManagers;

-- ============================================
-- STEP 22: Recreate CollegePlaying with foreign keys
-- ============================================

CREATE TABLE CollegePlaying_new (
    playerID nvarchar(20) NOT NULL,
    schoolID nvarchar(20) COLLATE NOCASE,
    yearID smallint,
    PRIMARY KEY(playerID, schoolID, yearID),
    FOREIGN KEY (playerID) REFERENCES People(playerID),
    FOREIGN KEY (schoolID) REFERENCES Schools(schoolID)
);

INSERT INTO CollegePlaying_new SELECT * FROM CollegePlaying;
DROP TABLE CollegePlaying;
ALTER TABLE CollegePlaying_new RENAME TO CollegePlaying;

-- ============================================
-- STEP 23: Recreate HomeGames with foreign keys
-- ============================================

CREATE TABLE HomeGames_new (
    yearkey INT,
    leaguekey nvarchar(3) COLLATE NOCASE,
    teamkey nvarchar(4) COLLATE NOCASE,
    parkkey nvarchar(20) COLLATE NOCASE,
    spanfirst nvarchar(10) COLLATE NOCASE,
    spanlast nvarchar(10) COLLATE NOCASE,
    games smallint,
    openings smallint,
    attendance INT,
    PRIMARY KEY(yearkey, leaguekey, teamkey, parkkey),
    FOREIGN KEY (parkkey) REFERENCES Parks(parkkey),
    FOREIGN KEY (teamkey, leaguekey, yearkey) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO HomeGames_new SELECT * FROM HomeGames;
DROP TABLE HomeGames;
ALTER TABLE HomeGames_new RENAME TO HomeGames;

-- ============================================
-- STEP 24: Recreate SeriesPost with foreign keys to Teams (winner and loser)
-- ============================================

CREATE TABLE SeriesPost_new (
    yearID smallint NOT NULL,
    round nvarchar(10) NOT NULL COLLATE NOCASE,
    teamIDwinner nvarchar(4) COLLATE NOCASE,
    lgIDwinner nvarchar(3) COLLATE NOCASE,
    teamIDloser nvarchar(4) COLLATE NOCASE,
    lgIDloser nvarchar(3) COLLATE NOCASE,
    wins smallint,
    losses smallint,
    ties smallint,
    PRIMARY KEY(teamIDwinner, lgIDwinner, yearID, round),
    FOREIGN KEY (teamIDwinner, lgIDwinner, yearID) REFERENCES Teams(teamID, lgID, yearID),
    FOREIGN KEY (teamIDloser, lgIDloser, yearID) REFERENCES Teams(teamID, lgID, yearID)
);

INSERT INTO SeriesPost_new SELECT * FROM SeriesPost;
DROP TABLE SeriesPost;
ALTER TABLE SeriesPost_new RENAME TO SeriesPost;

COMMIT;

-- Verify foreign keys are working
PRAGMA foreign_keys = ON;
PRAGMA foreign_key_check;

-- Show table info to confirm
SELECT 'Migration completed successfully!' AS status;
