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
