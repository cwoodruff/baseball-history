-- ============================================================================
-- Baseball History Project — shared query layer
-- Season-relative qualification + league-index rate stats (PostgreSQL)
--
-- Design goal: one code path. The website, the public API, and the MCP server
-- all read these views, so they cannot diverge on what "qualified" means.
--
-- ASSUMPTIONS (adjust to your migration):
--   * Lowercase unquoted identifiers (playerid, yearid, lgid, teamid).
--   * Doubles/triples: Lahman ships them as 2B/3B, which are not legal bare
--     identifiers. The stint CTE below aliases them once; change that one spot
--     if your migration renamed them to doubles/triples.
--   * hbp / sf / sh are NULL across large stretches of pre-1954 data. Every
--     reference is COALESCEd. Do NOT skip this — a NULL sf silently nulls PA.
-- ============================================================================


-- ----------------------------------------------------------------------------
-- 1. Team schedule length — the shared denominator
-- ----------------------------------------------------------------------------
-- This is the whole trick. 3.1 PA/team-game means a 154-game 1927 season, a
-- 162-game 1998 season, and a 74-game 1943 Negro National League season each
-- get their own threshold, instead of one flat career floor that erases the
-- short-schedule leagues entirely.

CREATE OR REPLACE VIEW v_team_games AS
SELECT
    t.yearid,
    t.lgid,
    t.teamid,
    t.g AS team_games
FROM teams t
WHERE t.g IS NOT NULL
  AND t.g > 0;

COMMENT ON VIEW v_team_games IS
  'Games played per team-season. Denominator for all season-relative qualification.';


-- ----------------------------------------------------------------------------
-- 2. Player-season batting, stints collapsed, with a qualification threshold
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_player_season_batting AS
WITH stint AS (
    SELECT
        b.playerid,
        b.yearid,
        b.stint,
        b.teamid,
        b.lgid,
        b.g,
        b.ab,
        b.r,
        b.h,
        b."2B"          AS doubles,   -- <- rename here if your schema differs
        b."3B"          AS triples,   -- <- rename here if your schema differs
        b.hr,
        b.rbi,
        b.sb,
        b.cs,
        b.bb,
        b.so,
        COALESCE(b.hbp, 0) AS hbp,
        COALESCE(b.sf,  0) AS sf,
        COALESCE(b.sh,  0) AS sh
    FROM batting b
),
joined AS (
    SELECT
        s.*,
        tg.team_games
    FROM stint s
    LEFT JOIN v_team_games tg
           ON tg.yearid = s.yearid
          AND tg.teamid = s.teamid
)
SELECT
    j.playerid,
    j.yearid,
    -- For split seasons, attribute the season to the league where he played most.
    (mode() WITHIN GROUP (ORDER BY j.lgid))                    AS lgid,
    SUM(j.g)                                                   AS g,
    SUM(j.ab)                                                  AS ab,
    SUM(j.r)                                                   AS r,
    SUM(j.h)                                                   AS h,
    SUM(j.doubles)                                             AS doubles,
    SUM(j.triples)                                             AS triples,
    SUM(j.hr)                                                  AS hr,
    SUM(j.rbi)                                                 AS rbi,
    SUM(j.sb)                                                  AS sb,
    SUM(j.cs)                                                  AS cs,
    SUM(j.bb)                                                  AS bb,
    SUM(j.so)                                                  AS so,
    SUM(j.hbp)                                                 AS hbp,
    SUM(j.sf)                                                  AS sf,
    SUM(j.sh)                                                  AS sh,

    SUM(j.ab + j.bb + j.hbp + j.sf + j.sh)                     AS pa,
    SUM(j.h + j.doubles + 2 * j.triples + 3 * j.hr)            AS tb,

    -- Schedule length weighted by where he actually played that year.
    -- A player who splits 40 G on a 154-game club and 100 G on a 162-game club
    -- gets a blended denominator rather than whichever team happened to sort first.
    ROUND(
        SUM(j.team_games::numeric * j.g) / NULLIF(SUM(j.g), 0)
    , 1)                                                       AS team_games_wtd,

    -- The MLB qualification rule, applied season-relative.
    CEIL(3.1 * SUM(j.team_games::numeric * j.g) / NULLIF(SUM(j.g), 0))
                                                               AS pa_threshold
FROM joined j
GROUP BY j.playerid, j.yearid;

COMMENT ON VIEW v_player_season_batting IS
  'One row per player-season. pa_threshold = 3.1 PA x weighted team games.';


