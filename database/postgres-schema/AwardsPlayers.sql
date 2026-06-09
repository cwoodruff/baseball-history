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
