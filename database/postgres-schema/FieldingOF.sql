-- Generated from lahman.db (FieldingOF)
CREATE TABLE "FieldingOF" (
    "playerID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    "stint" smallint NOT NULL,
    "Glf" smallint,
    "Gcf" smallint,
    "Grf" smallint,
    CONSTRAINT "FieldingOF_pkey" PRIMARY KEY ("playerID", "yearID", "stint"),
    CONSTRAINT "FieldingOF_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);
