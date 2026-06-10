# MCP Server Documentation Page — Design

**Date:** 2026-06-10
**Status:** Approved for planning
**Project:** baseball-history-web

## Goal

Add a developer documentation page to `baseball-history-web` that documents the
`baseball-history-mcp` server — its tools, resources, how to connect, and example
usage — mirroring the existing REST API documentation page (`Pages/ApiDocs.cshtml`).

## Background

The solution ships an MCP (Model Context Protocol) server (`baseball-history-mcp`)
that exposes read-only Lahman baseball data to AI clients. It runs over **stdio**,
is **read-only**, has **no HTTP transport** and **no auth**, and requires a
`ConnectionStrings:Lahman` value at startup. The web app already documents its REST
API at `/ApiDocs`; there is no equivalent page for the MCP server. This page fills
that gap for developers who want to connect an MCP client (Claude Desktop, VS Code,
Claude Code) or call the server programmatically.

## Non-Goals

- No runtime introspection of the MCP assembly. The page is static content.
- No changes to the MCP server itself.
- No live "try it" explorer (the API page links to Scalar; MCP has no browser-based
  equivalent, so this page is reference-only).
- No multi-language SDK examples beyond one C# example plus a pointer to the
  Python/TypeScript SDKs.

## Approach

A single static Razor Page, `Pages/McpDocs.cshtml` + `Pages/McpDocs.cshtml.cs`,
following the exact structure and styling of `Pages/ApiDocs.cshtml`:

- `_PageHeader` component for the title block
- `<section class="mb-5">` blocks with `<h2 class="border-bottom pb-2">` headings
- Dark code blocks: `<pre class="bg-dark text-light p-3 rounded"><code>…</code></pre>
- Bootstrap `badge` chips for tool names, `table table-sm` for parameter tables

Rejected alternative: generating the tool list at runtime from the MCP metadata
service. This would couple the web project to MCP-assembly internals and diverge
from the API page's static pattern, for no real benefit since the surface is stable.

The code-behind is an empty `OnGet()` `PageModel`, identical in shape to
`ApiDocsModel`.

## Navigation

Add a nav link in `Pages/Shared/_ShellHeader.cshtml`, immediately after the existing
"API" link (line 46):

```html
<a class="nav-link" asp-area="" asp-page="/McpDocs">MCP</a>
```

## Page Content (sections in order)

### 1. Header
`_PageHeader` with:
- Eyebrow: `Developer`
- Title: `MCP Server`
- Subtitle: read-only access to 150+ years of Major League Baseball data through the
  Model Context Protocol, for use with AI assistants and agents.

### 2. Info alert
A Bootstrap `alert alert-info` (matching the API page's alert) that:
- Explains MCP in one sentence and links to
  `https://modelcontextprotocol.io` (target=_blank, rel=noopener).
- States the server is **stdio transport, read-only, no HTTP, no authentication**.

### 3. How it works
Short prose + bullet list:
- 12 read-only tools and 6 JSON resources.
- Communicates over stdio (standard in/out); the client launches the server process.
- Requires a `ConnectionStrings:Lahman` connection string at startup; placeholder
  strings containing `<` are rejected.
- Recommended entry points: the `get_server_diagnostics` tool and the
  `baseball-history://server/info` resource before calling domain tools.

### 4. Connecting (client config JSON)
Two labeled subsections, each with a dark JSON code block.

**Claude Desktop / Claude Code** — stdio server entry:
```json
{
  "mcpServers": {
    "baseball-history": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/baseball-history-mcp"],
      "env": {
        "ConnectionStrings__Lahman": "Host=...;Database=...;Username=...;Password=..."
      }
    }
  }
}
```

**VS Code** (`.vscode/mcp.json`) — equivalent `servers` block.

A note: the connection string is supplied via the `ConnectionStrings__Lahman`
environment variable (the .NET double-underscore convention), or via user-secrets
during local development.

### 5. Example prompts (natural language)
A `table table-sm` mapping ~6 representative user questions to the tool(s) they drive:

| Ask your AI assistant | Tool(s) used |
| --- | --- |
| "Find players whose last name starts with R" | `search_players` |
| "Show me Babe Ruth's career stats" | `get_player` |
| "Career home run leaders since 2000" | `get_batting_leaders` |
| "Lowest career ERA, min 1000 innings" | `get_pitching_leaders` |
| "Who was inducted into the Hall of Fame in 1999?" | `list_hall_of_fame_inductees` |
| "Mike Trout's salary history" | `get_player_salary_history` |

