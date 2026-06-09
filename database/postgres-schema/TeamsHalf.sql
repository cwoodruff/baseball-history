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
