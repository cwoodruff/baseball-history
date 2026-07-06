-- Trigram indexes so the global search's ILIKE '%term%' filters can use an
-- index instead of scanning People/TeamsFranchises on every keystroke.
-- Idempotent: safe to run repeatedly. Requires the pg_trgm contrib extension.

CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX IF NOT EXISTS idx_people_namefirst_trgm
    ON "People" USING gin ("nameFirst" gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_people_namelast_trgm
    ON "People" USING gin ("nameLast" gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_people_fullname_trgm
    ON "People" USING gin (("nameFirst" || ' ' || "nameLast") gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_teamsfranchises_franchname_trgm
    ON "TeamsFranchises" USING gin ("franchName" gin_trgm_ops);
