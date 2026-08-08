# Project Context

- **Owner:** ripley
- **Project:** baseball-history
- **Role Summary:** Product/review lead: acceptance review, phase gating, and final sprint acceptance.

## Core Context

ripley has been contributing in their role: Product/review lead: acceptance review, phase gating, and final sprint acceptance. Key facts condensed: regression safety & guardrails are authoritative for sprint gates; shell/primitives extraction progressed under guarded reviews; Lahman Postgres export artifacts produced and validated (2026-06-08).

## Recent Updates


---

## Sprint 5 Orchestration Complete (2026-04-21)

**Status:** ✅ CLOSED

All Sprint 5 deliverables completed and verified:
- Dallas #14: Homepage/search/support pages migrated successfully
- Ash #15: Cache SOP documented, asset audit complete
- Lambert: Regression gate 344/344 PASS
- Ripley: Design review approved; orchestration documented

Test suite at 344/344. Repository ready for release.

## 2026-04-21 RHX/HTMX Audit

- **Live `rhx-*` usage is narrow.** Current `.cshtml` usage is `rhx-badge` across support/team/stats views plus a single `rhx-button` on `/About`; `_Layout.cshtml` only references `/_rhx/css/components/*` assets and is not itself a component-use site.
- **In this repo, `rhx-*` does not imply backend htmx wiring.** The shipped htmxRazor primitives in use are presentational; backend interaction still lives on surrounding anchors/forms/selects or shell-level `hx-boost`, so audits must verify the page surface contract rather than expecting `hx-*` on each badge/button.
- **Interactive surfaces containing migrated badges are already wired.** Stats leaderboards keep explicit `hx-get`/`hx-target`/partial-return paths, team pages keep modal links and htmx partial handlers intact, and shell-owned search remains `hx-get="/Search"` plus modal targeting through `#modal-container`. No missing backend htmx connection was found in live `rhx-*` component usage.

## GitHub Migration Closeout (2026-04-21)

**Status:** ✅ COMPLETE

