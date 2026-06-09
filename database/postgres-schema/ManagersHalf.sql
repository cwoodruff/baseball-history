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
