-- Generated from lahman.db
-- Replay tables in dependency-safe order.

-- Generated from lahman.db (People)
CREATE TABLE "People" (
    "ID" integer NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "birthYear" integer,
    "birthMonth" integer,
    "birthDay" integer,
    "birthCity" varchar(100),
    "birthCountry" varchar(50),
    "birthState" varchar(50),
    "deathYear" integer,
    "deathMonth" integer,
    "deathDay" integer,
    "deathCountry" varchar(50),
    "deathState" varchar(50),
    "deathCity" varchar(100),
    "nameFirst" varchar(100),
    "nameLast" varchar(100),
    "nameGiven" varchar(100),
    "weight" varchar(10),
    "height" varchar(10),
    "bats" varchar(10),
    "throws" varchar(10),
    "debut" varchar(20),
    "bbrefID" varchar(20),
    "finalGame" varchar(20),
    "retroID" varchar(20),
    CONSTRAINT "People_pkey" PRIMARY KEY ("playerID")
);

-- Generated from lahman.db (Schools)
CREATE TABLE "Schools" (
    "schoolID" varchar(20) NOT NULL,
    "name_full" varchar(255),
    "city" varchar(100),
    "state" varchar(50),
    "country" varchar(50),
    CONSTRAINT "Schools_pkey" PRIMARY KEY ("schoolID")
);

-- Generated from lahman.db (Parks)
CREATE TABLE "Parks" (
    "ID" integer NOT NULL,
    "parkalias" varchar(512),
    "parkkey" varchar(20),
    "parkname" varchar(255),
    "city" varchar(100),
    "state" varchar(50),
    "country" varchar(50),
    CONSTRAINT "Parks_pkey" PRIMARY KEY ("ID"),
    CONSTRAINT "Parks_parkkey_key" UNIQUE ("parkkey")
);

-- Generated from lahman.db (TeamsFranchises)
CREATE TABLE "TeamsFranchises" (
    "franchID" varchar(4) NOT NULL,
    "franchName" varchar(100),
    "active" varchar(2),
    "NAassoc" varchar(4),
    CONSTRAINT "TeamsFranchises_pkey" PRIMARY KEY ("franchID")
);

-- Generated from lahman.db (AllstarFull)
CREATE TABLE "AllstarFull" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "gameNum" smallint,
    "gameID" varchar(20) NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "GP" smallint,
    "startingPos" varchar(10),
    CONSTRAINT "AllstarFull_pkey" PRIMARY KEY ("playerID", "yearID", "lgID", "teamID", "gameID"),
    CONSTRAINT "AllstarFull_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (AwardsManagers)
