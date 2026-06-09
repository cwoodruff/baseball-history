-- Generated from lahman.db (Salaries)
CREATE TABLE "Salaries" (
    "yearID" smallint NOT NULL,
    "teamID" varchar(4) NOT NULL,
    "lgID" varchar(3) NOT NULL,
    "playerID" varchar(20) NOT NULL,
    "salary" bigint,
    CONSTRAINT "Salaries_pkey" PRIMARY KEY ("playerID", "teamID", "lgID", "yearID"),
    CONSTRAINT "Salaries_teamID_lgID_yearID_fkey" FOREIGN KEY ("teamID", "lgID", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "Salaries_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);
