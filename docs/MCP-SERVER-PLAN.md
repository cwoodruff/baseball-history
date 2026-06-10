# MCP Server Guide

## Status

`baseball-history-mcp` is the shipped **v1** MCP server for this repository.

- **Transport:** stdio only
- **Runtime database contract:** `ConnectionStrings:Lahman`
- **Database provider:** PostgreSQL via Npgsql
- **Posture:** read-only, bounded queries, no mutations

This file keeps the original path so existing README links continue to work, but the content is now the contributor/client adoption guide for the shipped v1 surface.

## What v1 actually ships

The server exposes a deliberately small MCP surface:

### Tools

1. `search_players`
   - Free-text or last-name-prefix player lookup
   - Paged, read-only results
2. `get_player`
   - One player with career batting, career pitching, and team tenures
3. `list_franchises`
   - Franchise summaries with optional league/active filters
4. `get_franchise`
   - One franchise with season-by-season history
5. `get_team_season`
   - One team-season snapshot with roster, club batting, and club pitching context
6. `get_batting_leaders`
   - Career or single-season batting leaderboards
7. `get_pitching_leaders`
   - Career or single-season pitching leaderboards
8. `list_hall_of_fame_inductees`
   - Bounded Hall of Fame inductee discovery with optional year/category filters
9. `get_hall_of_fame_voting_history`
   - Full Hall of Fame voting history for one player when Lahman voting rows exist
10. `get_player_salary_history`
    - Salary history and career total for one player when Lahman salary rows exist
11. `get_team_payroll`
    - One team payroll snapshot for a supported salary year
12. `get_salary_leaders`
    - Highest-paid player rows with optional year filtering and paging
13. `get_server_diagnostics`
    - Safe runtime posture and limits; no secrets returned

### Resources

- `baseball-history://server/info`
  - Server identity, startup requirements, limits, tool names, and resource links
- `baseball-history://server/stats-catalog`
  - Supported batting/pitching stats plus the supported Lahman year span
- `baseball-history://server/diagnostics`
  - Safe runtime posture and connectivity status
- `baseball-history://server/transport-policy`
  - The shipped v1 HTTP no-go recommendation and the MCP C# SDK host-validation/CORS guidance behind it
- `baseball-history://guides/getting-started`
  - The recommended discovery-first adoption path for real MCP clients
- `baseball-history://guides/workflows`
  - Representative shipped v1 workflows for discovery, leaderboards, Hall of Fame, salaries, and diagnostics

### Configured limits in v1

These concrete caps should match the shipped `baseball-history://server/info` limit snapshot:

- Query timeout: **30 seconds**
- `search_players` page size max: **100**
- `list_franchises` page size max: **50**
- `list_hall_of_fame_inductees` page size max: **100**
- Batting/pitching/salary leaderboard page size max: **100**
- `get_player_salary_history` season cap: **20**
- `get_team_payroll` player-row cap: **25**

If you need capabilities beyond that surface, treat them as follow-on work. Do not document or assume generic SQL, writes, or REST-parity tools in v1 because they are not part of the shipped MCP contract.

## Why v1 is stdio-first

v1 is stdio-first on purpose:

- It keeps adoption local and explicit while the MCP contracts settle.
- It avoids premature HTTP hosting decisions around auth, public exposure, CORS, host filtering, and operational hardening.
- It matches the implementation that actually ships today: the host registers `WithStdioServerTransport()` and nothing else.
- It lets contributors validate the read/query surface separately from the public Razor/minimal API app.

That posture keeps v1 coherent: one local process, one database contract, one bounded read-only server surface.

## HTTP transport recommendation for v1

**Recommendation: no-go for v1. Keep `baseball-history-mcp` stdio-only.**

That is grounded in the MCP C# SDK guidance for Streamable HTTP hosting:

- local HTTP servers should restrict `AllowedHosts` to loopback values instead of `"*"` because Kestrel does not validate `Host` headers by default
- CORS should only be enabled when browser-based cross-origin access is intentional
- CORS is not a substitute for host validation
- stateful or resumable HTTP flows require additional CORS headers such as `Mcp-Session-Id`

