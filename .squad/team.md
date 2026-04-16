# Squad Team

> Baseball History — an ASP.NET Core Razor Pages application migrating toward htmxRazor web components.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. Does not generate domain artifacts. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Ripley | Lead | `.squad/agents/ripley/charter.md` | ✅ Active |
| Dallas | Frontend Dev | `.squad/agents/dallas/charter.md` | ✅ Active |
| Parker | Backend Dev | `.squad/agents/parker/charter.md` | ✅ Active |
| Lambert | Tester | `.squad/agents/lambert/charter.md` | ✅ Active |
| Ash | Data/Platform | `.squad/agents/ash/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**
- Bug fixes with clear reproduction steps
- Test coverage additions and flaky test cleanup
- Small isolated features with clear acceptance criteria
- Boilerplate scaffolding and docs updates

**🟡 Needs review — route to @copilot but flag for squad member PR review:**
- Medium-size feature work following established patterns
- Refactoring with existing test coverage
- Endpoint additions that fit current conventions

**🔴 Not suitable — route to squad member instead:**
- Architecture and migration decisions
- Cross-cutting htmxRazor adoption work
- Security- or performance-critical changes
- Ambiguous requirements needing clarification

## Project Context

- **Owner:** Woody
- **Project:** baseball-history
- **Stack:** C#, .NET 10, ASP.NET Core Razor Pages, Entity Framework Core, SQLite, htmx, Bootstrap 5, htmxRazor
- **Description:** Explore historical baseball data from the Lahman database and migrate the existing UI patterns toward htmxRazor web components.
- **Created:** 2026-04-16T10:57:49Z
