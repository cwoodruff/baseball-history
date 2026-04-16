# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Migration scope & architecture | Ripley | htmxRazor adoption strategy, cross-page rollout, reviewer gating |
| Razor UI & components | Dallas | Page markup, partial replacement, component composition, UX polish |
| ASP.NET Core page logic | Parker | PageModels, handlers, API wiring, service integration |
| Testing & review | Lambert | Regression checks, edge cases, reviewer feedback, test gaps |
| Data & runtime integration | Ash | EF queries, caching behavior, DB/runtime impact, platform checks |
| Code review | Ripley | Review PRs, check quality, suggest improvements |
| Testing | Lambert | Write tests, find edge cases, verify fixes |
| Scope & priorities | Ripley | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Lead |
| `squad:ripley` | Pick up issue and complete the work | Ripley |
| `squad:dallas` | Pick up issue and complete the work | Dallas |
| `squad:parker` | Pick up issue and complete the work | Parker |
| `squad:lambert` | Pick up issue and complete the work | Lambert |
| `squad:ash` | Pick up issue and complete the work | Ash |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the **Lead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn the tester to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. The Lead handles all `squad` (base label) triage.
8. **htmxRazor migration work** — default to Ripley + Dallas + Parker, and add Lambert for regression review when behavior changes.
