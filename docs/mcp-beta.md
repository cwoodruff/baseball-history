# MCP Server Beta Program

Materials for recruiting and running the early MCP beta (issue #74):
the invitation note, the test checklist, the feedback channel, and the
credits commitment.

> **Before sending invitations, decide the access path.** The MCP server is
> not yet deployed anywhere public — only the web app runs in Azure. Either
> (a) deploy the MCP host and put its URL in the invitation, or (b) limit
> the first wave to technical testers who can run it locally (.NET 10 SDK
> plus a PostgreSQL loaded with Lahman data — a real setup cost). The
> invitation below is written for (a) with a placeholder URL; the local
> path is included as an appendix for technical testers.

---

## Invitation note (email-ready)

Subject: **Early access: ask an AI assistant about 150 years of baseball — beta testers wanted**

Hi <name>,

You looked at Baseball History (<site-url>) in depth earlier this year, and
your feedback made the site better — so I'd like to invite you to be one of
the first outside testers of its newest piece.

I've built a Model Context Protocol (MCP) server on top of the same
database. MCP is the emerging standard for connecting AI assistants like
Claude to real data sources: instead of answering baseball questions from
memory (and often getting them wrong), an assistant connected to this
server looks up the actual record — 24,000+ players from 1871 to 2025,
including the Negro Leagues, with the site's same honesty about what
survived and what didn't.

What you'd do as a beta tester:

- Connect your MCP-capable client (Claude Desktop is the easiest; I'll
  send a two-line config) to the server at <mcp-url>.
- Spend an hour or two asking it real research questions — the kind you'd
  normally answer with your own references — and note where it shines,
  where it stumbles, and where a tool's output made you distrust it.
- Work through a short checklist I'll send along (about 15 scenarios), or
  ignore the checklist entirely and just explore; both are useful.
- Report what you find on the project's GitHub issues page (or just reply
  to this email if that's easier — I'll file the issues).

The commitment is small — an hour or two, once — and every tester gets
named (with your permission) in the project's credits. If the setup gives
you any trouble at all, that's a finding too, and I want to hear it.

Interested? Reply and I'll send the config and checklist.

Thanks,
Chris

---

## Beta test checklist

Ask your assistant these in your own words — natural phrasing is part of
the test. For each: did it pick a sensible tool, was the answer correct
against your own references, and did the response say anything misleading?

**Getting oriented**

1. "What baseball data can you access through this server?" (should surface
   the workflow guide and tool list, not hallucinate)
2. "What are the server's limits on page sizes and history depth?"

**Player research**

3. Look up a well-documented player (e.g. Babe Ruth) — career line, teams,
   Hall of Fame status.
4. Look up a Negro Leagues star (e.g. Josh Gibson) — does the answer note
   the documented record's limits?
5. Search for a partial-record player (e.g. surname "Smith" in the 1920s
   Negro Leagues) — how does the assistant present a player with no first
   name?
6. Ask for a player who doesn't exist — clean "not found," or invention?

**Leaderboards and seasons**

7. A curated leaderboard ("top 10 in home runs, 1961 AL") — verify against
   your references.
8. A rate-stat leaderboard ("best ERA in 1968") — do qualification
   thresholds behave sensibly for short seasons?
9. A team season ("1927 Yankees") — record, standings context.

**Hall of Fame and salaries**

10. Voting history for a borderline candidate across multiple years.
11. A player's salary history; a team payroll for a specific year.

**Robustness**

12. Ask something the server explicitly doesn't cover (play-by-play,
    projections, park factors) — does it say so or improvise?
13. Ask an ambiguous question ("who was the best pitcher?") — does the
    assistant over-claim precision from raw totals?
14. Request an absurd page size or year range — is the error message
    helpful?
15. Anything from your own research life the server *should* handle —
    tell us what's missing.

**What to report:** for each finding — the question you asked, what you
expected, what happened, and (if wrong) the correct answer with your
source. Screenshots welcome. Setup friction counts as a finding.

## Feedback channel

- Primary: GitHub issues at
  https://github.com/cwoodruff/baseball-history/issues, labeled `mcp` +
  `beta-feedback` (create the `beta-feedback` label before inviting).
- Fallback: reply-by-email for testers who don't use GitHub; maintainer
  files the issue and links it back to them.

## Thank-you / credits

Add a **Credits** section to the MCP Server Guide (and the README once the
beta closes) naming each tester who consents, with a one-line description
of their contribution. Beta testers who file accuracy findings get called
out specifically — accuracy reports from people with real reference
shelves are the whole point of this beta.

---

## Appendix: local setup for technical testers

For testers comfortable with a terminal, before a hosted endpoint exists:

1. Install the .NET 10 SDK and clone
   https://github.com/cwoodruff/baseball-history
2. You'll need a PostgreSQL database loaded with Lahman data — ask the
   maintainer for read-only credentials rather than building your own.
3. `dotnet user-secrets set --project baseball-history-mcp "ConnectionStrings:Lahman" "<provided>"`
4. `dotnet run --project baseball-history-mcp` (serves http://localhost:5190)
5. Point your MCP client at it:

```json
{
  "mcpServers": {
    "baseball-history": {
      "type": "http",
      "url": "http://localhost:5190/"
    }
  }
}
```

Full details: [MCP Server Guide](./MCP-SERVER-PLAN.md).
