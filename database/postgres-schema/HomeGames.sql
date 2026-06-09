-- Generated from lahman.db (HomeGames)
CREATE TABLE "HomeGames" (
    "yearkey" integer NOT NULL,
    "leaguekey" varchar(3) NOT NULL,
    "teamkey" varchar(4) NOT NULL,
    "parkkey" varchar(20) NOT NULL,
    "spanfirst" varchar(10),
    "spanlast" varchar(10),
    "games" smallint,
    "openings" smallint,
    "attendance" integer,
    CONSTRAINT "HomeGames_pkey" PRIMARY KEY ("yearkey", "leaguekey", "teamkey", "parkkey"),
    CONSTRAINT "HomeGames_teamkey_leaguekey_yearkey_fkey" FOREIGN KEY ("teamkey", "leaguekey", "yearkey") REFERENCES "Teams" ("teamID", "lgID", "yearID"),
    CONSTRAINT "HomeGames_parkkey_fkey" FOREIGN KEY ("parkkey") REFERENCES "Parks" ("parkkey")
);
