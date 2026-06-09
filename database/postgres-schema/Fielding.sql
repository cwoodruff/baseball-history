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
