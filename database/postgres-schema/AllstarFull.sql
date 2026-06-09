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
