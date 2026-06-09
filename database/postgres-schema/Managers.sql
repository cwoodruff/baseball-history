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