### 6. Tools reference
Grouped by domain. Each tool is one line: a `badge bg-secondary` (or similar) with the
tool name + a one-line description taken from the `[Description]` attribute in
`Tools/BaseballReferenceTools.cs`.

Domains and tools:
- **Players** — `search_players`, `get_player`
- **Franchises & Teams** — `list_franchises`, `get_franchise`, `get_team_season`
- **Leaderboards** — `get_batting_leaders`, `get_pitching_leaders`
- **Hall of Fame** — `list_hall_of_fame_inductees`, `get_hall_of_fame_voting_history`
- **Salaries** — `get_player_salary_history`, `get_salary_leaders`
- **Diagnostics** — `get_server_diagnostics`

**Full parameter tables only** for `get_batting_leaders` and `get_pitching_leaders`
(parameter / type / default / description), because they carry the stat lists:
- Batting stats: `hr, h, r, rbi, sb, 2b, 3b, bb, g, ab, avg, obp, slg, ops`
- Pitching stats: `w, l, so, sv, cg, sho, ip, g, gs, hr, k9, wpct, era, whip, bb9`
  (era, whip, bb9 sort ascending — lower is better)

### 7. Resources reference
A `table table-sm` of the 6 resource URIs and their purpose (verbatim from the
server's `ResourceLinks`):

| URI | Purpose |
| --- | --- |
| `baseball-history://server/info` | Server identity, startup requirements, limits |
| `baseball-history://server/workflow-guide` | Tool/guide routing for common questions |
| `baseball-history://server/stats-catalog` | Supported batting/pitching stats + year span |
| `baseball-history://server/diagnostics` | Safe runtime posture, limits, connectivity |
| `baseball-history://hall-of-fame/guide` | Hall of Fame tool limits and caveats |
| `baseball-history://salary/guide` | Salary tool limits and row shape |

### 8. Programmatic SDK
One full C# example using `ModelContextProtocol.Client`: construct a
`StdioClientTransport` pointing at the server, create the client with
`McpClientFactory.CreateAsync`, list tools, and call a tool (e.g.
`get_batting_leaders`) with `CallToolAsync`. Followed by a one-line note that the
same connect → list → call flow is available from the official Python and TypeScript
MCP SDKs.

### 9. Limits & data notes
A bullet list mirroring the API page's "Data Notes", covering:
- Page-size caps (player search 100, franchise list 50, Hall of Fame 50, leaderboards
  100, salary leaderboard 50) and bounded history (Hall of Fame voting 25 years,
  salary history 40 seasons). Values from `BaseballMcpLimitOptions`.
- All data from the Lahman Baseball Database; read-only.
- Lahman ID conventions (`ruthba01`, `NYY`, `NYA`/`AL`).
- Salary data from 1985 onward; innings pitched stored as outs and converted.

## Accuracy Constraints

Every tool name, resource URI, stat key, and limit value on the page MUST match the
source of truth:
- Tools: `baseball-history-mcp/Tools/BaseballReferenceTools.cs` and
  `BaseballServerDiagnosticsTools.cs`
- Resources: `ResourceLinks` in `Metadata/BaseballMcpMetadataService.cs`
- Limits: `BaseballMcpLimitOptions` in `Configuration/BaseballMcpOptions.cs`
- Stat keys: `Querying/LeaderboardStatCatalog.cs`

## Testing

Follow the existing web test patterns (`baseball-history-tests`):
- A page-rendering smoke test (in the style of `Pages/PageRoutingIntegrationTests.cs`)
  asserting `GET /McpDocs` returns 200 and the HTML contains key surface markers
  (e.g. "MCP Server", a couple of tool names like `search_players` and
  `get_batting_leaders`, and a resource URI).
- Optionally assert the nav contains the MCP link.

No new runtime code paths are introduced, so unit tests are not required beyond the
rendering smoke test.

## Files Touched

- `baseball-history-web/Pages/McpDocs.cshtml` (new)
- `baseball-history-web/Pages/McpDocs.cshtml.cs` (new)
- `baseball-history-web/Pages/Shared/_ShellHeader.cshtml` (add nav link)
- `baseball-history-tests/Pages/…` (new rendering smoke test)
