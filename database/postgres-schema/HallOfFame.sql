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