All GitHub migration tracking issues (#4–#15) and umbrella issue (#16) closed. All 5 sprint milestones archived.

### Closeout Actions
- Closed all migration issues #4–#7 (Sprint 1: Foundation)
- Closed all migration issues #8–#9 (Sprint 2: Foundation Pages)
- Closed all migration issues #10–#11 (Sprint 3: Comparison & Features)
- Closed all migration issues #12–#13 (Sprint 4: Leaderboards)
- Closed all migration issues #14–#15 (Sprint 5: Polish & Documentation)
- Closed umbrella tracking issue #16
- Archived all 5 sprint milestones (Sprints 1–5)

### Migration Work Summary
The htmxRazor migration is complete across all pages:
1. Shared shell and primitives foundation
2. Player and Team foundation pages
3. Comparison and feature pages (Awards, HallOfFame, Postseason, Salaries)
4. Leaderboard pages (Batting, Pitching) with bug fixes
5. Homepage, search surfaces, and documentation

### Decisions Documented
- `.squad/decisions.md` — RHX/HTMX audit decision (no follow-up needed)
- `.squad/decisions.md` — Migration closeout decision
- `.squad/orchestration-log/2026-04-21T18:33:10Z-ripley.md` — orchestration log
- `.squad/log/2026-04-21T18:33:10Z-github-closeout.md` — session log

### Next Phase
**Validation and merge** — out of sprint scope. The repository is ready for:
1. Code review on `htmxRazor` branch
2. Testing and QA validation
3. Merge to main

No migration tracking issues remain open.

## 2026-04-27 Index Page EF Core Warning Resolution

**Status:** ✅ COMPLETE

Resolved EF Core warning on Index page regarding `First()`/`FirstOrDefault()` without deterministic ordering on grouped results.

### Details

- **Warning:** "... calling FirstOrDefault() without OrderBy on grouped result"
- **Root Cause:** Index.cshtml.cs was consuming a grouped result with non-deterministic First() selector
- **Resolution:** Added deterministic ordering before consuming grouped result

### Verification

- ✅ Build passed: zero warnings, zero errors
- ✅ All tests passing: 344/344 regression suite
- ✅ Code quality gates met

### Artifacts

- Orchestration log: `.squad/orchestration-log/2026-04-27T18:40:07Z-ripley.md`
- Session log: `.squad/log/2026-04-27T18:40:07Z-index-warning.md`

## 2026-06-08 Issue #18 Triage: Salaries Page Currency Formatting

**Status:** ✅ TRIAGED & ROUTED TO DALLAS

Triaged GitHub issue #18 (Salaries page missing dollar sign on salary amounts). Routed to Dallas as `squad:dallas` because this is a Razor UI formatting issue—display layer responsibility, not page logic or data retrieval. Issue has low risk and clear scope: add currency formatting across all salary display surfaces on Salaries page and related partials.

**Routing Rationale:** Display formatting is Dallas's domain (page markup, component composition, UX polish). No architectural decisions or cross-page contracts affected.

**Decision File:** `.squad/decisions/decisions.md` — Ripley: Issue #18 Triage & Routing

**Artifacts:**
- Orchestration log: `.squad/orchestration-log/2026-06-09T02:42:39Z-ripley.md`
- Session log: `.squad/log/2026-06-09T02:42:39Z-issue-18-triage.md`

---

## Learnings

### 2026-08-08 Leaderboard Qualification — Reconciliation & Execution Plan

**Context:** External feedback identified two related bugs: (1) rate-stat leaderboards default to "No minimum," surfacing 124 players batting 1.000 with 1-2 ABs; (2) the 3,000-AB career floor erases the Negro Leagues population because those leagues played 60-80 game schedules with partial surviving records.

**Key finding:** This exact problem was already analyzed. An approved design spec (`docs/superpowers/specs/2026-07-21-leaderboard-qualification-design.md`) and 6 open GitHub issues (#63-#66, #70, #75) already exist. Today's independent team investigation (Ash/Parker/Dallas/Lambert) confirmed the spec's approach and added implementation detail.

**Reconciliation:**
- **Spec's design is sound:** One shared query layer (`baseball-history-data` project), season-relative qualification (3.1 PA per team game for batting, 1 IP per team game for pitching), EF Core entity/context migration, `ILeaderboardQueryService` consumed by all three surfaces (Razor Pages, REST API, MCP). No conflicts with fresh findings.
- **Ash's data investigation confirmed:** `Teams.G` is fully populated (0 NULLs across 338 Negro Leagues team-seasons), PA can be derived NULL-safely, 95% of Negro Leagues players fall below 3,000-AB floor (Gibson 2,768 AB qualifies under season-relative rule). **Convergence with spec:** Ash proposed "no user toggle for qualification type," but the spec's `LeaderboardRequest` already supports `Qualified` flag + `MinAtBats` override — keep the override for researchers (Dallas's UX finding also supports this).
- **Parker's backend map confirmed:** 3 duplicate implementations (Pages, API, MCP) — spec's shared-service extraction is mandatory before any fix. Parker found the `UsesPlayingTimeTieBreaker` flag in MCP's stat catalog — **actionable addition:** migrate this to the shared stat catalog in the new data project.
- **Dallas's UX direction aligned:** "Qualified" as default for rate stats, "No minimum" for counting stats, visual badge for qualified players — **refinement needed:** Dallas proposed "Qualified" badge on qualified players; spec doesn't address UI badging — this is valid scope creep for #64 (UI default) as a presentational enhancement.
- **Lambert's test strategy hardened the gates:** 24 test scenarios with 5 hard merge blockers (golden-name smoke test, Negro Leagues inclusion proof, counting-stat regression, small-sample exclusion, explicit-parameter contract preservation). Lambert flagged cache invalidation risk: leaderboard results may be cached — **rollout gate:** prove cache invalidation strategy before deploy.
- **Issue #75 (OBP formula bug) is naturally bundled:** All three implementations compute OBP as `(H+BB)/(AB+BB)` — spec already corrects this to `(H+BB+HBP)/(AB+BB+HBP+SF)` in the shared query layer. No separate work stream needed; #75 closes when #63 lands.

**What's Already Decided (from spec + team confirmation):**
1. New `baseball-history-data` project holds entities, context, and `ILeaderboardQueryService`
2. Season-relative qualification (3.1 PA × Teams.G for batting, 1 IP × Teams.G for pitching) via `QualificationRules` static class
3. `LeaderboardRequest` record with `Qualified = true` default, `MinAtBats`/`MinInningsPitched` overrides
4. All three surfaces (Pages, API, MCP) rewired to the shared service; duplicate query logic deleted
5. OBP formula corrected as side effect (#75)
6. Rate stats qualified by default; counting stats unchanged

**Execution Sequence (issue-mapped rollout plan):**

**Phase 1: Foundation (#63) — BLOCKING for all others**
- **Owner:** Parker (backend lead; complex EF Core work)
- **Scope:** Create `baseball-history-data` project, move entities/context, implement `ILeaderboardQueryService` with `QualificationRules`, migrate stat catalog from MCP (including `UsesPlayingTimeTieBreaker`), rewire all three surfaces, delete duplicate implementations, correct OBP formula
- **Acceptance:** Lambert's 24 test scenarios pass, including golden-name smoke tests (#20-21), Negro Leagues inclusion proof, counting-stat regression, explicit-parameter preservation
- **Risk:** EF Core grouped-join translation to SQL — spec already flagged this; fallback is a keyless entity mapped to hand-written SQL view if needed
- **Merge blocker:** All 5 of Lambert's hard gates must pass

**Phase 2: UI Default & Override Control (#64) — depends on #63**
- **Owner:** Dallas (UI/UX)
- **Scope:** Change Batting/Pitching page dropdowns to default "Qualified" for rate stats, "No minimum" for counting stats; preserve override control; add "Qualified" badge (similar to HOF badge) on qualified players; add explanatory note
- **Acceptance:** Cobb/Hornsby/Gibson appear on page one of career AVG; override to "No minimum" restores 1.000 crowd; badge appears for qualified players
- **Risk:** Cache invalidation — if leaderboard results are cached, old "No minimum" default may serve stale responses post-deploy; Lambert flagged this as unknown risk needing verification
- **Merge blocker:** Manual smoke test post-deploy proves no stale cache (hit `/Stats/Batting?stat=avg` immediately after deploy, verify Gibson appears if qualified)

**Phase 3: API/MCP Parameter & Documentation (#65) — depends on #63**
- **Owner:** Ash (API/MCP/docs)
- **Scope:** Plumb `qualified` query param through API endpoints (default `true`), update MCP tool descriptions to document qualification default and override, update API OpenAPI/Scalar docs
- **Acceptance:** API `/api/leaders/batting?stat=avg&qualified=false` returns unqualified results; MCP tool description explains default behavior
- **Risk:** Low; purely additive parameter + doc work
- **Can run parallel to #64** (independent work streams)

**Phase 4: Regression Test Suite (#66) — depends on #63, gates final release**
- **Owner:** Lambert (test lead)
- **Scope:** Pin known aggregation totals (Bonds 762 HR, Aaron 755 W / 3,771 H, Ruth 714 HR, Young 511 W, Galvin 365 W, Mays 3,293 H / 660 HR including 1948 Birmingham Black Barons); expand Lambert's 24 test scenarios into full xUnit suite
- **Acceptance:** CI green with 350+ tests passing (current baseline 350/350); aggregation totals asserted
- **Risk:** Low; purely additive tests
- **Can run parallel to #64/#65** once #63 lands

**Out of Scope (not needed for the two feedback items):**
- Issue #70 (data scope statement for /About) — related to Negro Leagues visibility but not a bug fix
- Park factors / era adjustment — explicitly a non-goal per spec

**Open Questions for User (none — all decided):**
The spec + team findings converged on the approach. No open architectural decisions remain. The user can proceed with execution.

**Risks & Rollback Safety:**
1. **Cache invalidation (HIGH PRIORITY):** Dallas/Parker must verify whether leaderboard results are cached (grep for `IMemoryCache.Set` in `Batting.cshtml.cs`/`Pitching.cshtml.cs`). If yes, either (a) invalidate all leaderboard caches on deploy, (b) version cache keys (`batting_leaders_v2_{stat}`), or (c) accept 24h stale data. Lambert's manual smoke test post-deploy is the verification gate.
2. **EF Core translation risk (MEDIUM):** If grouped SUM join doesn't translate to SQL, fallback is a hand-written SQL view (spec already documents this).
3. **Contract preservation (LOW):** API response fields must match existing structure verbatim; Lambert's tests #22-24 assert this.
4. **Rollback:** If post-deploy smoke test fails, revert the PR and investigate cache strategy before retry. The shared-service extraction (#63) is all-or-nothing — partial rollout is not an option.

**Decision artifacts:**
- Execution plan: `.squad/decisions/inbox/ripley-qualification-plan.md`
- Orchestration: this learning entry