CREATE TABLE "AwardsManagers" (
    "playerID" varchar(20) NOT NULL,
    "awardID" varchar(255) NOT NULL,
    "yearID" smallint NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "tie" varchar(1) NOT NULL,
    "notes" varchar(255),
    CONSTRAINT "AwardsManagers_pkey" PRIMARY KEY ("playerID", "awardID", "yearID", "lgID", "tie"),
    CONSTRAINT "AwardsManagers_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (AwardsPlayers)
CREATE TABLE "AwardsPlayers" (
    "playerID" varchar(20) NOT NULL,
    "awardID" varchar(255) NOT NULL,
    "yearID" integer NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "tie" varchar(1) NOT NULL,
    "notes" varchar(255) NOT NULL,
    CONSTRAINT "AwardsPlayers_pkey" PRIMARY KEY ("playerID", "yearID", "awardID", "lgID", "tie", "notes"),
    CONSTRAINT "AwardsPlayers_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (AwardsShareManagers)
CREATE TABLE "AwardsShareManagers" (
    "awardID" varchar(255) NOT NULL,
    "yearID" smallint NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "pointsWon" smallint,
    "pointsMax" smallint,
    "votesFirst" smallint,
    CONSTRAINT "AwardsShareManagers_pkey" PRIMARY KEY ("playerID", "lgID", "yearID", "awardID"),
    CONSTRAINT "AwardsShareManagers_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (AwardsSharePlayers)
CREATE TABLE "AwardsSharePlayers" (
    "awardID" varchar(255) NOT NULL,
    "yearID" smallint NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "pointsWon" smallint,
    "pointsMax" smallint,
    "votesFirst" smallint,
    CONSTRAINT "AwardsSharePlayers_pkey" PRIMARY KEY ("playerID", "lgID", "yearID", "awardID"),
    CONSTRAINT "AwardsSharePlayers_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (FieldingOF)
CREATE TABLE "FieldingOF" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "Glf" smallint,
    "Gcf" smallint,
    "Grf" smallint,
    CONSTRAINT "FieldingOF_pkey" PRIMARY KEY ("playerID", "yearID", "stint"),
    CONSTRAINT "FieldingOF_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (HallOfFame)
CREATE TABLE "HallOfFame" (
    "playerID" varchar(20) NOT NULL,
    "yearid" smallint NOT NULL,
    "votedBy" varchar(255) NOT NULL,
    "ballots" smallint,
    "needed" smallint,
    "votes" smallint,
    "inducted" varchar(1),
    "category" varchar(100),
    "needed_note" varchar(1000),
    CONSTRAINT "HallOfFame_pkey" PRIMARY KEY ("playerID", "yearid", "votedBy"),
    CONSTRAINT "HallOfFame_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (CollegePlaying)
CREATE TABLE "CollegePlaying" (
    "playerID" varchar(20) NOT NULL,
    "schoolID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    CONSTRAINT "CollegePlaying_pkey" PRIMARY KEY ("playerID", "schoolID", "yearID"),
    CONSTRAINT "CollegePlaying_schoolID_fkey" FOREIGN KEY ("schoolID") REFERENCES "Schools" ("schoolID"),
    CONSTRAINT "CollegePlaying_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (Teams)
CREATE TABLE "Teams" (
    "yearID" smallint NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "franchID" varchar(4),
    "divID" varchar(2),
    "Rank" smallint,
    "G" smallint,
    "Ghome" smallint,
    "W" smallint,
    "L" smallint,
    "DivWin" varchar(1),
    "WCWin" varchar(1),
    "LgWin" varchar(1),
    "WSWin" varchar(1),
    "R" smallint,
    "AB" smallint,
    "H" smallint,
    "2B" smallint,
    "3B" smallint,
    "HR" smallint,
    "BB" smallint,
    "SO" smallint,
    "SB" smallint,
    "CS" smallint,
    "HBP" smallint,
    "SF" smallint,
    "RA" smallint,
    "ER" smallint,
    "ERA" double precision,
    "CG" smallint,
    "SHO" smallint,
    "SV" smallint,
    "IPouts" smallint,
    "HA" smallint,
    "HRA" smallint,
    "BBA" smallint,
    "SOA" smallint,
    "E" smallint,
    "DP" smallint,
    "FP" double precision,
    "name" varchar(100),
    "park" varchar(255),
    "attendance" integer,
    "BPF" smallint,
    "PPF" smallint,
    "teamIDBR" varchar(4),
    "teamIDlahman45" varchar(4),
    "teamIDretro" varchar(4),
    CONSTRAINT "Teams_pkey" PRIMARY KEY ("teamID", "lgID", "yearID"),
    CONSTRAINT "Teams_franchID_fkey" FOREIGN KEY ("franchID") REFERENCES "TeamsFranchises" ("franchID")
);

-- Generated from lahman.db (Appearances)
CREATE TABLE "Appearances" (
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "G_all" smallint,
    "GS" smallint,
    "G_batting" smallint,
    "G_defense" smallint,
    "G_p" smallint,
    "G_c" smallint,
    "G_1b" smallint,
    "G_2b" smallint,
    "G_3b" smallint,
    "G_ss" smallint,
    "G_lf" smallint,
    "G_cf" smallint,
    "G_rf" smallint,
    "G_of" smallint,
    "G_dh" smallint,
    "G_ph" smallint,
    "G_pr" smallint,
    CONSTRAINT "Appearances_pkey" PRIMARY KEY ("playerID", "lgID", "teamID", "yearID"),
    CONSTRAINT "Appearances_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Appearances_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (Batting)
CREATE TABLE "Batting" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "G" smallint,
    "AB" smallint,
    "R" smallint,
    "H" smallint,
    "2B" smallint,
    "3B" smallint,
    "HR" smallint,
    "RBI" smallint,
    "SB" smallint,
    "CS" smallint,
    "BB" smallint,
    "SO" smallint,
    "IBB" smallint,
    "HBP" smallint,
    "SH" smallint,
    "SF" smallint,
    "GIDP" smallint,
    CONSTRAINT "Batting_pkey" PRIMARY KEY ("playerID", "yearID", "stint", "teamID", "lgID"),
    CONSTRAINT "Batting_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Batting_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (BattingPost)
CREATE TABLE "BattingPost" (
    "yearID" smallint NOT NULL,
    "round" varchar(10) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "G" smallint,
    "AB" smallint,
    "R" smallint,
    "H" smallint,
    "2B" smallint,
    "3B" smallint,
    "HR" smallint,
    "RBI" smallint,
    "SB" smallint,
    "CS" smallint,
    "BB" smallint,
    "SO" smallint,
    "IBB" smallint,
    "HBP" smallint,
    "SH" smallint,
    "SF" smallint,
    "GIDP" smallint,
    CONSTRAINT "BattingPost_pkey" PRIMARY KEY ("yearID", "round", "playerID", "teamID", "lgID"),
    CONSTRAINT "BattingPost_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "BattingPost_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (Fielding)
CREATE TABLE "Fielding" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "POS" varchar(2) NOT NULL,
    "G" smallint,
    "GS" smallint,
    "InnOuts" smallint,
    "PO" smallint,
    "A" smallint,
    "E" smallint,
    "DP" smallint,
    "PB" smallint,
    "WP" smallint,
    "SB" smallint,
    "CS" smallint,
    "ZR" double precision,
    CONSTRAINT "Fielding_pkey" PRIMARY KEY ("playerID", "yearID", "stint", "teamID", "lgID", "POS"),
    CONSTRAINT "Fielding_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Fielding_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (FieldingOFsplit)
CREATE TABLE "FieldingOFsplit" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "POS" varchar(2) NOT NULL,
    "G" smallint,
    "GS" smallint,
    "InnOuts" smallint,
    "PO" smallint,
    "A" smallint,
    "E" smallint,
    "DP" smallint,
    "PB" smallint,
    "WP" smallint,
    "SB" smallint,
    "CS" smallint,
    "ZR" double precision,
    CONSTRAINT "FieldingOFsplit_pkey" PRIMARY KEY ("playerID", "yearID", "stint", "teamID", "lgID", "POS"),
    CONSTRAINT "FieldingOFsplit_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "FieldingOFsplit_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (FieldingPost)
CREATE TABLE "FieldingPost" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "round" varchar(10) NOT NULL,
    "POS" varchar(2) NOT NULL,
    "G" smallint,
    "GS" smallint,
    "InnOuts" smallint,
    "PO" smallint,
    "A" smallint,
    "E" smallint,
    "DP" smallint,
    "TP" smallint,
    "PB" smallint,
    "SB" smallint,
    "CS" smallint,
    CONSTRAINT "FieldingPost_pkey" PRIMARY KEY ("playerID", "yearID", "teamID", "lgID", "round", "POS"),
    CONSTRAINT "FieldingPost_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "FieldingPost_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (HomeGames)
CREATE TABLE "HomeGames" (
    "yearkey" integer NOT NULL,
    "leaguekey" varchar(3) NOT NULL,
    "teamkey" varchar(4) NOT NULL,
    "parkkey" varchar(20) NOT NULL,
    "spanfirst" varchar(10),
    "spanlast" varchar(10),
    "games" smallint,
    "openings" smallint,
    "attendance" integer,
    CONSTRAINT "HomeGames_pkey" PRIMARY KEY ("yearkey", "leaguekey", "teamkey", "parkkey"),
    CONSTRAINT "HomeGames_teamkey_leaguekey_yearkey_fkey" FOREIGN KEY ("teamkey", "leaguekey", "yearkey") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "HomeGames_parkkey_fkey" FOREIGN KEY ("parkkey") REFERENCES "Parks" ("parkkey")
);

-- Generated from lahman.db (Managers)
CREATE TABLE "Managers" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "inseason" smallint NOT NULL,
    "G" smallint,
    "W" smallint,
    "L" smallint,
    "rank" smallint,
    "plyrMgr" varchar(1),
    CONSTRAINT "Managers_pkey" PRIMARY KEY ("playerID", "yearID", "teamID", "lgID", "inseason"),
    CONSTRAINT "Managers_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Managers_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (ManagersHalf)
CREATE TABLE "ManagersHalf" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "inseason" smallint,
    "half" smallint NOT NULL,
    "G" smallint,
    "W" smallint,
    "L" smallint,
    "rank" smallint,
    CONSTRAINT "ManagersHalf_pkey" PRIMARY KEY ("playerID", "yearID", "teamID", "lgID", "half"),
    CONSTRAINT "ManagersHalf_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "ManagersHalf_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (Pitching)
CREATE TABLE "Pitching" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "teamID" varchar(4),
    "lgID" varchar(3),
    "W" smallint,
    "L" smallint,
    "G" smallint,
    "GS" smallint,
    "CG" smallint,
    "SHO" smallint,
    "SV" smallint,
    "IPouts" smallint,
    "H" smallint,
    "ER" smallint,
    "HR" smallint,
    "BB" smallint,
    "SO" smallint,
    "BAOpp" double precision,
    "ERA" double precision,
    "IBB" smallint,
    "WP" smallint,
    "HBP" smallint,
    "BK" smallint,
    "BFP" smallint,
    "GF" smallint,
    "R" smallint,
    "SH" smallint,
    "SF" smallint,
    "GIDP" smallint,
    CONSTRAINT "Pitching_pkey" PRIMARY KEY ("playerID", "yearID", "stint"),
    CONSTRAINT "Pitching_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Pitching_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (PitchingPost)
CREATE TABLE "PitchingPost" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "round" varchar(10) NOT NULL,
    "teamID" varchar(4),
    "lgID" varchar(3),
    "W" smallint,
    "L" smallint,
    "G" smallint,
    "GS" smallint,
    "CG" smallint,
    "SHO" smallint,
    "SV" smallint,
    "IPouts" integer,
    "H" smallint,
    "ER" smallint,
    "HR" smallint,
    "BB" smallint,
    "SO" smallint,
    "BAOpp" double precision,
    "ERA" double precision,
    "IBB" smallint,
    "WP" smallint,
    "HBP" smallint,
    "BK" smallint,
    "BFP" smallint,
    "GF" smallint,
    "R" smallint,
    "SH" smallint,
    "SF" smallint,
    "GIDP" smallint,
    CONSTRAINT "PitchingPost_pkey" PRIMARY KEY ("playerID", "yearID", "round"),
    CONSTRAINT "PitchingPost_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "PitchingPost_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (Salaries)
CREATE TABLE "Salaries" (
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "salary" bigint,
    CONSTRAINT "Salaries_pkey" PRIMARY KEY ("playerID", "teamID", "lgID", "yearID"),
    CONSTRAINT "Salaries_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Salaries_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);

-- Generated from lahman.db (SeriesPost)
CREATE TABLE "SeriesPost" (
    "yearID" smallint NOT NULL,
    "round" varchar(10) NOT NULL,
    "teamIDwinner" varchar(4) NOT NULL,
    "lgIDwinner" varchar(3) NOT NULL,
    "teamIDloser" varchar(4),
    "lgIDloser" varchar(3),
    "wins" smallint,
    "losses" smallint,
    "ties" smallint,
    CONSTRAINT "SeriesPost_pkey" PRIMARY KEY ("teamIDwinner", "lgIDwinner", "yearID", "round"),
    CONSTRAINT "SeriesPost_teamIDloser_lgIDloser_yearID_fkey" FOREIGN KEY ("teamIDloser", "lgIDloser", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "SeriesPost_teamIDwinner_lgIDwinner_yearID_fkey" FOREIGN KEY ("teamIDwinner", "lgIDwinner", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID")
);

-- Generated from lahman.db (TeamsHalf)
CREATE TABLE "TeamsHalf" (
    "yearID" smallint NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "Half" smallint NOT NULL,
    "divID" varchar(2),
    "DivWin" varchar(1),
    "Rank" smallint,
    "G" smallint,
    "W" smallint,
    "L" smallint,
    CONSTRAINT "TeamsHalf_pkey" PRIMARY KEY ("teamID", "lgID", "yearID", "Half"),
    CONSTRAINT "TeamsHalf_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID")
);
