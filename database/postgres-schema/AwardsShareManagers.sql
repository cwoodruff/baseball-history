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
