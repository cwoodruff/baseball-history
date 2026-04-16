---
date: 2026-04-16
for_team: true
purpose: Quick reference for Sprint 1 execution
---

# Sprint 1 Summary: htmxRazor Foundation (Quick Reference)

## The Order
1. **#4** (Parker, 3–5d) — Prove htmxRazor with one component on About.cshtml → unblocks #5/#6/#7
2. **#5** (Lambert, parallel with #6 after #4) — 19 PageModel tests + 8+ integration tests + edge cases
3. **#6** (Dallas, sequential in her work) — Rework _Layout, nav, footer, search, modal
4. **#7** (Dallas, after #6) — Rework Pagination, AlphabetNav, extract FilterForm, cards, loading spinner

## Why This Works
- **#4 is 90% done** — htmxRazor already wired, just needs one component + doc comment
- **#5 gates #6/#7** — Regression tests provide immediate feedback as shell/primitives change
- **#6 + #7 coordinated** — Same owner, shell first, primitives build on it
- **Parallel #5 + #6** — No waiting; Lambert tests Dallas's shell changes in real time

## Critical Risks (What Can Go Wrong)
- **#4 breaks:** Revert, narrow scope, defer Sprint 1
- **#5 insufficient:** Add tests post-hoc if Lambert discovers critical gaps after #6/#7
- **#6 breaks nav/modal:** Revert, break into smaller pieces (nav only, then footer, etc.)
- **#7 breaks Pagination:** Backwards-compat layer — wrap old Pagination in new component

## After Sprint 1 (Ready for Feature Teams)
- htmxRazor integration proven ✓
- Regression safety net in place ✓
- Shared primitives modernized ✓
- Feature migrations (#8–#15) can begin (Players, Teams, Compare, Stats, etc.)

## Success Criteria (All or Nothing)
- [ ] #4 + #5 + #6 + #7 all merged
- [ ] All tests green
- [ ] No regressions vs. main
- [ ] Feature pages still render (Bootstrap interop preserved)
- [ ] Shared components have stable interfaces

---

**Documented in full at:** `.squad/decisions/inbox/ripley-sprint1-brief.md`  
**Approval:** Ripley (ready for Parker to start)
