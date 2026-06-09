-- Generated from lahman.db (CollegePlaying)
CREATE TABLE "CollegePlaying" (
    "playerID" varchar(20) NOT NULL,
    "schoolID" varchar(20) NOT NULL,
    "yearID" smallint NOT NULL,
    CONSTRAINT "CollegePlaying_pkey" PRIMARY KEY ("playerID", "schoolID", "yearID"),
    CONSTRAINT "CollegePlaying_schoolID_fkey" FOREIGN KEY ("schoolID") REFERENCES "Schools" ("schoolID"),
    CONSTRAINT "CollegePlaying_playerID_fkey" FOREIGN KEY ("playerID") REFERENCES "People" ("playerID")
);
