/******************************************************************************
 * Lahman Baseball Database -- SQL Server schema
 * Migrated from SQLite (lahman.db)
 * Generated automatically; see migration notes at top of file.
 *-----------------------------------------------------------------------------
 * Notes on conversions from the source SQLite schema:
 *   - Identifiers bracket-quoted ([2B], [Rank] are reserved/illegal bare).
 *   - SQLite's COLLATE NOCASE removed; rely on DB default collation.
 *   - PK columns forced to NOT NULL (SQL Server requirement).
 *   - FOREIGN KEYs added at the end via ALTER TABLE.
 *   - Data file converts SQLite's empty-string sentinels in numeric
 *     columns to NULL (see 02_data.sql).
 ******************************************************************************/
USE BASEBALLHISTORY;

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---- Clean slate: drop existing objects (safe to re-run) ---- */
IF OBJECT_ID(N'dbo.FK_TeamsHalf_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[TeamsHalf] DROP CONSTRAINT [FK_TeamsHalf_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_SeriesPost_Teams_teamIDwinner_lgIDwinner_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[SeriesPost] DROP CONSTRAINT [FK_SeriesPost_Teams_teamIDwinner_lgIDwinner_yearID];
IF OBJECT_ID(N'dbo.FK_SeriesPost_Teams_teamIDloser_lgIDloser_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[SeriesPost] DROP CONSTRAINT [FK_SeriesPost_Teams_teamIDloser_lgIDloser_yearID];
IF OBJECT_ID(N'dbo.FK_Salaries_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Salaries] DROP CONSTRAINT [FK_Salaries_People_playerID];
IF OBJECT_ID(N'dbo.FK_Salaries_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Salaries] DROP CONSTRAINT [FK_Salaries_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_PitchingPost_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[PitchingPost] DROP CONSTRAINT [FK_PitchingPost_People_playerID];
IF OBJECT_ID(N'dbo.FK_PitchingPost_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[PitchingPost] DROP CONSTRAINT [FK_PitchingPost_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_Pitching_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Pitching] DROP CONSTRAINT [FK_Pitching_People_playerID];
IF OBJECT_ID(N'dbo.FK_Pitching_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Pitching] DROP CONSTRAINT [FK_Pitching_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_ManagersHalf_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[ManagersHalf] DROP CONSTRAINT [FK_ManagersHalf_People_playerID];
IF OBJECT_ID(N'dbo.FK_ManagersHalf_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[ManagersHalf] DROP CONSTRAINT [FK_ManagersHalf_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_Managers_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Managers] DROP CONSTRAINT [FK_Managers_People_playerID];
IF OBJECT_ID(N'dbo.FK_Managers_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Managers] DROP CONSTRAINT [FK_Managers_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_HomeGames_Parks_parkkey', N'F') IS NOT NULL ALTER TABLE dbo.[HomeGames] DROP CONSTRAINT [FK_HomeGames_Parks_parkkey];
IF OBJECT_ID(N'dbo.FK_HomeGames_Teams_teamkey_leaguekey_yearkey', N'F') IS NOT NULL ALTER TABLE dbo.[HomeGames] DROP CONSTRAINT [FK_HomeGames_Teams_teamkey_leaguekey_yearkey];
IF OBJECT_ID(N'dbo.FK_HallOfFame_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[HallOfFame] DROP CONSTRAINT [FK_HallOfFame_People_playerID];
IF OBJECT_ID(N'dbo.FK_FieldingPost_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[FieldingPost] DROP CONSTRAINT [FK_FieldingPost_People_playerID];
IF OBJECT_ID(N'dbo.FK_FieldingPost_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[FieldingPost] DROP CONSTRAINT [FK_FieldingPost_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_FieldingOFsplit_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[FieldingOFsplit] DROP CONSTRAINT [FK_FieldingOFsplit_People_playerID];
IF OBJECT_ID(N'dbo.FK_FieldingOFsplit_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[FieldingOFsplit] DROP CONSTRAINT [FK_FieldingOFsplit_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_FieldingOF_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[FieldingOF] DROP CONSTRAINT [FK_FieldingOF_People_playerID];
IF OBJECT_ID(N'dbo.FK_Fielding_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Fielding] DROP CONSTRAINT [FK_Fielding_People_playerID];
IF OBJECT_ID(N'dbo.FK_Fielding_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Fielding] DROP CONSTRAINT [FK_Fielding_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_CollegePlaying_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[CollegePlaying] DROP CONSTRAINT [FK_CollegePlaying_People_playerID];
IF OBJECT_ID(N'dbo.FK_CollegePlaying_Schools_schoolID', N'F') IS NOT NULL ALTER TABLE dbo.[CollegePlaying] DROP CONSTRAINT [FK_CollegePlaying_Schools_schoolID];
IF OBJECT_ID(N'dbo.FK_BattingPost_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[BattingPost] DROP CONSTRAINT [FK_BattingPost_People_playerID];
IF OBJECT_ID(N'dbo.FK_BattingPost_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[BattingPost] DROP CONSTRAINT [FK_BattingPost_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_Batting_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Batting] DROP CONSTRAINT [FK_Batting_People_playerID];
IF OBJECT_ID(N'dbo.FK_Batting_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Batting] DROP CONSTRAINT [FK_Batting_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_AwardsSharePlayers_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[AwardsSharePlayers] DROP CONSTRAINT [FK_AwardsSharePlayers_People_playerID];
IF OBJECT_ID(N'dbo.FK_AwardsShareManagers_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[AwardsShareManagers] DROP CONSTRAINT [FK_AwardsShareManagers_People_playerID];
IF OBJECT_ID(N'dbo.FK_AwardsPlayers_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[AwardsPlayers] DROP CONSTRAINT [FK_AwardsPlayers_People_playerID];
IF OBJECT_ID(N'dbo.FK_AwardsManagers_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[AwardsManagers] DROP CONSTRAINT [FK_AwardsManagers_People_playerID];
IF OBJECT_ID(N'dbo.FK_Appearances_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[Appearances] DROP CONSTRAINT [FK_Appearances_People_playerID];
IF OBJECT_ID(N'dbo.FK_Appearances_Teams_teamID_lgID_yearID', N'F') IS NOT NULL ALTER TABLE dbo.[Appearances] DROP CONSTRAINT [FK_Appearances_Teams_teamID_lgID_yearID];
IF OBJECT_ID(N'dbo.FK_AllstarFull_People_playerID', N'F') IS NOT NULL ALTER TABLE dbo.[AllstarFull] DROP CONSTRAINT [FK_AllstarFull_People_playerID];
IF OBJECT_ID(N'dbo.FK_Teams_TeamsFranchises_franchID', N'F') IS NOT NULL ALTER TABLE dbo.[Teams] DROP CONSTRAINT [FK_Teams_TeamsFranchises_franchID];

DROP TABLE IF EXISTS dbo.[TeamsHalf];
DROP TABLE IF EXISTS dbo.[SeriesPost];
DROP TABLE IF EXISTS dbo.[Salaries];
DROP TABLE IF EXISTS dbo.[PitchingPost];
DROP TABLE IF EXISTS dbo.[Pitching];
DROP TABLE IF EXISTS dbo.[ManagersHalf];
DROP TABLE IF EXISTS dbo.[Managers];
DROP TABLE IF EXISTS dbo.[HomeGames];
DROP TABLE IF EXISTS dbo.[HallOfFame];
DROP TABLE IF EXISTS dbo.[FieldingPost];
DROP TABLE IF EXISTS dbo.[FieldingOFsplit];
DROP TABLE IF EXISTS dbo.[FieldingOF];
DROP TABLE IF EXISTS dbo.[Fielding];
DROP TABLE IF EXISTS dbo.[CollegePlaying];
DROP TABLE IF EXISTS dbo.[BattingPost];
DROP TABLE IF EXISTS dbo.[Batting];
DROP TABLE IF EXISTS dbo.[AwardsSharePlayers];
DROP TABLE IF EXISTS dbo.[AwardsShareManagers];
DROP TABLE IF EXISTS dbo.[AwardsPlayers];
DROP TABLE IF EXISTS dbo.[AwardsManagers];
DROP TABLE IF EXISTS dbo.[Appearances];
DROP TABLE IF EXISTS dbo.[AllstarFull];
DROP TABLE IF EXISTS dbo.[Teams];
DROP TABLE IF EXISTS dbo.[Parks];
DROP TABLE IF EXISTS dbo.[TeamsFranchises];
DROP TABLE IF EXISTS dbo.[Schools];
DROP TABLE IF EXISTS dbo.[People];
GO

/* ---- Table definitions ---- */

CREATE TABLE dbo.[People] (
    [ID] INT,
    [playerID] NVARCHAR(20) NOT NULL,
    [birthYear] INT,
    [birthMonth] INT,
    [birthDay] INT,
    [birthCity] NVARCHAR(100),
    [birthCountry] NVARCHAR(50),
    [birthState] NVARCHAR(50),
    [deathYear] INT,
    [deathMonth] INT,
    [deathDay] INT,
    [deathCountry] NVARCHAR(50),
    [deathState] NVARCHAR(50),
    [deathCity] NVARCHAR(100),
    [nameFirst] NVARCHAR(100),
    [nameLast] NVARCHAR(100),
    [nameGiven] NVARCHAR(100),
    [weight] NVARCHAR(10),
    [height] NVARCHAR(10),
    [bats] NVARCHAR(10),
    [throws] NVARCHAR(10),
    [debut] NVARCHAR(20),
    [bbrefID] NVARCHAR(20),
    [finalGame] NVARCHAR(20),
    [retroID] NVARCHAR(20),
    CONSTRAINT [PK_People] PRIMARY KEY ([playerID])
);
GO

CREATE TABLE dbo.[Schools] (
    [schoolID] NVARCHAR(20) NOT NULL,
    [name_full] NVARCHAR(255),
    [city] NVARCHAR(100),
    [state] NVARCHAR(50),
    [country] NVARCHAR(50),
    CONSTRAINT [PK_Schools] PRIMARY KEY ([schoolID])
);
GO

CREATE TABLE dbo.[TeamsFranchises] (
    [franchID] NVARCHAR(4) NOT NULL,
    [franchName] NVARCHAR(100),
    [active] NVARCHAR(2),
    [NAassoc] NVARCHAR(4),
    CONSTRAINT [PK_TeamsFranchises] PRIMARY KEY ([franchID])
);
GO

CREATE TABLE dbo.[Parks] (
    [ID] INT NOT NULL,
    [parkalias] NVARCHAR(512),
    [parkkey] NVARCHAR(20),
    [parkname] NVARCHAR(255),
    [city] NVARCHAR(100),
    [state] NVARCHAR(50),
    [country] NVARCHAR(50),
    CONSTRAINT [PK_Parks] PRIMARY KEY ([ID]),
    CONSTRAINT [UQ_Parks_parkkey] UNIQUE ([parkkey])
);
GO

CREATE TABLE dbo.[Teams] (
    [yearID] SMALLINT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [franchID] NVARCHAR(4),
    [divID] NVARCHAR(2),
    [Rank] TINYINT,
    [G] SMALLINT,
    [Ghome] SMALLINT,
    [W] SMALLINT,
    [L] SMALLINT,
    [DivWin] NVARCHAR(1),
    [WCWin] NVARCHAR(1),
    [LgWin] NVARCHAR(1),
    [WSWin] NVARCHAR(1),
    [R] SMALLINT,
    [AB] SMALLINT,
    [H] SMALLINT,
    [2B] SMALLINT,
    [3B] SMALLINT,
    [HR] SMALLINT,
    [BB] SMALLINT,
    [SO] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    [HBP] SMALLINT,
    [SF] SMALLINT,
    [RA] SMALLINT,
    [ER] SMALLINT,
    [ERA] FLOAT,
    [CG] SMALLINT,
    [SHO] SMALLINT,
    [SV] SMALLINT,
    [IPouts] SMALLINT,
    [HA] SMALLINT,
    [HRA] SMALLINT,
    [BBA] SMALLINT,
    [SOA] SMALLINT,
    [E] SMALLINT,
    [DP] SMALLINT,
    [FP] FLOAT,
    [name] NVARCHAR(100),
    [park] NVARCHAR(255),
    [attendance] INT,
    [BPF] TINYINT,
    [PPF] TINYINT,
    [teamIDBR] NVARCHAR(4),
    [teamIDlahman45] NVARCHAR(4),
    [teamIDretro] NVARCHAR(4),
    CONSTRAINT [PK_Teams] PRIMARY KEY ([teamID], [lgID], [yearID])
);
GO

CREATE TABLE dbo.[AllstarFull] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [gameNum] TINYINT,
    [gameID] NVARCHAR(20) NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [GP] TINYINT,
    [startingPos] NVARCHAR(10),
    CONSTRAINT [PK_AllstarFull] PRIMARY KEY ([playerID], [yearID], [lgID], [teamID], [gameID])
);
GO

CREATE TABLE dbo.[Appearances] (
    [yearID] SMALLINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [playerID] NVARCHAR(20) NOT NULL,
    [G_all] SMALLINT,
    [GS] SMALLINT,
    [G_batting] SMALLINT,
    [G_defense] SMALLINT,
    [G_p] SMALLINT,
    [G_c] SMALLINT,
    [G_1b] SMALLINT,
    [G_2b] SMALLINT,
    [G_3b] SMALLINT,
    [G_ss] SMALLINT,
    [G_lf] SMALLINT,
    [G_cf] SMALLINT,
    [G_rf] SMALLINT,
    [G_of] SMALLINT,
    [G_dh] SMALLINT,
    [G_ph] SMALLINT,
    [G_pr] SMALLINT,
    CONSTRAINT [PK_Appearances] PRIMARY KEY ([playerID], [lgID], [teamID], [yearID])
);
GO

CREATE TABLE dbo.[AwardsManagers] (
    [playerID] NVARCHAR(20) NOT NULL,
    [awardID] NVARCHAR(255) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [tie] NVARCHAR(1) NOT NULL,
    [notes] NVARCHAR(255),
    CONSTRAINT [PK_AwardsManagers] PRIMARY KEY ([playerID], [awardID], [yearID], [lgID], [tie])
);
GO

CREATE TABLE dbo.[AwardsPlayers] (
    [playerID] NVARCHAR(20) NOT NULL,
    [awardID] NVARCHAR(255) NOT NULL,
    [yearID] INT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [tie] NVARCHAR(1) NOT NULL,
    [notes] NVARCHAR(255) NOT NULL,
    CONSTRAINT [PK_AwardsPlayers] PRIMARY KEY ([playerID], [yearID], [awardID], [lgID], [tie], [notes])
);
GO

CREATE TABLE dbo.[AwardsShareManagers] (
    [awardID] NVARCHAR(255) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [playerID] NVARCHAR(20) NOT NULL,
    [pointsWon] SMALLINT,
    [pointsMax] SMALLINT,
    [votesFirst] SMALLINT,
    CONSTRAINT [PK_AwardsShareManagers] PRIMARY KEY ([playerID], [lgID], [yearID], [awardID])
);
GO

CREATE TABLE dbo.[AwardsSharePlayers] (
    [awardID] NVARCHAR(255) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [playerID] NVARCHAR(20) NOT NULL,
    [pointsWon] SMALLINT,
    [pointsMax] SMALLINT,
    [votesFirst] SMALLINT,
    CONSTRAINT [PK_AwardsSharePlayers] PRIMARY KEY ([playerID], [lgID], [yearID], [awardID])
);
GO

CREATE TABLE dbo.[Batting] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [stint] TINYINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [G] SMALLINT,
    [AB] SMALLINT,
    [R] SMALLINT,
    [H] SMALLINT,
    [2B] SMALLINT,
    [3B] SMALLINT,
    [HR] SMALLINT,
    [RBI] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    [BB] SMALLINT,
    [SO] SMALLINT,
    [IBB] SMALLINT,
    [HBP] SMALLINT,
    [SH] SMALLINT,
    [SF] SMALLINT,
    [GIDP] SMALLINT,
    CONSTRAINT [PK_Batting] PRIMARY KEY ([playerID], [yearID], [stint], [teamID], [lgID])
);
GO

CREATE TABLE dbo.[BattingPost] (
    [yearID] SMALLINT NOT NULL,
    [round] NVARCHAR(10) NOT NULL,
    [playerID] NVARCHAR(20) NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [G] SMALLINT,
    [AB] SMALLINT,
    [R] SMALLINT,
    [H] SMALLINT,
    [2B] SMALLINT,
    [3B] SMALLINT,
    [HR] SMALLINT,
    [RBI] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    [BB] SMALLINT,
    [SO] SMALLINT,
    [IBB] SMALLINT,
    [HBP] SMALLINT,
    [SH] SMALLINT,
    [SF] SMALLINT,
    [GIDP] SMALLINT,
    CONSTRAINT [PK_BattingPost] PRIMARY KEY ([yearID], [round], [playerID], [teamID], [lgID])
);
GO

CREATE TABLE dbo.[CollegePlaying] (
    [playerID] NVARCHAR(20) NOT NULL,
    [schoolID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    CONSTRAINT [PK_CollegePlaying] PRIMARY KEY ([playerID], [schoolID], [yearID])
);
GO

CREATE TABLE dbo.[Fielding] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [stint] TINYINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [POS] NVARCHAR(2) NOT NULL,
    [G] SMALLINT,
    [GS] SMALLINT,
    [InnOuts] SMALLINT,
    [PO] SMALLINT,
    [A] SMALLINT,
    [E] SMALLINT,
    [DP] SMALLINT,
    [PB] SMALLINT,
    [WP] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    [ZR] FLOAT,
    CONSTRAINT [PK_Fielding] PRIMARY KEY ([playerID], [yearID], [stint], [teamID], [lgID], [POS])
);
GO

CREATE TABLE dbo.[FieldingOF] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [stint] TINYINT NOT NULL,
    [Glf] SMALLINT,
    [Gcf] SMALLINT,
    [Grf] SMALLINT,
    CONSTRAINT [PK_FieldingOF] PRIMARY KEY ([playerID], [yearID], [stint])
);
GO

CREATE TABLE dbo.[FieldingOFsplit] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [stint] TINYINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [POS] NVARCHAR(2) NOT NULL,
    [G] SMALLINT,
    [GS] SMALLINT,
    [InnOuts] SMALLINT,
    [PO] SMALLINT,
    [A] SMALLINT,
    [E] SMALLINT,
    [DP] SMALLINT,
    [PB] SMALLINT,
    [WP] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    [ZR] FLOAT,
    CONSTRAINT [PK_FieldingOFsplit] PRIMARY KEY ([playerID], [yearID], [stint], [teamID], [lgID], [POS])
);
GO

CREATE TABLE dbo.[FieldingPost] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [round] NVARCHAR(10) NOT NULL,
    [POS] NVARCHAR(2) NOT NULL,
    [G] SMALLINT,
    [GS] SMALLINT,
    [InnOuts] SMALLINT,
    [PO] SMALLINT,
    [A] SMALLINT,
    [E] SMALLINT,
    [DP] SMALLINT,
    [TP] SMALLINT,
    [PB] SMALLINT,
    [SB] SMALLINT,
    [CS] SMALLINT,
    CONSTRAINT [PK_FieldingPost] PRIMARY KEY ([playerID], [yearID], [teamID], [lgID], [round], [POS])
);
GO

CREATE TABLE dbo.[HallOfFame] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearid] SMALLINT NOT NULL,
    [votedBy] NVARCHAR(255) NOT NULL,
    [ballots] SMALLINT,
    [needed] SMALLINT,
    [votes] SMALLINT,
    [inducted] NVARCHAR(1),
    [category] NVARCHAR(100),
    [needed_note] NVARCHAR(1000),
    CONSTRAINT [PK_HallOfFame] PRIMARY KEY ([playerID], [yearid], [votedBy])
);
GO

CREATE TABLE dbo.[HomeGames] (
    [yearkey] INT NOT NULL,
    [leaguekey] NVARCHAR(3) NOT NULL,
    [teamkey] NVARCHAR(4) NOT NULL,
    [parkkey] NVARCHAR(20) NOT NULL,
    [spanfirst] NVARCHAR(10),
    [spanlast] NVARCHAR(10),
    [games] SMALLINT,
    [openings] SMALLINT,
    [attendance] INT,
    CONSTRAINT [PK_HomeGames] PRIMARY KEY ([yearkey], [leaguekey], [teamkey], [parkkey])
);
GO

CREATE TABLE dbo.[Managers] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [inseason] TINYINT NOT NULL,
    [G] SMALLINT,
    [W] SMALLINT,
    [L] SMALLINT,
    [rank] TINYINT,
    [plyrMgr] NVARCHAR(1),
    CONSTRAINT [PK_Managers] PRIMARY KEY ([playerID], [yearID], [teamID], [lgID], [inseason])
);
GO

CREATE TABLE dbo.[ManagersHalf] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [inseason] TINYINT,
    [half] TINYINT NOT NULL,
    [G] SMALLINT,
    [W] SMALLINT,
    [L] SMALLINT,
    [rank] TINYINT,
    CONSTRAINT [PK_ManagersHalf] PRIMARY KEY ([playerID], [yearID], [teamID], [lgID], [half])
);
GO

CREATE TABLE dbo.[Pitching] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [stint] TINYINT NOT NULL,
    [teamID] NVARCHAR(4),
    [lgID] NVARCHAR(3),
    [W] SMALLINT,
    [L] SMALLINT,
    [G] SMALLINT,
    [GS] SMALLINT,
    [CG] SMALLINT,
    [SHO] SMALLINT,
    [SV] SMALLINT,
    [IPouts] SMALLINT,
    [H] SMALLINT,
    [ER] SMALLINT,
    [HR] SMALLINT,
    [BB] SMALLINT,
    [SO] SMALLINT,
    [BAOpp] FLOAT,
    [ERA] FLOAT,
    [IBB] SMALLINT,
    [WP] SMALLINT,
    [HBP] SMALLINT,
    [BK] SMALLINT,
    [BFP] SMALLINT,
    [GF] SMALLINT,
    [R] SMALLINT,
    [SH] SMALLINT,
    [SF] SMALLINT,
    [GIDP] SMALLINT,
    CONSTRAINT [PK_Pitching] PRIMARY KEY ([playerID], [yearID], [stint])
);
GO

CREATE TABLE dbo.[PitchingPost] (
    [playerID] NVARCHAR(20) NOT NULL,
    [yearID] SMALLINT NOT NULL,
    [round] NVARCHAR(10) NOT NULL,
    [teamID] NVARCHAR(4),
    [lgID] NVARCHAR(3),
    [W] SMALLINT,
    [L] SMALLINT,
    [G] SMALLINT,
    [GS] SMALLINT,
    [CG] SMALLINT,
    [SHO] SMALLINT,
    [SV] SMALLINT,
    [IPouts] INT,
    [H] SMALLINT,
    [ER] SMALLINT,
    [HR] SMALLINT,
    [BB] SMALLINT,
    [SO] SMALLINT,
    [BAOpp] FLOAT,
    [ERA] FLOAT,
    [IBB] SMALLINT,
    [WP] SMALLINT,
    [HBP] SMALLINT,
    [BK] SMALLINT,
    [BFP] SMALLINT,
    [GF] SMALLINT,
    [R] SMALLINT,
    [SH] SMALLINT,
    [SF] SMALLINT,
    [GIDP] SMALLINT,
    CONSTRAINT [PK_PitchingPost] PRIMARY KEY ([playerID], [yearID], [round])
);
GO

CREATE TABLE dbo.[Salaries] (
    [yearID] SMALLINT NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [playerID] NVARCHAR(20) NOT NULL,
    [salary] BIGINT,
    CONSTRAINT [PK_Salaries] PRIMARY KEY ([playerID], [teamID], [lgID], [yearID])
);
GO

CREATE TABLE dbo.[SeriesPost] (
    [yearID] SMALLINT NOT NULL,
    [round] NVARCHAR(10) NOT NULL,
    [teamIDwinner] NVARCHAR(4) NOT NULL,
    [lgIDwinner] NVARCHAR(3) NOT NULL,
    [teamIDloser] NVARCHAR(4),
    [lgIDloser] NVARCHAR(3),
    [wins] SMALLINT,
    [losses] SMALLINT,
    [ties] SMALLINT,
    CONSTRAINT [PK_SeriesPost] PRIMARY KEY ([teamIDwinner], [lgIDwinner], [yearID], [round])
);
GO

CREATE TABLE dbo.[TeamsHalf] (
    [yearID] SMALLINT NOT NULL,
    [lgID] NVARCHAR(3) NOT NULL,
    [teamID] NVARCHAR(4) NOT NULL,
    [Half] TINYINT NOT NULL,
    [divID] NVARCHAR(2),
    [DivWin] NVARCHAR(1),
    [Rank] TINYINT,
    [G] SMALLINT,
    [W] SMALLINT,
    [L] SMALLINT,
    CONSTRAINT [PK_TeamsHalf] PRIMARY KEY ([teamID], [lgID], [yearID], [Half])
);
GO

/* ---- Foreign keys ---- */

ALTER TABLE dbo.[Teams] ADD CONSTRAINT [FK_Teams_TeamsFranchises_franchID]
    FOREIGN KEY ([franchID])
    REFERENCES dbo.[TeamsFranchises] ([franchID]);
ALTER TABLE dbo.[AllstarFull] ADD CONSTRAINT [FK_AllstarFull_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Appearances] ADD CONSTRAINT [FK_Appearances_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Appearances] ADD CONSTRAINT [FK_Appearances_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[AwardsManagers] ADD CONSTRAINT [FK_AwardsManagers_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[AwardsPlayers] ADD CONSTRAINT [FK_AwardsPlayers_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[AwardsShareManagers] ADD CONSTRAINT [FK_AwardsShareManagers_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[AwardsSharePlayers] ADD CONSTRAINT [FK_AwardsSharePlayers_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Batting] ADD CONSTRAINT [FK_Batting_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Batting] ADD CONSTRAINT [FK_Batting_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[BattingPost] ADD CONSTRAINT [FK_BattingPost_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[BattingPost] ADD CONSTRAINT [FK_BattingPost_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[CollegePlaying] ADD CONSTRAINT [FK_CollegePlaying_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[CollegePlaying] ADD CONSTRAINT [FK_CollegePlaying_Schools_schoolID]
    FOREIGN KEY ([schoolID])
    REFERENCES dbo.[Schools] ([schoolID]);
ALTER TABLE dbo.[Fielding] ADD CONSTRAINT [FK_Fielding_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Fielding] ADD CONSTRAINT [FK_Fielding_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[FieldingOF] ADD CONSTRAINT [FK_FieldingOF_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[FieldingOFsplit] ADD CONSTRAINT [FK_FieldingOFsplit_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[FieldingOFsplit] ADD CONSTRAINT [FK_FieldingOFsplit_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[FieldingPost] ADD CONSTRAINT [FK_FieldingPost_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[FieldingPost] ADD CONSTRAINT [FK_FieldingPost_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[HallOfFame] ADD CONSTRAINT [FK_HallOfFame_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[HomeGames] ADD CONSTRAINT [FK_HomeGames_Parks_parkkey]
    FOREIGN KEY ([parkkey])
    REFERENCES dbo.[Parks] ([parkkey]);
ALTER TABLE dbo.[HomeGames] ADD CONSTRAINT [FK_HomeGames_Teams_teamkey_leaguekey_yearkey]
    FOREIGN KEY ([teamkey], [leaguekey], [yearkey])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[Managers] ADD CONSTRAINT [FK_Managers_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Managers] ADD CONSTRAINT [FK_Managers_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[ManagersHalf] ADD CONSTRAINT [FK_ManagersHalf_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[ManagersHalf] ADD CONSTRAINT [FK_ManagersHalf_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[Pitching] ADD CONSTRAINT [FK_Pitching_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Pitching] ADD CONSTRAINT [FK_Pitching_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[PitchingPost] ADD CONSTRAINT [FK_PitchingPost_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[PitchingPost] ADD CONSTRAINT [FK_PitchingPost_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[Salaries] ADD CONSTRAINT [FK_Salaries_People_playerID]
    FOREIGN KEY ([playerID])
    REFERENCES dbo.[People] ([playerID]);
ALTER TABLE dbo.[Salaries] ADD CONSTRAINT [FK_Salaries_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[SeriesPost] ADD CONSTRAINT [FK_SeriesPost_Teams_teamIDwinner_lgIDwinner_yearID]
    FOREIGN KEY ([teamIDwinner], [lgIDwinner], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[SeriesPost] ADD CONSTRAINT [FK_SeriesPost_Teams_teamIDloser_lgIDloser_yearID]
    FOREIGN KEY ([teamIDloser], [lgIDloser], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
ALTER TABLE dbo.[TeamsHalf] ADD CONSTRAINT [FK_TeamsHalf_Teams_teamID_lgID_yearID]
    FOREIGN KEY ([teamID], [lgID], [yearID])
    REFERENCES dbo.[Teams] ([teamID], [lgID], [yearID]);
GO
