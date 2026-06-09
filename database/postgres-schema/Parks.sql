-- Generated from lahman.db (Parks)
CREATE TABLE "Parks" (
    "ID" integer NOT NULL,
    "parkalias" varchar(512),
    "parkkey" varchar(20),
    "parkname" varchar(255),
    "city" varchar(100),
    "state" varchar(50),
    "country" varchar(50),
    CONSTRAINT "Parks_pkey" PRIMARY KEY ("ID"),
    CONSTRAINT "Parks_parkkey_key" UNIQUE ("parkkey")
);
