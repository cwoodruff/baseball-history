# MCP Server Plan

## Status

Approved for **MCP M1: Foundation & Contracts**.

## Decision

Build MCP as a **dedicated `baseball-history-mcp` project** in this solution and keep **v1 stdio-first**.

That means M1 should deliver a local MCP server process that exposes a bounded, read-only baseball query surface without coupling protocol concerns to `baseball-history-web`.

## Why this shape

- **Separate host, separate concerns.** MCP transport, tool metadata, and future auth/rate-limit concerns should not live inside the public Razor/minimal API app.
- **Safer rollout.** Stdio keeps the first milestone focused on protocol contracts and query seams instead of adding HTTP hosting, CORS, host filtering, or public exposure decisions.
- **Reuse the right layer.** MCP should depend on read-only EF Core query services and projected records, not Razor `PageModel` types or HTML view models.

## M1 scope

M1 is the foundation milestone. It should cover:

1. **Host project**
   - Add `baseball-history-mcp` to `baseball-history.sln`
   - Target `net10.0`
   - Start locally over stdio
   - Preserve `ConnectionStrings:Lahman`

2. **Shared query seam**
   - Introduce reusable, read-only query services/records for non-web consumers
   - Start with player lookup, franchise lookup, and leaderboard-style reads
   - Follow existing projection-first, `NoTracking`, PostgreSQL-compatible EF Core patterns

3. **Metadata and discoverability**
   - Server name/version metadata
   - MCP-visible diagnostics or startup/config guidance
   - MCP-visible catalog for supported stat categories, limits, and result caps

4. **Explicit operational limits**
   - Conservative result caps
   - Explicit timeout/cancellation behavior
   - No unbounded list or query-shaped tool contracts

## Non-goals for M1

Do **not** expand M1 into these areas:

- HTTP or Streamable HTTP transport
- `ModelContextProtocol.AspNetCore`
- Aspire orchestration for the MCP host
- Remote/public hosting
- CORS or browser-based MCP support
- Authentication/authorization rollout beyond local execution
- Generic SQL execution
- Write/update/delete tools
- Full REST parity
- Reusing Razor `PageModel` classes, partials, or HTML view models as MCP contracts

Those belong in a later hardening/adoption milestone once the foundation is proven.

## Contract guardrails

- Return **bounded, structured JSON** only.
- Every collection-shaped tool/resource must expose an explicit limit or page contract with a hard cap.
- Tool/resource descriptions must state supported stat aliases and safe parameter ranges.
- Query services must materialize only the fields needed for MCP responses.
- Web behavior must remain unchanged; M1 is additive.

## Review gate before handoff

M1 is not ready to hand back unless all of these are true:

- `baseball-history-mcp` exists as a separate project in the solution
- transport is stdio-first, with no HTTP transport added in M1
- `ConnectionStrings:Lahman` remains the only runtime database contract
- query reuse happens through shared read services/records, not web page types
- metadata/diagnostics/stat-catalog content is discoverable through MCP-visible surfaces
- result caps/timeouts are explicit in code/config
- solution build is green
- regression suite remains green

## Issue alignment

- **#21** confirms this architecture decision
- **#22** implements the dedicated stdio-first host
- **#23** implements the shared query seam
- **#24** implements metadata, diagnostics, and stat-catalog discoverability

If implementation pressure conflicts with these guardrails, the scope is wrong, not the gate.
