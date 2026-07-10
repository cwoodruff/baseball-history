# MCP Server Guide

## Status

`baseball-history-mcp` is the shipped MCP server for this repository.

- **Transport:** streamable HTTP on port 5190
- **Runtime database contract:** `ConnectionStrings:Lahman`
- **Database provider:** PostgreSQL via Npgsql
- **Posture:** read-only, bounded queries, no mutations

This file keeps the original path so existing README links continue to work, but the content is now the contributor/client adoption guide for the shipped surface.

## What ships today

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
11. `get_salary_leaders`
    - Highest-paid player rows with optional year filtering and paging
12. `get_server_diagnostics`
    - Safe runtime posture and limits; no secrets returned

### Resources

- `baseball-history://server/info`
  - Server identity, startup requirements, limits, tool names, and resource links
- `baseball-history://server/workflow-guide`
  - Workflow routing for common baseball question types
- `baseball-history://server/stats-catalog`
  - Supported batting/pitching stats plus the supported Lahman year span
- `baseball-history://server/diagnostics`
  - Safe runtime posture and connectivity status
- `baseball-history://hall-of-fame/guide`
  - Hall of Fame tool limits, year coverage, and voting-history caveats
- `baseball-history://salary/guide`
  - Salary tool limits, year coverage, and row-shape caveats

### Configured limits

These concrete caps match `appsettings.json` and the shipped `baseball-history://server/info` limit snapshot:

- Query timeout: **30 seconds**
- `search_players` page size max: **100**
- `list_franchises` page size max: **50**
- `list_hall_of_fame_inductees` page size max: **50**
- Batting/pitching leaderboard page size max: **100**
- `get_hall_of_fame_voting_history` row cap: **25**
- `get_player_salary_history` season cap: **40**
- `get_salary_leaders` page size max: **50**

If you need capabilities beyond that surface, treat them as follow-on work. Do not document or assume generic SQL, writes, or REST-parity tools because they are not part of the shipped MCP contract.

## Transport

The server serves streamable HTTP at `http://localhost:5190/`, with health endpoints at `/healthz` and `/alive` from the shared service defaults. Port 5190 is unique within this solution (the web app uses 5186/7209; Aspire infrastructure uses 15066–23211). The client connects to the running server over that URL; it does not launch the process itself.

Hardening currently in place, per the MCP C# SDK guidance for local HTTP hosting:

- The server binds to localhost only; there is no remote or public hosting story.
- `AllowedHosts` is restricted to `localhost;127.0.0.1` because Kestrel does not validate `Host` headers by default.
- No CORS is enabled; browser-based cross-origin access is not a supported scenario.
- There is no authentication — do not expose the port beyond the local machine.

### Aspire orchestration

The Aspire AppHost (`baseball-history-aspire`) hosts the MCP server as the `mcp` resource using the `http` launch profile, with an HTTP health check on `/healthz`. `aspire run` (or `dotnet run --project baseball-history-aspire`) starts the web app and the MCP server together.

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

Streamable HTTP on port 5190:

```bash
dotnet run --project baseball-history-mcp
curl http://localhost:5190/healthz   # -> Healthy
```

Everything at once via Aspire:

```bash
aspire run
```

### 4. Validate the shipped metadata/query surface

Use the existing MCP-focused test coverage when touching the server contract or docs about the server contract:

```bash
dotnet test baseball-history-tests --filter "FullyQualifiedName~baseball_history_tests.Mcp"
```

This includes protocol integration tests and HTTP smoke tests that spawn the server and exercise it over HTTP.

## Sample local client configuration

The server must already be running (for example under `aspire run` or `dotnet run --project baseball-history-mcp`). This repository's own `.mcp.json` connects to it over HTTP:

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

Notes:

- Start the server before the client connects; the client no longer launches the process itself.
- The `ConnectionStrings:Lahman` value is configured on the server (through user-secrets or environment variables), not by the client.

## How client authors should adopt the server

Start from the discoverable metadata instead of hard-coding assumptions:

1. Read `baseball-history://server/info`
   - Confirm transport, limits, tool names, and startup requirements
2. Read `baseball-history://server/stats-catalog`
   - Discover supported stat keys and aliases before leaderboard calls
3. Read `baseball-history://server/workflow-guide`, `baseball-history://hall-of-fame/guide`, and `baseball-history://salary/guide`
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

- Remote or authenticated HTTP access (the HTTP transport is localhost-only)
- Browser/CORS compatibility
- Write tools
- Arbitrary table/query access
- Full parity with the web app or future roadmap ideas

## Explicit non-goals

These are out of scope for the shipped server:

- Remote/public hosting
- Browser-based MCP usage or CORS support
- Authentication/authorization rollout beyond local execution
- Generic SQL execution
- Write/update/delete tools
- Full REST parity with the web app
- Reusing Razor `PageModel` types, partials, or HTML view models as MCP contracts
- Additional baseball domains that are not currently exposed as MCP tools/resources

## Plausible future expansion

Reasonable follow-on directions:

1. **Remote hosting**
   - Only after auth, exposure, and hardening decisions are explicit
2. **Broader read surface**
   - Additional baseball domains beyond the current player/franchise/team-season/leaderboard/Hall-of-Fame/salary surface, but only when shipped as bounded read models/tools
3. **More discoverability**
   - Additional metadata resources when they reflect real shipped behavior, not aspirational scope

Future notes are roadmap-shaped, not promises. Keep documentation anchored to code, tests, and the currently exported MCP surface.