This repository does not yet have a committed browser client or remote-hosting requirement for MCP. Enabling HTTP now would therefore add host-filtering, CORS-policy, ingress, and transport-test obligations without improving the shipped stdio-first use case. Revisit HTTP only when the team is ready to own explicit `AllowedHosts` configuration and a narrowly scoped CORS allowlist end to end.

## Local setup for contributors

### Prerequisites

- .NET 10 SDK
- Access to a PostgreSQL database loaded with Lahman data

### 1. Restore and build

```bash
dotnet restore
dotnet build baseball-history.sln
```

### 2. Configure the shared connection string

`baseball-history-web` and `baseball-history-mcp` share the same `UserSecretsId`, so setting the secret on either project works for both.

```bash
dotnet user-secrets set --project baseball-history-mcp \
  "ConnectionStrings:Lahman" \
  "Host=<server>;Port=5432;Database=lahman;Username=<user>;Password=<local-password>;SSL Mode=Require;Trust Server Certificate=true"
```

You can also provide the same value through `ConnectionStrings__Lahman`. The server rejects missing or placeholder values containing `<`.

### 3. Run the server locally

```bash
dotnet run --project baseball-history-mcp --no-build
```

The process stays attached to stdio for MCP clients. It is not an HTTP server.

### 4. Validate the shipped metadata/query surface

Use the existing MCP-focused test coverage when touching the server contract or docs about the server contract:

```bash
dotnet test baseball-history-tests --filter "FullyQualifiedName~baseball_history_tests.Mcp"
```

## Sample local client configuration

Example workspace `.mcp.json` entry for a stdio client that launches the server through `dotnet run`:

```json
{
  "mcpServers": {
    "baseball-history": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/baseball-history/baseball-history-mcp/baseball-history-mcp.csproj",
        "--no-build"
      ]
    }
  }
}
```

Notes:

- Use an absolute path so the client does not depend on its own working-directory behavior.
- The client machine still needs `ConnectionStrings:Lahman` configured through user-secrets or environment variables before launch.
- If you already built the solution, `--no-build` keeps startup faster and closer to normal client usage.

## How client authors should adopt v1

Start from the discoverable metadata instead of hard-coding assumptions:

1. Read `baseball-history://server/info`
   - Confirm transport, limits, tool names, and startup requirements
2. Read `baseball-history://server/stats-catalog`
   - Discover supported stat keys and aliases before leaderboard calls
3. Read `baseball-history://guides/getting-started` or `baseball-history://guides/workflows`
   - Follow the shipped discovery and workflow guidance instead of inventing new tool sequences
4. Use the bounded tools
   - Respect page contracts and configured caps
5. Use `get_server_diagnostics` or `baseball-history://server/diagnostics`
   - Check runtime posture without expecting secrets or raw connection details

Client authors should assume:

- Responses are structured JSON only
- Collections are paged/capped
- The server is read-only
- The server may refuse startup if the database contract is not configured correctly

Client authors should **not** assume:

- HTTP endpoints
- Browser/CORS compatibility
- Write tools
- Arbitrary table/query access
- Full parity with the web app or future roadmap ideas

## Explicit v1 non-goals

These are out of scope for the shipped v1 server:

- HTTP or Streamable HTTP transport
- `ModelContextProtocol.AspNetCore`
- Remote/public hosting
- Browser-based MCP usage or CORS support
- Authentication/authorization rollout beyond local execution
- Generic SQL execution
- Write/update/delete tools
- Full REST parity with the web app
- Reusing Razor `PageModel` types, partials, or HTML view models as MCP contracts
- Aspire orchestration for the MCP host
- Additional baseball domains that are not currently exposed as MCP tools/resources

## Plausible future expansion

Reasonable follow-on directions, once v1 adoption is stable:

1. **HTTP transport**
   - Only after auth, exposure, and hardening decisions are explicit
2. **Broader read surface**
   - Additional baseball domains beyond the current player/franchise/team-season/leaderboard/Hall-of-Fame/salary surface, but only when shipped as bounded read models/tools
3. **Operational hosting**
   - Aspire orchestration or other local/dev hosting support if the team decides the MCP host should participate in the resource graph
4. **More discoverability**
   - Additional metadata resources when they reflect real shipped behavior, not aspirational scope

Future notes are roadmap-shaped, not promises. Keep documentation anchored to code, tests, and the currently exported MCP surface.
