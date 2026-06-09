-- Generated from lahman.db (SeriesPost)
CREATE TABLE "SeriesPost" (
    "yearID" smallint NOT NULL,
    "round" varchar(10) NOT NULL,
    "teamIDwinner" varchar(4) NOT NULL,
    "lgIDwinner" varchar(3) NOT NULL,
    "teamIDloser" varchar(4),
    "lgIDloser" varchar(3),
    "wins" smallint,
    "losses" smallint,
    "ties" smallint,
    CONSTRAINT "SeriesPost_pkey" PRIMARY KEY ("teamIDwinner", "lgIDwinner", "yearID", "round"),
    CONSTRAINT "SeriesPost_teamIDloser_lgIDloser_yearID_fkey" FOREIGN KEY ("teamIDloser", "lgIDloser", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "SeriesPost_teamIDwinner_lgIDwinner_yearID_fkey" FOREIGN KEY ("teamIDwinner", "lgIDwinner", "yearID") REFERENCES "Teams" ("teamID", "lgID", "yearID")
);
