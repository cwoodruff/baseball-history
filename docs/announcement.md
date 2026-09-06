# Announcement Copy

Draft copy for launching Baseball History (issue #73). Two lengths, one
framing: lead with the differentiators — honest historical record, AI access
via MCP, open API — not the tech stack.

> Replace the live URL below with the custom domain before publishing;
> the current deployment answers at
> https://baseball-history-auhvctc7b0a3bfd0.centralus-01.azurewebsites.net

---

## Long form (blog post, SABR forum, mailing list)

**Baseball History: 150 years of MLB — with the gaps in the record marked,
not papered over**

Most baseball stats sites render every player the same way, whether the
record behind them is a complete career or a single surviving box score.
Baseball History takes the opposite approach. The database holds 453 players
known only by a surname or an initial — eight of them just "Smith" — most
from the segregation-era Black leagues, where the record lived in weekly
newspapers and much of it never survived. The site marks each of them with a
*Partial record* badge, explains on every leaderboard that raw totals are not
context-adjusted across eras, and tells the story behind the fragments on a
dedicated page: why the box scores survived unevenly, what reconstruction
projects like Seamheads are recovering, and why Josh Gibson's documented 197
home runs and his plaque's "almost 800" are both honest numbers measuring
different things.

Around that foundation is a full research toolkit covering 24,000+ players
from 1871 to 2025: side-by-side comparison of up to four players with charts
and CSV export, batting and pitching leaderboards with season-relative
qualification thresholds, complete Hall of Fame voting history, award voting
breakdowns, postseason results back to 1884, and player salary data.

Two more things you won't find on most stats sites. Everything is open
through a REST API — 30+ JSON endpoints, no authentication, with an
interactive explorer. And the data is exposed to AI assistants through a
Model Context Protocol server, so Claude and other MCP clients can answer
baseball questions against the real database — look up a player, pull a
curated leaderboard, walk Hall of Fame voting year by year — rather than
from memory.

The project is open source (ASP.NET Core, PostgreSQL, htmx — no JavaScript
framework), built on the Lahman Baseball Database (CC BY-SA 3.0):
https://github.com/cwoodruff/baseball-history

## Short form (social)

150 years of MLB history, browsable and queryable — with the gaps in the
record marked, not papered over. 453 players survive only as surnames;
Baseball History badges them and tells you why. Plus: 4-player comparison,
an open REST API, and an MCP server so AI assistants can query the real
data. Open source.

## One-liner (link description)

Explore 150 years of baseball history — honest about what survived, open
through a REST API, and queryable by AI assistants via MCP.