-- ----------------------------------------------------------------------------
-- 3. League-season context
-- ----------------------------------------------------------------------------
-- Materialized because it never changes for a closed season and every rate-stat
-- page joins to it. REFRESH after an annual data load.
--
-- NOTE: baseball-reference excludes pitchers from the league batting context for
-- OPS+. Doing that here needs a position filter off Appearances/Fielding; until
-- then this is a whole-league baseline and the numbers will run a few points
-- lower than bbref's. Say so in the glossary rather than quietly differing.

CREATE MATERIALIZED VIEW IF NOT EXISTS mv_league_batting_season AS
SELECT
    ps.yearid,
    ps.lgid,
    SUM(ps.pa)                                                  AS lg_pa,
    SUM(ps.ab)                                                  AS lg_ab,
    SUM(ps.h)                                                   AS lg_h,
    SUM(ps.bb)                                                  AS lg_bb,
    SUM(ps.hbp)                                                 AS lg_hbp,
    SUM(ps.sf)                                                  AS lg_sf,
    SUM(ps.tb)                                                  AS lg_tb,

    (SUM(ps.h)::numeric / NULLIF(SUM(ps.ab), 0))                AS lg_avg,
    ((SUM(ps.h) + SUM(ps.bb) + SUM(ps.hbp))::numeric
        / NULLIF(SUM(ps.ab) + SUM(ps.bb) + SUM(ps.hbp) + SUM(ps.sf), 0))
                                                                AS lg_obp,
    (SUM(ps.tb)::numeric / NULLIF(SUM(ps.ab), 0))               AS lg_slg
FROM v_player_season_batting ps
GROUP BY ps.yearid, ps.lgid;

CREATE UNIQUE INDEX IF NOT EXISTS ux_lg_bat_season
    ON mv_league_batting_season (yearid, lgid);


-- ----------------------------------------------------------------------------
-- 4. Season rate stats, indexed to league
-- ----------------------------------------------------------------------------
-- ops_index is the bbref OPS+ formula minus the park factor. Lahman has no
-- home/road run splits, so a real park factor is not derivable from this data —
-- do not label this column "OPS+". Call it "OPS vs. League" and let the glossary
-- explain the missing adjustment. That honesty is a feature on a history site.

CREATE OR REPLACE VIEW v_player_season_rates AS
SELECT
    ps.playerid,
    ps.yearid,
    ps.lgid,
    ps.g,
    ps.pa,
    ps.ab,
    ps.h,
    ps.hr,
    ps.tb,
    ps.team_games_wtd,
    ps.pa_threshold,
    (ps.pa >= ps.pa_threshold)                                  AS qualified,

    ROUND(ps.h::numeric      / NULLIF(ps.ab, 0), 4)             AS avg,
    ROUND((ps.h + ps.bb + ps.hbp)::numeric
        / NULLIF(ps.ab + ps.bb + ps.hbp + ps.sf, 0), 4)         AS obp,
    ROUND(ps.tb::numeric     / NULLIF(ps.ab, 0), 4)             AS slg,
    ROUND((ps.tb - ps.h)::numeric / NULLIF(ps.ab, 0), 4)        AS iso,
    ROUND((ps.h - ps.hr)::numeric
        / NULLIF(ps.ab - ps.so - ps.hr + ps.sf, 0), 4)          AS babip,
    ROUND(100.0 * ps.bb      / NULLIF(ps.pa, 0), 1)             AS bb_pct,
    ROUND(100.0 * ps.so      / NULLIF(ps.pa, 0), 1)             AS k_pct,

    lg.lg_obp,
    lg.lg_slg,

    -- 100 = exactly league average. Park-neutral, NOT park-adjusted.
    ROUND(100 * (
          ((ps.h + ps.bb + ps.hbp)::numeric
              / NULLIF(ps.ab + ps.bb + ps.hbp + ps.sf, 0)) / NULLIF(lg.lg_obp, 0)
        + (ps.tb::numeric / NULLIF(ps.ab, 0)) / NULLIF(lg.lg_slg, 0)
        - 1
    ))                                                          AS ops_index,

    -- Short-schedule normalization. This is what makes a 74-game Negro Leagues
    -- season legible next to a 154-game one without pretending they're the same.
    ROUND(ps.hr * 162.0 / NULLIF(ps.team_games_wtd, 0), 1)      AS hr_per_162,
    ROUND(ps.h  * 162.0 / NULLIF(ps.team_games_wtd, 0), 1)      AS h_per_162,
    ROUND(ps.rbi * 162.0 / NULLIF(ps.team_games_wtd, 0), 1)     AS rbi_per_162
