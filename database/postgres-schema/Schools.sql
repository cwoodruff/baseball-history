-- Generated from lahman.db (Schools)
CREATE TABLE "Schools" (
    "schoolID" varchar(20) NOT NULL,
    "name_full" varchar(255),
    "city" varchar(100),
    "state" varchar(50),
    "country" varchar(50),
    CONSTRAINT "Schools_pkey" PRIMARY KEY ("schoolID")
);
