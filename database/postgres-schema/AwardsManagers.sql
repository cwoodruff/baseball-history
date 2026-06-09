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