FROM v_player_season_batting ps
LEFT JOIN mv_league_batting_season lg
       ON lg.yearid = ps.yearid
      AND lg.lgid   = ps.lgid;


-- ----------------------------------------------------------------------------
-- 5. Career totals with a derived career threshold
-- ----------------------------------------------------------------------------
-- Career qualification = the sum of the season thresholds his teams actually
-- faced, not a flat 3,000 AB. Gibson's ~2,400 documented ABs clear a threshold
-- built from 60–80 game schedules; they never clear 3,000.
--
-- Career league context is PA-weighted, not an average of season indexes. A
-- player's rate is compared to the league he actually batted in, weighted by
-- how much he batted there.

CREATE OR REPLACE VIEW v_career_batting AS
WITH career AS (
    SELECT
        ps.playerid,
        MIN(ps.yearid)                          AS first_year,
        MAX(ps.yearid)                          AS last_year,
        COUNT(*)                                AS seasons,
        SUM(ps.g)                               AS g,
        SUM(ps.pa)                              AS pa,
        SUM(ps.ab)                              AS ab,
        SUM(ps.h)                               AS h,
        SUM(ps.doubles)                         AS doubles,
        SUM(ps.triples)                         AS triples,
        SUM(ps.hr)                              AS hr,
        SUM(ps.rbi)                             AS rbi,
        SUM(ps.bb)                              AS bb,
        SUM(ps.so)                              AS so,
        SUM(ps.hbp)                             AS hbp,
        SUM(ps.sf)                              AS sf,
        SUM(ps.tb)                              AS tb,
        SUM(ps.pa_threshold)                    AS career_pa_threshold,

        SUM(lg.lg_obp * ps.pa) / NULLIF(SUM(ps.pa), 0) AS career_lg_obp,
        SUM(lg.lg_slg * ps.pa) / NULLIF(SUM(ps.pa), 0) AS career_lg_slg
    FROM v_player_season_batting ps
    LEFT JOIN mv_league_batting_season lg
           ON lg.yearid = ps.yearid
          AND lg.lgid   = ps.lgid
    GROUP BY ps.playerid
)
SELECT
    c.*,
    (c.pa >= c.career_pa_threshold)                             AS qualified,
    ROUND(100.0 * c.pa / NULLIF(c.career_pa_threshold, 0), 1)   AS pct_of_threshold,

    ROUND(c.h::numeric / NULLIF(c.ab, 0), 4)                    AS avg,
    ROUND((c.h + c.bb + c.hbp)::numeric
        / NULLIF(c.ab + c.bb + c.hbp + c.sf, 0), 4)             AS obp,
    ROUND(c.tb::numeric / NULLIF(c.ab, 0), 4)                   AS slg,

    ROUND(100 * (
          ((c.h + c.bb + c.hbp)::numeric
              / NULLIF(c.ab + c.bb + c.hbp + c.sf, 0)) / NULLIF(c.career_lg_obp, 0)
        + (c.tb::numeric / NULLIF(c.ab, 0)) / NULLIF(c.career_lg_slg, 0)
        - 1
    ))                                                          AS ops_index
FROM career c;


-- ----------------------------------------------------------------------------
-- 6. Pitching equivalent — 1 IP per team game
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_player_season_pitching AS
WITH stint AS (
    SELECT
        p.playerid, p.yearid, p.teamid, p.lgid,
        p.w, p.l, p.g, p.gs, p.ipouts, p.h, p.er, p.hr, p.bb, p.so,
        COALESCE(p.hbp, 0) AS hbp,
        COALESCE(p.bfp, 0) AS bfp
    FROM pitching p
),
joined AS (
    SELECT s.*, tg.team_games
    FROM stint s
    LEFT JOIN v_team_games tg
           ON tg.yearid = s.yearid AND tg.teamid = s.teamid
)
SELECT
    j.playerid,
    j.yearid,
    (mode() WITHIN GROUP (ORDER BY j.lgid))                     AS lgid,
    SUM(j.w) AS w, SUM(j.l) AS l, SUM(j.g) AS g, SUM(j.gs) AS gs,
    SUM(j.ipouts) AS ipouts,
    ROUND(SUM(j.ipouts)::numeric / 3, 1)                        AS ip,
    SUM(j.h) AS h, SUM(j.er) AS er, SUM(j.hr) AS hr,
    SUM(j.bb) AS bb, SUM(j.so) AS so, SUM(j.hbp) AS hbp,
    ROUND(SUM(j.team_games::numeric * j.g) / NULLIF(SUM(j.g), 0), 1)
                                                                AS team_games_wtd,
    -- 1 IP per team game = 3 outs per team game.
    CEIL(3.0 * SUM(j.team_games::numeric * j.g) / NULLIF(SUM(j.g), 0))
                                                                AS ipouts_threshold,
    (SUM(j.ipouts) >= CEIL(3.0 * SUM(j.team_games::numeric * j.g)
        / NULLIF(SUM(j.g), 0)))                                 AS qualified,
    ROUND(9.0 * SUM(j.er) / NULLIF(SUM(j.ipouts)::numeric / 3, 0), 2)  AS era,
    ROUND((SUM(j.h) + SUM(j.bb))::numeric
        / NULLIF(SUM(j.ipouts)::numeric / 3, 0), 3)             AS whip,
    ROUND(9.0 * SUM(j.so) / NULLIF(SUM(j.ipouts)::numeric / 3, 0), 2)  AS k9,
    ROUND(9.0 * SUM(j.bb) / NULLIF(SUM(j.ipouts)::numeric / 3, 0), 2)  AS bb9
