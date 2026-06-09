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