FROM joined j
GROUP BY j.playerid, j.yearid;


-- ----------------------------------------------------------------------------
-- 7. Indexes
-- ----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS ix_batting_player_year  ON batting  (playerid, yearid);
CREATE INDEX IF NOT EXISTS ix_batting_year_team    ON batting  (yearid, teamid);
CREATE INDEX IF NOT EXISTS ix_pitching_player_year ON pitching (playerid, yearid);
CREATE INDEX IF NOT EXISTS ix_pitching_year_team   ON pitching (yearid, teamid);
CREATE INDEX IF NOT EXISTS ix_teams_year_team      ON teams    (yearid, teamid);


-- ============================================================================
-- Sanity checks — run these before wiring anything to the API
-- ============================================================================

-- A. Career AVG leaderboard, qualified. Cobb and Hornsby should top it.
--    Under the old flat floor this page was ~100 players hitting 1.000.
--
-- SELECT p.namefirst || ' ' || p.namelast AS player,
--        c.avg, c.pa, c.career_pa_threshold, c.ops_index
-- FROM v_career_batting c
-- JOIN people p USING (playerid)
-- WHERE c.qualified
-- ORDER BY c.avg DESC
-- LIMIT 25;

-- B. Do the Negro Leagues stars qualify now? Gibson, Charleston, Stearnes,
--    Suttles should all come back qualified = true.
--
-- SELECT p.namefirst || ' ' || p.namelast AS player,
--        c.pa, c.career_pa_threshold, c.pct_of_threshold, c.qualified, c.avg
-- FROM v_career_batting c
-- JOIN people p USING (playerid)
-- WHERE p.namelast IN ('Gibson','Charleston','Stearnes','Suttles')
-- ORDER BY c.pa DESC;

-- C. Nobody with a handful of PA survives the default filter.
--
-- SELECT COUNT(*) AS should_be_zero
-- FROM v_career_batting
-- WHERE qualified AND pa < 100;

-- D. Aggregation spot-checks that must not move (from the SABR review).
--    Bonds 762 HR, Aaron 755 HR / 3771 H, Ruth 714 HR, Mays 3293 H / 660 HR.
--
-- SELECT p.namefirst || ' ' || p.namelast AS player, c.h, c.hr
-- FROM v_career_batting c
-- JOIN people p USING (playerid)
-- WHERE p.playerid IN ('bondsba01','aaronha01','ruthba01','mayswi01')
-- ORDER BY c.hr DESC;


-- ============================================================================
-- KNOWN CAVEATS — put these in the glossary, not just in a comment
-- ============================================================================
-- 1. No park factors. Lahman carries no home/road run splits, so ops_index is
--    league-relative only. Do not ship it under the name "OPS+".
-- 2. League baseline includes pitchers, so ops_index will read a few points
--    below bbref's OPS+ for the same season. Refine with a position filter.
-- 3. Teams.g for Negro Leagues clubs may reflect *documented* games rather than
--    scheduled ones. Where documentation is thin, team_games is understated and
--    the threshold is correspondingly lenient. Verify against Seamheads schedule
--    data before treating these thresholds as authoritative.
-- 4. Pre-1954 sf and pre-1887 hbp are absent, so early OBP is approximate. This
--    is a property of the historical record, not a bug — say so on the page.
-- 5. mv_league_batting_season needs REFRESH MATERIALIZED VIEW after each load.
