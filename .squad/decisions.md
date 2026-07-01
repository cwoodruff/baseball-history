# Squad Decisions

# Parker — PostgreSQL EF Core Migration (2026-06-09)

**Author:** Parker  
**Status:** ✅ COMPLETE (Commit: `6ddf8c0`)

## Decision

Migrate the baseball-history application from SQLite to PostgreSQL using Npgsql (EF Core provider), while maintaining the stable `ConnectionStrings:Lahman` runtime configuration key.

## What Was Done

1. **EF Core Provider Switch**
   - Changed `Program.cs` from `.UseSqlite()` to `.UseNpgsql()`
   - Updated connection string to point to PostgreSQL database
   - Migrated all database schemas to PostgreSQL

2. **Model Normalization**
   - Stripped SQLite-specific collations and store-type annotations
   - Applied value converters for legacy string properties backed by numeric PostgreSQL columns
   - Ensured full compatibility with Npgsql query translation

3. **Configuration & Security**
   - Committed config contains only placeholders (no live credentials)
   - Local development uses `dotnet user-secrets`
   - Azure deployment uses App Service settings / Key Vault references
   - Connection string key remains `ConnectionStrings:Lahman` across all environments

4. **Testing**
   - Added provider-level smoke tests validating Npgsql model and query translation
   - Full regression suite: 350/350 tests passing
   - Tests validate functionality even without live PostgreSQL connection in environment

## Rationale

**Why PostgreSQL?**
- Aligns with Azure deployment platform (Azure Database for PostgreSQL)
- Improves scalability and feature set vs SQLite
- Npgsql is mature, well-maintained EF Core provider
- Configuration pattern is identical to SQLite from app perspective

**Why keep `ConnectionStrings:Lahman`?**
- Stable runtime contract across environments
- Zero changes needed in app configuration resolution logic
- Developers and operators don't need to learn new key names

**Why separate smoke tests?**
- Validates model/translation logic independent of live database availability
- Unblocks CI/local testing even without PostgreSQL provisioning
- Smoke tests can run in any environment; full integration requires real database

## Consequences

- App now **requires** a real PostgreSQL connection at runtime
- Operator must provide `ConnectionStrings:Lahman` before app can start
- Local development requires `dotnet user-secrets set` or environment variable
- Azure requires App Service configuration or Key Vault-backed setting
- Standalone `dotnet run` of web project fails without connection string (intentional safety measure)

---

# Ash — PostgreSQL Documentation & Configuration (2026-06-09)

**Author:** Ash  
**Status:** ✅ COMPLETE

## Decisions

### 1. Configuration Documentation (`docs/POSTGRES-MIGRATION.md`)

Created comprehensive migration guide covering:
- Local development setup using `dotnet user-secrets`
- Azure deployment architecture with App Service + Key Vault + Managed Identity
- Security model: connection strings never committed, secrets live in appropriate layers
- Step-by-step troubleshooting for connection issues
- Migration path for developers transitioning from SQLite

**Why separate doc?**
- `DATABASE.md` is for schema design, not configuration
- `DEVELOPMENT.md` covers setup flow; deserves comprehensive config reference
- New developers joining post-migration get clear onboarding path

### 2. Connection String Contract

Treat `ConnectionStrings:Lahman` as the single runtime contract everywhere:
- Local: User Secrets (developer machine, not synced to git)
- Azure: App Service configuration with Key Vault reference (preferred)
- Fallback: appsettings.json for development only

**Why single key?**
- No drift between environments
- `Program.cs` already hard-fails without real connection, so docs must match runtime reality
- Developers only need to learn one configuration key

### 3. README & Documentation Updates

Updated project documentation:
- Technology Stack now reflects "SQLite → PostgreSQL (migrating)"
- Cross-reference to `docs/POSTGRES-MIGRATION.md`
- Clarified that `lahman.db` is historical migration input, not app startup requirement
- Clear signaling about configuration approach and local setup

**Why now?**
- Transparency: Future PRs won't look like surprise changes
- New clones get correct expectations about database technology
- Reduced onboarding friction when migration lands

## Rationale

**Why User Secrets + Key Vault?**
- User Secrets: Standard .NET pattern for local dev, keeps secrets off developer disk
- Key Vault: Azure-native, integrates with Managed Identity, no stored credentials needed
- Combined: Developers never enter a password in any config file

**Why update docs proactively?**
- Parker's code changes won't need doc updates when merged—guidance already in place
- Reduces friction for developers reading docs before code change lands
- Clear responsibility matrix: who sets what where across environments

## Consequences

- Developers must understand User Secrets setup for local work
- Azure operators must configure Key Vault references before deployment
- Documentation is now the source of truth for configuration flow
- No connection strings should ever appear in tracked files

---

# Ash — Health Route Ambiguity Resolution (2026-06-09)

**Author:** Ash  
**Status:** ✅ COMPLETE (Commit: `6a5f202`)

## Decision

Resolve `/Health` route ambiguity by moving the machine-readable readiness endpoint from `/health` to `/healthz`, keeping `/alive` as liveness probe, and preserving the existing Razor support page at `/Health`.

## Context

The web app exposes a Razor support page at `/Health` for human diagnostics. The shared `baseball-history-servicedefaults` package also mapped a machine health-check endpoint at `/health`. ASP.NET Core routing is case-insensitive, so requests to `/Health` matched both endpoints, causing `AmbiguousMatchException` failures in integration tests.

## What Was Changed

1. **Moved readiness endpoint** from `/health` → `/healthz` in `baseball-history-servicedefaults`
2. **Preserved** liveness endpoint at `/alive`
3. **Preserved** human-facing diagnostics at `/Health` (Razor page)

## Rationale

- **Human-readable `/Health` page**: Preserves intended support page contract already used by app and tests
- **Dedicated `/healthz`**: Provides machine readiness probe without Aspire-specific logic in web project
- **No breaking changes**: Remains compatible with local, App Service, and Aspire scenarios where probe paths can target machine endpoint explicitly
- **Platform-level clarity**: Kubernetes, Docker compose, and Aspire probe configs can explicitly target `/healthz` for readiness

## Consequences

- Human diagnostics remain at `/Health`
- Machine readiness probes must use `/healthz`
- Liveness probes continue to use `/alive`
- No changes to web project code or Aspire AppHost configuration
- Existing `/Health` page behavior unchanged

---

# Lambert — PostgreSQL Acceptance Review (2026-06-09)

**Author:** Lambert  
**Status:** ✅ ACCEPTED (Commit: `6ddf8c0`, `8a59a17`, `6a5f202`)

## Context

PostgreSQL migration was previously blocked for two reasons:
1. Repository documentation lagged behind PostgreSQL runtime change
2. Health endpoint `/health` collided with `/Health` Razor page (case-insensitive routing)

Both blockers addressed:
- Ash updated docs to match runtime contract
- Ash moved readiness endpoint to `/healthz`

## Decision

Accept PostgreSQL migration and health-route fix for handoff.

## Verification

- ✅ Solution builds: `dotnet build baseball-history.sln`
- ✅ Full regression suite: 350/350 tests passing
- ✅ Secret review: Only placeholders/training examples in tracked files; no live raw database password
- ✅ Configuration contract: `ConnectionStrings:Lahman` consistently documented and enforced
- ✅ Documentation: README and POSTGRES-MIGRATION.md provide clear setup path
- ✅ Route ambiguity: Resolved without breaking changes

## Rationale

- Application consistently uses PostgreSQL through `ConnectionStrings:Lahman` at runtime
- Documentation matches runtime contract
- `/Health` support page remains intact for humans
- `/healthz` provides distinct machine-readable readiness probe
- `/alive` remains liveness probe
- Validation is strong enough for handoff: build passed, full regression suite green, no secret leaks

## Consequences

- Engineering can treat migration branch as ready to merge from quality gate perspective
- Azure deployment still requires operator configuration: provision real `ConnectionStrings__Lahman` (preferably Key Vault reference), ensure managed identity can read secret, restart app after setting in place
- Platform probes should target `/healthz` for readiness, `/alive` for liveness
- `/Health` remains human-facing diagnostics page

---

# Dallas — Issue #18 Salary Currency Formatting Fix (2026-06-08)

**Author:** Dallas  
**Date:** 2026-06-08  
**Issue:** #18  
**Status:** ✅ COMPLETE

## Decision

Use an explicit USD display helper for Salaries UI output (`$` + grouped whole-number formatting) instead of Razor's culture-sensitive `"C0"` formatting.

## Context

Salary amounts on the Salaries page were using Razor's culture-sensitive currency formatting (`"C0"`), which depends on the process culture and resulted in inconsistent display across environments.

## What Was Done

- Created shared USD display helper in `SalaryViewModel.cs`
- Applied consistent `$` + grouped whole-number formatting across list rows and team payroll card
- Added routing/integration coverage for both full-page and non-boosted htmx Salaries responses
- Verified build passes and new Salaries tests pass

## Rationale

- Bug is display-layer problem: salary amounts need stable dollar sign everywhere this page renders
- `"C0"` depends on process culture, making page less predictable across environments
- Shared helper keeps list rows and team payroll card aligned
- One source of truth for USD formatting logic

## Consequences

- Salaries page displays consistently regardless of server culture settings
- USD formatting rules now controlled by application logic, not framework defaults
- Any future currency display needs can reference same pattern

# Ash — M1 bounded franchise listing contract (2026-06-09)

**Status:** ✅ IMPLEMENTED

## Decision

Make `list_franchises` a bounded MCP collection contract by giving it the same explicit paging/clamp semantics as the other read tools, backed by `BaseballMcp:Limits:FranchiseListPageSizeMax`.

## What changed

1. Added `Limits:FranchiseListPageSizeMax` to MCP configuration and validation.
2. Changed the franchise lookup request/tool surface from an unbounded list to `page` + `pageSize` with a paged response that reports requested vs applied values.
3. Moved franchise filtering/counting/paging into the query pipeline so the service only materializes the requested window.
4. Extended MCP metadata/diagnostics limit snapshots and tests so the franchise cap is discoverable beside the existing player/leaderboard caps.

## Rationale

- Ripley’s final M1 rejection was correct: a top-level collection tool cannot rely on the dataset being “small enough.”
- Reusing the same bounded response pattern across MCP tools gives clients one limit/clamp story instead of a special-case franchise exception.
- Keeping the contract in MCP-specific read models preserves the stdio-first host boundary and avoids leaking any Razor/UI pagination types.

## Consequences

- `list_franchises` is now M1-compliant as a bounded, config-driven query surface.
- Operators can tune franchise list caps without code changes.
- Clients can detect clamping explicitly from the response and discover the hard cap through server metadata/diagnostics.
# Ash — MCP M1 metadata, limits, and diagnostics

**Date:** 2026-06-09  
**Status:** ✅ IMPLEMENTED

## Decision

Expose MCP discovery and runtime posture through MCP-visible resources plus one read-only diagnostics tool, while moving page-size caps and EF command timeout into explicit `BaseballMcp` configuration.

## What changed

1. Added direct MCP resources for:
   - `baseball-history://server/info`
   - `baseball-history://server/stats-catalog`
   - `baseball-history://server/diagnostics`
2. Added `get_server_diagnostics` tool for clients that prefer tool calls over resources.
3. Bound explicit MCP settings from `BaseballMcp` config:
   - `QueryTimeoutSeconds`
   - `Limits:PlayerSearchPageSizeMax`
   - `Limits:LeaderboardPageSizeMax`
4. Made paged responses echo requested vs applied page sizing so clamping is visible instead of implicit.
5. Kept the existing `ConnectionStrings:Lahman` runtime contract and stdio transport.

## Rationale

- Clients need discoverable guidance before richer MCP surfaces land.
- Hidden clamps are hard to debug from the client side; exposing configured caps and applied values removes ambiguity.
- Diagnostics must stay safe for local and hosted use, so only booleans, counts, configured limits, timeout values, and year-span metadata are exposed.

## Consequences

- MCP clients can discover supported stat categories, supported year span, and runtime posture without reading repo docs.
- Operators can tune timeout/cap behavior through configuration instead of code edits.
- Missing or placeholder `ConnectionStrings:Lahman` still fails fast at startup, but the expected failure mode is now documented in MCP-visible metadata.
# Ash — M2 workflow guidance surface (2026-06-09)

**Status:** ✅ COMPLETE

## Decision

Add a dedicated in-server MCP workflow guide resource at `baseball-history://server/workflow-guide` instead of introducing another tool or drifting into broader host changes.

## What Changed

1. Added a new JSON workflow guide resource that routes common baseball questions across:
   - `search_players` / `get_player`
   - `list_franchises` / `get_franchise`
   - `get_team_season`
   - `get_batting_leaders` / `get_pitching_leaders`
   - `list_hall_of_fame_inductees` / `get_hall_of_fame_voting_history`
   - `get_player_salary_history` / `get_salary_leaders`
   - `get_server_diagnostics` plus the shipped metadata resources
2. Updated server discovery metadata and stdio instructions to point clients at the workflow guide first.
3. Added focused metadata tests to prove:
   - the workflow guide is discoverable
   - its vocabulary matches the shipped tool/resource names
   - it teaches supported query shapes without implying unsupported capabilities

## Rationale

- This preserves the existing stdio-first, read-only host shape.
- Guidance belongs in a resource because clients should be able to discover it without invoking a side-effecting surface.
- The issue gap was not missing data access logic; it was missing safe routing guidance across the now-broader query surface.

## Consequences

- MCP clients now have a single canonical workflow-routing document inside the server.
- Future MCP additions should update both the workflow guide and the discoverability tests so vocabulary drift is caught quickly.
# Ash — MCP M3 hardening posture (2026-06-09)

**Status:** ✅ COMPLETE

## Decision

Keep `baseball-history-mcp` stdio-only for v1, centralize request-limit enforcement inside the MCP host, and normalize tool failures so clients get safe, predictable error payloads instead of raw implementation details.

## What Changed

1. Added a shared `BaseballMcpRequestPolicy` that now owns page-cap enforcement plus request normalization/validation for MCP read services.
2. Moved leaderboard stat definitions into a shared catalog so metadata and runtime validation cannot drift.
3. Added MCP call-tool failure normalization that preserves safe usage errors and masks unexpected runtime failures.
4. Added a discoverable `baseball-history://server/transport-policy` resource and updated `docs/MCP-SERVER-PLAN.md` with an explicit HTTP no-go recommendation for v1.

## Rationale

- Centralized request policy reduces hidden drift across services and keeps configured caps authoritative.
- MCP clients need deterministic, non-leaky failures; silent fallback and raw exception propagation are both poor contracts.
- The MCP C# SDK guidance makes HTTP transport a deliberate security decision because host validation (`AllowedHosts`) and restrictive CORS must be configured explicitly. With no committed browser or remote-hosting requirement today, stdio-only is the safer v1 posture.

## Consequences

- v1 remains coherent: one stdio transport, one PostgreSQL connection-string contract, one bounded read-only MCP surface.
- Future HTTP work now has a documented gate: do not enable it until ingress host validation, CORS ownership, and tests are ready.
- Future MCP surface additions should plug into the shared request policy/stat catalog so discovery metadata and runtime behavior stay aligned.

# Ash — MCP Server Planning for Baseball Statistics (2026-06-09)

**Author:** Ash  
**Status:** PROPOSED

## Decision

If the team adds MCP support, do it as a **new dedicated `baseball-history-mcp` project** in the solution, hosted with **`ModelContextProtocol.AspNetCore` over Streamable HTTP**, and set **`options.Stateless = true`** explicitly for the first release.

## Why this shape

- The current target capability is **read-only baseball-stat querying**, not server-to-client sampling, elicitation, subscriptions, or per-client workspace state.
- Stateless HTTP is the MCP C# SDK’s recommended mode for this kind of server because it avoids session affinity, lowers memory overhead, and scales cleanly behind normal ASP.NET Core hosting.
- A dedicated host avoids coupling MCP exposure to the public Razor/minimal-API web app and lets us harden transport, auth, rate limits, and observability independently.

## Technical slices

### Slice 1 — Project foundation and transport

- Add `baseball-history-mcp` to `baseball-history.sln`
- Target `net10.0`
- Use `ModelContextProtocol.AspNetCore`
- Reuse `baseball-history-servicedefaults` for health/telemetry conventions if we keep the host HTTP-based
- Map MCP on a dedicated route (for example `/mcp`)
- Set `WithHttpTransport(options => options.Stateless = true)` explicitly
- Keep the initial server loopback/local by default; defer public exposure until auth and host filtering are ready

### Slice 2 — Shared read-model/query layer

- Extract only the **first MCP-safe read paths** from the existing minimal API logic into reusable query services
- Start with logic currently proven in:
  - `baseball-history-web/Api/Endpoints/SearchEndpoints.cs`
  - `baseball-history-web/Api/Endpoints/PlayerEndpoints.cs`
  - `baseball-history-web/Api/Endpoints/LeaderEndpoints.cs`
- Reuse the existing `BaseballDbContext` and model normalization from `baseball-history-web/Models/BaseballDbContext.cs`
- Keep the extraction narrow; do **not** pause rollout for a repo-wide “shared data layer” rewrite

### Slice 3 — MVP MCP tool catalog

Recommended first tool set:

1. `search_players_and_franchises`
2. `get_player_detail`
3. `get_batting_leaders`
4. `get_pitching_leaders`
5. `get_team_season`
6. `get_hall_of_fame_inductees`

Contract rules:

- All tools return **bounded, structured JSON**
- Every list-style tool requires `page` / `pageSize` or an equivalent bounded limit
- Default page sizes should be conservative (for example 10-25), with hard caps no higher than existing REST norms
- Tool descriptions should explain the safe parameter ranges and supported stat aliases

### Slice 4 — Optional MCP resources/prompts after tools land

Not MVP-critical, but useful later:

- Resource for stat glossary / supported aliases
- Resource for query capability catalog
- Prompt templates for “explain this player” or “compare these seasons”

These are phase-two items. Tools should come first.

### Slice 5 — Local orchestration, tests, and rollout

- Wire the MCP project into `baseball-history-aspire/AppHost.cs` for local startup/discovery
- Add transport/config smoke tests similar in spirit to the existing provider/model smoke tests
- Add a few end-to-end MCP contract tests for the highest-value tools
- Roll out locally first, then protected remote hosting, then optional broader client enablement

## Prerequisites

### 1. Exposure decision

The team needs to decide whether the first consumer is:

- local/dev-only MCP clients, or
- remotely hosted MCP clients

That choice changes the auth and host-filtering work needed in slice 1.

### 2. Database credential model

The MCP host should ideally use a **read-only database principal** before remote rollout.

Safe path:

- local/dev MVP can reuse the existing runtime contract temporarily if needed
- remote rollout should move to a least-privileged secret for the MCP host

### 3. Query contract boundaries

Before implementation, lock:

- supported stats and aliases
- max year ranges
- max page sizes
- whether comparisons or bulk multi-player lookups are in scope

Without those limits, LLM-driven retry loops can create avoidable database load.

### 4. Configuration hardening

- keep secrets externalized
- do not rely on permissive host filtering for the MCP host
- define explicit `AllowedHosts` for the MCP endpoint
- only enable CORS if browser-based MCP access is truly required

### 5. Observability baseline

The MCP host should emit:

- tool name
- validated argument summary
- duration
- rows/results returned
- cache hit/miss where relevant
- failure class / rejection reason

This is necessary because LLM traffic is bursty and hard to reason about from HTTP logs alone.

## Operational concerns

### Load amplification

LLMs tend to retry, reformulate, and chain tool calls. A single user question can become multiple DB queries. The server must protect itself with:

- bounded result sizes
- request timeouts
- cancellation propagation
- concurrency/rate limiting
- no generic unrestricted query surface

### Result-shape inflation

Raw baseball tables are too wide and too verbose for LLM context windows. MCP responses should return:

- concise summaries
- pagination metadata
- only the fields needed for reasoning

Returning “everything” is bad for latency, token cost, and answer quality.

### Query drift

If MCP reimplements LINQ separately from the REST endpoints, behavior will drift. Shared read services for the first tool set are safer than copy/paste.

### Caching strategy

Existing cache patterns remain useful:

- 24-hour lookup caches for Hall of Fame IDs and filter sets
- no aggressive cache-warming for every MCP tool on day one
- consider warming only if telemetry shows a hot-path leaderboard or search query

### Security

- No generic SQL execution tool in v1
- Keep the server read-only
- Require explicit auth before remote exposure
- Lock down `AllowedHosts`
- Enable CORS only for specific trusted browser origins, if ever needed
- Treat tool/resource descriptions and upstream content as prompt-injection-sensitive inputs

### Deployment shape

Stateless HTTP keeps deployment simple:

- no session affinity
- no in-memory client session management
- cleaner horizontal scaling

Stateful mode should only be introduced if we later need subscriptions, sampling, elicitation, or per-client state.

## Recommended safe defaults

- **Project:** dedicated `baseball-history-mcp`
- **Transport:** Streamable HTTP
- **Mode:** explicit stateless
- **Access:** local/loopback first
- **Auth:** required before remote rollout
- **Data access:** bounded read-only tools only
- **Reuse:** `BaseballDbContext` + extracted shared query services
- **Rollout:** search/player/leaders first; prompts/resources later

## Consequences

- Slightly more solution surface area now, but lower operational risk later
- Some query extraction work is required to avoid drift
- Remote exposure is blocked until auth, host filtering, and least-privilege DB access are in place
- The end state fits the current solution structure without forcing a UI or API redesign
# Dallas — M3 MCP limit contract doc fix (2026-06-09)

## Decision

Update only the MCP adoption guide for the M3 review fix and make its published limit list match the full shipped `server/info` contract.

## Rationale

- The review issue was a docs mismatch, not a runtime or UX issue.
- `BaseballMcpMetadataService` and `McpLimitSnapshot` already publish the authoritative v1 caps.
- The smallest safe fix is to align the guide's "Configured limits in v1" section with those exported values instead of broadening other docs.

## Consequence

- Contributors and client authors now see the same Hall of Fame, salary-history, and team-payroll caps in the guide that MCP metadata already exposes.
## Lambert — MCP M1 review (2026-06-09)

**Status:** ✅ APPROVED

### Decision

Approve the completed MCP M1 foundation work for handoff.

### Why

- Dedicated `baseball-history-mcp` host exists in the solution and runs locally over stdio with no HTTP transport added.
- Runtime database contract remains `ConnectionStrings:Lahman`; startup guidance and diagnostics expose that contract without leaking secrets.
- Shared read models/services stay isolated from Razor `PageModel` and HTML-facing view-model types while preserving projected, read-only EF Core query patterns.
- Metadata/resources/diagnostics are discoverable through MCP-visible resources plus a read-only diagnostics tool.
- Result caps and EF command timeout are explicit in config/code and echoed through diagnostics.
- Regression evidence is credible: solution build passed, MCP-focused tests passed 11/11, leaderboard regression subset passed 26/26, and the full suite passed 361/361.

### Notes

- Approval assumes later milestones keep the same boundary: additive MCP host, bounded read-only contracts, and no hidden HTTP/public exposure.
# Lambert — M1 MCP / shared-query review gate

**Date:** 2026-06-09  
**Status:** Proposed acceptance gate for issues #22-#24

## Baseline I used

- Current solution already contains web, tests, Aspire AppHost, and service defaults — no MCP project yet.
- Validation on the current tree is green: solution build passes and the full regression suite passes at **350/350**.
- Existing regression coverage is strongest around:
  - HTML surfaces for Players, Teams/Franchise, Team Season, Search, Batting, Pitching, support pages
  - Pagination boundary behavior
  - REST smoke tests for search, player detail, team season, and franchise 404s
  - PostgreSQL runtime/config contract and health-route behavior

## M1 acceptance gate

I will pass M1 only if **all** of the following are true after Parker and Ash finish:

### 1. New host project is real and solution-safe

- `baseball-history-mcp` exists and is added to `baseball-history.sln`
- The project starts locally without crashing
- The chosen v1 transport is **explicit** in code/config/docs, not implied
- M1 stays **stdio-first**; if HTTP/Streamable HTTP shows up in this milestone, I reject scope drift
- `ConnectionStrings:Lahman` remains the only runtime DB contract; the web app contract must not change
- Full solution build remains green after the new project is wired in

### 2. Shared query seam is actually shared, not web leakage in disguise

- No MCP-facing contracts use Razor `PageModel` types
- No MCP-facing contracts use HTML view models from `baseball-history-web/ViewModels/**`
- New query abstractions stay read-only and projection-first, matching existing PostgreSQL + EF Core NoTracking patterns
- Shared contracts cover, at minimum:
  - player lookup/listing shape
  - franchise lookup/detail shape
  - leaderboard-style reads with season/career semantics

**Reviewer check:** if I see `Pages/**`, `PageModel`, or HTML `ViewModels/**` crossing into MCP contracts, I reject.

### 3. Existing behavior still matches current public contracts

Because the seam extraction will likely touch duplicated query logic, I want proof that the old surfaces still behave the same:

#### Players
- `/Players` full page, HTMX partial, boosted navigation still behave the same
- `/api/players` pagination clamp behavior still holds
- Player modal links/contracts remain intact

#### Franchise / teams
- `/Teams/Franchise/{id}` full page and HTMX partial still render unchanged
- `/api/teams/franchises/{id}` still returns valid franchise detail / 404 behavior

#### Leaderboards
- Batting and pitching pages still preserve filter, pagination, and partial/full-page behavior
- Pitching still preserves **ascending** semantics for ERA/WHIP and descending semantics for counting stats
- Any new shared leaderboard query must preserve single-season vs career shape differences

### 4. MCP-visible metadata/resources are reviewable, not hand-waved

- Server metadata/version is discoverable from the MCP surface
- Diagnostic output/resource clearly reports startup/configuration state
- Supported stat categories, season expectations, result caps, and timeout limits are exposed through MCP-visible content
- Missing database configuration failure modes are documented and testable

### 5. Regression evidence is upgraded where the current suite is thin

Current tests do **not** directly cover the future MCP host/resources, and they do not directly exercise API leaderboard endpoints. Before M1 passes, I want at least one of these:

1. New automated tests covering the new MCP/server-info/shared-query contracts, **or**
2. A concise, reproducible smoke-test script/checklist showing those contracts on a live run

For the shared seam specifically, I prefer targeted automated checks for:
- player query output shape
- franchise query output shape
- leaderboard ordering semantics
- config/result-cap/timeout defaults

## Likely rejection triggers

- MCP project compiles but has no credible startup proof
- Transport choice is ambiguous or undocumented
- HTTP transport appears in an M1 branch that was supposed to stay stdio-first
- Shared seam reuses PageModel/view-model types as contracts
- Leaderboard extraction changes ERA/WHIP ordering semantics
- Existing web or API routes drift while “just” extracting queries
- Result caps/timeouts exist only as magic numbers with no visible documentation
- Missing `ConnectionStrings:Lahman` handling is opaque, misleading, or untested

## Reviewer note

M1 is low-risk only if the team treats this as **host addition + seam extraction + metadata contract work**, not “just add another executable.” The seam is where regressions will hide.
# Lambert — MCP M2 test/review gate

**Date:** 2026-06-09  
**Status:** ❌ REJECT UNTIL THESE PROOFS EXIST

## Baseline I used

- M1 host shape is already in place and should stay untouched for this review.
- Validation on the current tree is green: `dotnet build baseball-history.sln` passed, the full suite passed at **362/362**, and MCP-focused tests passed **12/12**.
- The question for M2 is not “does the repo still work?” The question is “have issues #25-#28 been fully proven through the MCP surface?”

## M2 pass criteria

I will pass M2 only if **all five** areas below are proven.

### 1. Discovery must be complete, not partial

M2 must prove:

- Player discovery is still bounded and deterministic.
- Franchise discovery is still bounded and deterministic.
- **Team-season lookup exists as an explicit MCP surface**; franchise detail alone does not satisfy issue #25.
- Invalid discovery inputs return normalized, MCP-friendly failures where validation should fail.

Concrete proof I need:

- Happy-path tests for `search_players`, `get_player`, franchise lookup/detail, and the new team-season tool.
- Deterministic ordering/paging assertions for discovery results.
- Invalid-input coverage for empty/invalid identifiers and malformed season lookup shapes.

Current status:

- **Partially covered today.**
- Existing MCP tests prove player prefix search, player detail, franchise listing, and page-size clamping.
- Missing proof: explicit team-season MCP contract and normalized invalid-input behavior.

### 2. Leaderboards must reject unsupported shapes and sort deterministically

M2 must prove:

- Supported stat names are **enumerated and validated**, not silently accepted with fallback behavior.
- Ordering semantics are correct:
  - batting rate/counting stats sort as intended
  - pitching `ERA` / `WHIP` / `BB9` preserve ascending semantics
- Tie-breaks are deterministic after the primary stat sort.
- Invalid season/minimum filters are explicitly handled and tested.
- Result caps remain bounded and discoverable.

Concrete proof I need:

- Tests that bad stat keys fail cleanly instead of defaulting to hits/wins.
- Tests for invalid year ranges and invalid minimum thresholds.
- Tests or explicit assertions that equal-stat rows use stable secondary ordering.
- Metadata/tool-description checks showing the same stat vocabulary clients are expected to send.

Current status:

- **Partially covered today.**
- Existing tests already prove batting HR descending, pitching ERA ascending, and leaderboard page-size clamping.
- Current implementation still silently falls back on unknown stats and does not yet prove deterministic tie-break ordering.

### 3. Hall of Fame must be queryable as a bounded MCP surface

M2 must prove:

- Hall of Fame is available through dedicated MCP tooling/resources, not just boolean flags embedded in other payloads.
- The returned shape is bounded and deterministic.
- Tool/resource descriptions explain whether the data represents inductions, full voting history, category data, and/or computed vote percentages.

Concrete proof I need:

- At least one bounded Hall of Fame listing/query tool.
- At least one bounded voting-history/player-history tool if that is part of the shipped shape.
- Tests for paging/order caps plus invalid-player behavior.

Current status:

- **Missing on the MCP surface.**
- There is useful Hall of Fame logic elsewhere in the repo, but M2 is not done until the MCP host exposes it directly.

### 4. Salary history must be queryable as a bounded MCP surface

M2 must prove:

- Salary history or salary leaders are queryable through dedicated MCP tooling/resources.
- Payloads stay within defined caps.
- Sorting is deterministic.
- Descriptions explain what the salary rows represent (player-season salaries, team payroll season, yearly leaderboards, etc.).

Concrete proof I need:

- Bounded player salary history and/or salary leader tools.
- Tests for order, caps, and invalid player/team/year behavior.
- Explicit wording that keeps salary semantics understandable to MCP clients.

Current status:

- **Missing on the MCP surface.**
- The web/API project already shows likely source contracts, but that is not acceptance evidence for MCP by itself.

### 5. In-server guidance must teach the shipped workflows only

M2 must prove:

- The server contains workflow guidance/examples for common baseball research flows.
- Guidance vocabulary matches final tool/resource names and descriptions.
- Guidance clearly distinguishes supported query shapes:
  - player
  - franchise
  - team-season
  - leaderboard
  - Hall of Fame
  - salary
- Guidance does **not** promise unsupported workflows.

Concrete proof I need:

- A workflow-focused MCP resource (or equivalent discoverable surface) with concrete examples.
- Tests that the guidance mentions only shipped tools/resources and uses the same vocabulary as the server surface.

Current status:

- **Not enough today.**
- `server/info`, `stats-catalog`, and `diagnostics` are valuable metadata resources, but they are not yet workflow guidance for issue #28.

## Hard rejection triggers

- Team-season remains absent from the MCP tool/resource list.
- Invalid leaderboard stats still silently fall back.
- Leaderboard ordering has no deterministic tie-break contract.
- Hall of Fame or salary remain “documented for later” instead of shipped now.
- Workflow guidance mentions tools or query shapes the server still does not support.
- M2 lands with only broad/full-suite confidence and no targeted MCP regression evidence.

## Reviewer note

M2 is a **query-surface completion gate**. If the feature exists only in the web/API app, or only in someone’s head, or only as a generic metadata hint, I still fail the milestone.
# Lambert — MCP M2 review

Date: 2026-06-09
Status: ACCEPTED FOR HANDOFF

## Decision

Accept the completed MCP M2 implementation for handoff. The shipped MCP surface now credibly covers discovery, deterministic team-season lookup, curated leaderboard validation, Hall of Fame history, salary history, and in-server workflow guidance without promising unsupported capabilities.

## Evidence

- `baseball-history-mcp/Tools/BaseballReferenceTools.cs` exposes the expected read-only tools for player/franchise discovery, deterministic `get_team_season`, batting/pitching leaderboards, Hall of Fame, salary history/leaders, and diagnostics.
- `baseball-history-mcp/Resources/BaseballReferenceResources.cs` and `Metadata/BaseballMcpMetadataService.cs` publish matching workflow/info/guide resources, limits, unsupported shapes, and resource URIs that align with the registered surface and server instructions.
- Query services bound results and normalize inputs:
  - team-season lookup requires exact team/league/year normalization
  - leaderboards use enumerated stat catalogs plus deterministic secondary ordering
  - Hall of Fame and salary responses apply explicit caps from `BaseballMcpOptions`
- Focused regression evidence exists in `baseball-history-tests/Mcp/BaseballReadServiceTests.cs` and `BaseballMcpMetadataTests.cs`, and the broader validation on this tree passed:
  - `dotnet build baseball-history.sln`
  - `dotnet test baseball-history-tests`

## Residual caution

- The targeted suite credibly proves the bounded surface, but there is still more room to add explicit negative-case tests for every minimum-threshold validation branch. That is follow-up hardening, not a handoff blocker for M2 as shipped.
# Lambert — MCP M3 Final Docs Gate (2026-06-09)

**Status:** ✅ APPROVED

## Decision

Approve M3 final gate. The remaining MCP documentation mismatch is resolved.

## What I Verified

1. `docs/MCP-SERVER-PLAN.md` now publishes the full shipped v1 limit contract:
   - query timeout 30 seconds
   - `search_players` max 100
   - `list_franchises` max 50
   - `list_hall_of_fame_inductees` max 100
   - leaderboard max 100
   - `get_player_salary_history` max 20 seasons
   - `get_team_payroll` max 25 player rows
2. Those numbers match the actual contract sources:
   - `baseball-history-mcp/appsettings.json`
   - `baseball-history-mcp/Configuration/BaseballMcpOptions.cs`
   - `baseball-history-mcp/Metadata/BaseballMcpMetadataModels.cs`
   - `baseball-history-mcp/Metadata/BaseballMcpMetadataService.cs`
3. `README.md` and `docs/DEVELOPMENT.md` now point readers to the guide instead of restating a stale subset.
4. MCP-focused regression coverage remains green: `dotnet test baseball-history-tests --filter "FullyQualifiedName~baseball_history_tests.Mcp" --no-restore --nologo --logger "console;verbosity=minimal"` passed 25/25.

## Rationale

The blocker was contract drift in the shipped adoption docs. That drift is gone: the written v1 cap snapshot now matches the exported metadata contract and the surrounding entry-point docs no longer create a competing partial story.

## Consequences

- M3 can be treated as docs-aligned and ready to close from the regression/review gate perspective.
- Future doc edits should keep `server/info` and this published cap list synchronized, or explicitly defer concrete cap values to `server/info` instead of publishing a subset.
# Lambert — M3 Hardening/Review Gate (2026-06-09)

## Decision

Do not call MCP M3 ready until the server proves five things together: contract/integration coverage, non-leaky errors, centralized limits, explicit HTTP posture guidance, and adoption docs aligned with the shipped surface.

## Required proof for M3

### 1. Contract / integration coverage

- Prove the actual MCP surface, not just the underlying services.
- Tests must verify tool listing includes the shipped read-only tools and resource listing includes the three `baseball-history://server/*` resources.
- Tests must execute representative successful calls end to end for players, franchises, batting leaders, pitching leaders, and diagnostics.
- Tests must cover validation/error paths through the MCP-facing contract, not only direct service calls.
- The coverage must live in the existing `.NET` test workflow and be runnable with a normal repository test command.

### 2. Non-leaky errors

- Invalid inputs and runtime failures must return normalized, client-safe failures.
- Review must explicitly prove no MCP-visible failure leaks connection strings, database hosts, raw provider exceptions, stack traces, or EF/Npgsql implementation details.
- Diagnostics surfaces may report posture and reachability, but not secret material.

### 3. Centralized limits

- Query timeout and collection caps must come from one reviewed configuration source.
- Every collection-shaped tool/resource must enforce the same configured caps and report the applied page/limit behavior consistently.
- Tests must cover cap enforcement, page adjustment/clamping, and the behavior of oversized or otherwise expensive requests.
- Metadata/docs must expose the same limits the implementation enforces.

### 4. HTTP posture guidance

- v1 remains stdio-first unless the team explicitly chooses otherwise.
- If HTTP stays out of v1, the docs must say so clearly and explain why.
- If HTTP is proposed, the review gate must require explicit ASP.NET Core hardening guidance: allowed-host/host-filter posture, named CORS policy guidance, and an explicit exposure model. No implicit browser/public rollout.

### 5. Adoption docs

- README/development/MCP guide must agree on startup, `ConnectionStrings:Lahman`, local launch, sample client config, discovery flow, non-goals, and future expansion boundaries.
- At least one concrete stdio client configuration example must be documented.
- Docs must describe the shipped surface only; roadmap items must stay labeled as follow-on work.

## Current assessment

- **Partially proven:** the repo already has focused MCP tests for metadata and query services, and the MCP-targeted test slice passes (`12/12`) in the existing test project.
- **Not yet proven enough for M3:** current MCP tests stop at resource/service classes rather than proving protocol-level tool/resource discovery and invocation.
- **High-risk gap:** there is no proof yet that MCP-visible failures are normalized. During the full repository test run, raw Npgsql/PostgreSQL failure details and the Azure database host surfaced in failure output when the shared database hit connection-slot pressure.
- **Operational review note:** `dotnet build baseball-history.sln` passed, but the full repository suite did not stay green in one normal run (`349/362` passed, `13` failed) because the shared PostgreSQL environment hit connection-slot limits. The MCP slice (`12/12`) and the direct database subset (`20/20`) both passed in isolation, so M3 should require a deterministic, reviewer-runnable workflow rather than relying on isolated green subsets alone.
- **Docs posture:** the current tree already contains strong stdio-first adoption guidance, sample client config, and explicit non-goals/future-expansion framing. That content is close to gate-ready, but M3 should still require it to land in the same reviewed handoff as the shipped surface.

## Reviewer verdict

**M3 gate is HOLD / READY-WHEN-PROVEN.**

The likely finish line is clear, but approval should wait for protocol-surface contract tests, explicit sanitized error-path proof, and a stable reviewer workflow that remains green without relying on database connection luck.
# Lambert — MCP M3 Re-review (2026-06-09)

## Verdict

**❌ REJECT**

## Why

Parker's limit-contract revision resolves the original implementation blocker. Hall of Fame paging, salary-history item counts, and team-payroll item counts now flow through the shared `BaseballMcpOptions` / `BaseballMcpRequestPolicy` path, `server/info` publishes those caps, and both service-level plus protocol-level tests prove clamp reporting on the affected endpoints.

I am still not approving M3 because the adoption contract is not yet fully coherent in the checked-in docs. `docs/MCP-SERVER-PLAN.md` still has a `Configured limits in v1` section that enumerates only query timeout, player search, franchise list, and leaderboard caps, while omitting the shipped Hall of Fame, salary-history, and team-payroll limits now exposed by metadata and tests. That is better than the prior hidden-cap defect, but it is still reviewer-visible drift between the advertised static guide and the discoverable/runtime contract.

## What is now credible

- Build passed: `dotnet build baseball-history.sln --nologo`
- MCP-focused test slice passed: `25/25`
- Full repository suite passed: `375/375`
- Code/config path is aligned for the prior blocker:
  - `baseball-history-mcp/Configuration/BaseballMcpOptions.cs`
  - `baseball-history-mcp/Configuration/BaseballMcpRequestPolicy.cs`
  - `baseball-history-mcp/Querying/HallOfFameReadService.cs`
  - `baseball-history-mcp/Querying/SalaryReadService.cs`
- Metadata/tests now expose and verify the additional limits:
  - `baseball-history-mcp/Metadata/BaseballMcpMetadataService.cs`
  - `baseball-history-tests/Mcp/BaseballMcpMetadataTests.cs`
  - `baseball-history-tests/Mcp/BaseballReadServiceTests.cs`
  - `baseball-history-tests/Mcp/McpProtocolIntegrationTests.cs`

## Approval condition

Approve once the adoption docs stop partially enumerating the cap contract. Either:

1. list the Hall of Fame, salary-history, and team-payroll limits alongside the existing cap list in `docs/MCP-SERVER-PLAN.md`, or
2. remove the partial static enumeration and explicitly point readers to `baseball-history://server/info` as the sole authority for concrete shipped cap values.

Until then, the hardening/adoption proof is close, but not fully end-to-end.
# Lambert — MCP M3 Final Review (2026-06-09)

## Verdict

**❌ REJECT**

## Why

The M3 branch is much closer to handoff than the prior gate: protocol-level MCP coverage is now real, the stdio-first/HTTP no-go posture is explicit, adoption docs are coherent, and the current repository test workflow is green. I built the solution, ran the MCP-focused test slice successfully, ran the full repository suite successfully, and manually spot-checked a bad-database tool call to confirm the host returns a normalized generic failure instead of leaking provider/host details.

I am still rejecting because the **centralized limits contract is not credibly complete across the shipped v1 surface**:

1. `BaseballMcpOptions` only centralizes three caps: player search, franchise list, and leaderboard (`baseball-history-mcp/Configuration/BaseballMcpOptions.cs`).
2. `list_hall_of_fame_inductees` is a shipped paged tool, but its service still enforces a local hardcoded `const int maxPageSize = 100` instead of using the shared options/request-policy path (`baseball-history-mcp/Querying/HallOfFameReadService.cs`).
3. Server metadata/docs only publish those same three centralized caps (`baseball-history-mcp/Metadata/BaseballMcpMetadataModels.cs`, `docs/MCP-SERVER-PLAN.md`), so Hall of Fame paging is described as “bounded” without exposing the actual contract value.
4. Regression coverage proves clamp behavior for players, franchises, and leaderboards, but not the shipped Hall of Fame or salary collection endpoints (`baseball-history-tests/Mcp/BaseballReadServiceTests.cs`, `baseball-history-tests/Mcp/McpProtocolIntegrationTests.cs`).

## What is already credible

- Contract/integration coverage: yes, materially improved. Real stdio host tests now cover discovery plus representative players, franchise, team-season, batting, pitching, Hall of Fame, salary, diagnostics, invalid-input, and startup-failure paths.
- Non-leaky errors: acceptable posture based on current normalization behavior and spot checks.
- HTTP posture guidance: acceptable. The repo clearly keeps v1 stdio-only and documents the `AllowedHosts`/CORS hardening prerequisites before any HTTP revisit.
- Adoption docs: broadly aligned with the shipped v1 surface.

## Acceptance condition

Approve M3 once **every shipped collection contract** is aligned the same way:

- limits come from the reviewed shared config/policy path,
- metadata/resources/docs publish those limits consistently,
- and tests prove the applied clamp/adjustment behavior for the affected Hall of Fame and salary collection endpoints.
# Lambert — M3 MCP Contract Test Decision (2026-06-09)

## Decision

Treat the real stdio host as the primary contract boundary for MCP M3 coverage, not just the underlying read services.

## What this means

- Keep the existing read-service/metadata tests, but add protocol-level coverage that launches `baseball-history-mcp` the way a real client does.
- Verify `tools/list`, `resources/list`, representative `tools/call`, and `resources/read` flows against the live stdio host.
- Include sanitized invalid-input coverage and one startup-failure proof path that isolates user-secrets so placeholder connection strings are actually exercised.

## Why

Service tests alone miss the exact MCP surface clients bind to: discovery metadata, tool schemas, resource MIME types, and normalized MCP-visible failures. The stdio host in this repo speaks newline-delimited JSON-RPC cleanly enough that we can prove the real client contract without adding custom infrastructure or a separate sample app.
# Lambert — MCP test-drift review (2026-06-10)

## Verdict

**❌ REJECT**

## What I verified

- Current tree does **not** clear the first gate: `dotnet build baseball-history.sln --nologo` fails with 32 compile errors, all concentrated in `baseball-history-mcp`.
- Last known coherent MCP milestone still exists: commit `9cb62c8` builds cleanly and the MCP-focused slice passes `25/25`.
- Earlier MCP baselines (`2e63645`, `9de5358`) also build and pass their smaller MCP slices, so the break is not historical test debt; it is post-milestone merge damage plus contract drift.

## Why I am rejecting

The reported “7 failing contract assertions” are real symptoms, but they are **not** the top-level blocker on the checked-out tree. The MCP branch currently has duplicated and partially merged implementations across:

- `baseball-history-mcp/Configuration/BaseballMcpOptions.cs`
- `baseball-history-mcp/Metadata/BaseballMcpMetadataModels.cs`
- `baseball-history-mcp/Metadata/BaseballMcpMetadataService.cs`
- `baseball-history-mcp/Querying/HallOfFameReadService.cs`
- `baseball-history-mcp/Program.cs`

That means I cannot approve Parker’s repair as an end-to-end gate yet: the runtime surface is internally inconsistent before the protocol assertions even run.

## Audit of the 7 drift assertions

1. **`server/info.limits.hallOfFamePageSizeMax == 100`**  
   **Classification:** stale test expectation.  
   **Why:** current checked-in `appsettings.json` and updated metadata tests point to a Hall of Fame cap of **50**, not 100.

2. **`server/info.limits.salaryHistorySeasonCountMax == 20`**  
   **Classification:** integration response-shape mismatch.  
   **Why:** the newer MCP salary contract is trying to expose **`salaryHistorySeasonsMax` = 40** instead of the old `salaryHistorySeasonCountMax` field.

3. **`server/info.limits.teamPayrollPlayerCountMax == 25`**  
   **Classification:** hidden contract drift / response-shape mismatch.  
   **Why:** the checked-in tree is inconsistent here. `BaseballReferenceTools.cs` no longer exposes `get_team_payroll`, but old payroll limit fields still linger in options/request-policy/metadata fragments. Parker must choose one contract and make tools, metadata, and tests agree.

4. **`list_hall_of_fame_inductees(...).maxPageSize == 100`**  
   **Classification:** stale test expectation.  
   **Why:** the Hall of Fame page cap has moved to **50**.

5. **`get_salary_leaders(...).maxPageSize == 100`**  
   **Classification:** stale test expectation.  
   **Why:** salary leaders now appear to use their own cap of **50**, not the generic leaderboard cap of 100.

6. **`get_player_salary_history(...).maxItemCount == 20`**  
   **Classification:** stale test expectation.  
   **Why:** the newer salary-history limit is **40** seasons.

7. **`get_team_payroll(...).maxItemCount == 25`**  
   **Classification:** hidden contract drift, not approvable as-is.  
   **Why:** this assertion only makes sense if `get_team_payroll` is still part of the shipped MCP surface. The current tree sends mixed signals, so this cannot be waived as “just a stale test” until the surface is made coherent.

## Approval conditions

I will approve only after all of the following are true:

1. `baseball-history-mcp` compiles cleanly again with one coherent implementation per option/model/service.
2. Parker’s repair makes a single explicit decision about payroll:
   - either keep `get_team_payroll` and publish/test its limit metadata,
   - or remove it end to end from tools, metadata, and protocol expectations.
3. The MCP-focused slice passes on the repaired tree, not just on the old milestone commit.
4. No remaining metadata/test mismatch exists around renamed limit fields (`salaryHistorySeasonCountMax` vs `salaryHistorySeasonsMax`, etc.).
# Parker — MCP M1 Foundation (2026-06-09)

**Author:** Parker  
**Status:** ✅ COMPLETE

## Decision

Stand up `baseball-history-mcp` as a dedicated stdio-first MCP host, and keep its read surface isolated behind read-only query services/records that depend on `BaseballDbContext` but not Razor PageModels or HTML view models.

## Why

- **Stdio first for v1:** the MCP issue explicitly scoped transport to stdio-first, and that keeps startup simple while the tool surface is still read-only.
- **No quiet web-contract leakage:** MCP tools now return purpose-built records instead of Razor view models or page-specific handler shapes.
- **Surgical migration path:** the existing web/API behavior stays untouched; MCP gets its own backend seam without forcing a wider refactor of the web app.

## What changed

1. Added `baseball-history-mcp` to the solution using the official `ModelContextProtocol` C# SDK.
2. Preserved the existing `ConnectionStrings:Lahman` contract and shared user-secrets flow.
3. Added reusable read-only services/records for:
   - player lookup + player detail
   - franchise lookup + franchise detail
   - batting and pitching leaderboard reads
4. Exposed those reads as MCP tools over stdio.
5. Added regression tests around the new query seam.

## Consequences

- Local MCP startup is `dotnet run --project baseball-history-mcp` rather than an Aspire-managed HTTP resource.
- If/when HTTP transport is needed later, the read services can be reused without changing MCP tool contracts.
- Web endpoints remain the source of existing HTTP behavior; MCP is a separate consumer of the shared read seam.
# Parker — MCP M2 query-surface contracts (2026-06-09)

**Author:** Parker  
**Status:** ✅ COMPLETE

## Decision

Close the remaining M2 MCP acceptance gaps by keeping the MCP host on dedicated read services, adding deterministic exact team-season lookup, validating leaderboard stat/filter inputs through a shared catalog, and surfacing Hall of Fame and salary reads behind explicit server-configured caps.

## What Changed

1. Added `get_team_season` as an exact `teamId + league + year` lookup so MCP clients do not rely on fuzzy franchise-era matching.
2. Normalized invalid input handling to `McpProtocolException` with `InvalidParams` for bad ids, bad pages, negative minimums, and impossible year ranges.
3. Hardened discovery lookups by normalizing ids/codes and making player search handle full-name token queries while rejecting conflicting filters.
4. Added bounded Hall of Fame and salary read services/tools plus guide resources that document coverage and payload caps.
5. Centralized leaderboard stat definitions so metadata, aliases, validation, and deterministic tie-breaking stay in sync.

## Rationale

- MCP clients need explicit contract failures, not silent clamping or fallback stats, when parameters are malformed.
- Team-season reads are only deterministic when league is part of the key.
- Shared stat catalogs prevent the metadata surface from drifting away from actual server validation logic.
- Hall of Fame and salary history are high-value read surfaces, but they still need hard caps and self-describing resources to stay LLM-safe.

## Consequences

- New MCP tools/resources expand the read surface without changing Razor or web API behavior.
- Invalid MCP inputs now fail consistently with machine-friendly protocol errors.
- Future leaderboard additions should extend the shared stat catalog first, then tools/resources/tests.
# Parker — M3 collection limit contract fix (2026-06-09)

## Decision

Keep Hall of Fame and salary collection bounds in the same MCP contract path as the earlier capped endpoints: one centralized options object, one request-policy layer, published `server/info` metadata, and response payload fields that report when the server clamped a requested collection size.

## Why

- The rejected M3 artifact left Hall of Fame with a hidden local cap and left salary collection bounds under-described.
- Clients need one discoverable place to learn the shipped limits before they call tools.
- Clamp reporting should be machine-readable on every bounded collection surface, not inferred from item counts alone.

## Applied shape

- Added centralized MCP options for Hall of Fame page size, salary-history season count, and team-payroll player count.
- Routed Hall of Fame paging and salary collection bounds through `BaseballMcpRequestPolicy`.
- Published the full collection-cap set through `baseball-history://server/info` and diagnostics metadata.
- Added regression coverage for Hall of Fame, salary leaders, player salary history, and team payroll clamp behavior.
# Parker — MCP compile fix triage (2026-06-10)

## Decision
Treat the current MCP compilation break as merge damage, not a feature gap: keep the active `ITeamReadService`/`get_team_season` path, remove the duplicate unused `ITeamSeasonReadService` path, and repair corrupted service/catalog files by re-establishing one coherent implementation per contract.

## Why
- The compiler errors came from duplicated constructor parameters, duplicated method bodies, and duplicate type definitions in MCP service code.
- The duplicate `TeamSeasonReadModel` and extra `TeamSeasonReadService` path were not required by the shipped tool surface, while `ITeamReadService` already owned `get_team_season`.
- Removing the dead duplicate path was smaller and safer than trying to keep two overlapping server-side contracts alive.

## Verification
- `dotnet build baseball-history-mcp/baseball-history-mcp.csproj --no-restore`
- `dotnet build baseball-history.sln --no-restore`

## Note
Focused only on restoring clean compilation. MCP integration tests still show separate contract/test drift outside the compile break itself.
# Ripley — MCP M1 Approval & Guardrails (2026-06-09)

## Decision

Approve **MCP M1: Foundation & Contracts** with a **dedicated `baseball-history-mcp` project** and a **stdio-first** rollout.

## What this enforces

- MCP work stays out of `baseball-history-web`
- M1 focuses on local protocol and contract validation, not network hosting
- shared read-only query services/records are the seam for reuse
- metadata, diagnostics, stat limits, and result caps must be MCP-visible in M1

## Explicit non-goals for M1

- HTTP / Streamable HTTP transport
- `ModelContextProtocol.AspNetCore`
- Aspire orchestration for MCP
- remote exposure, CORS, or auth hardening
- REST parity
- Razor `PageModel` or HTML view-model reuse as MCP contracts

## Acceptance gate

Do not hand M1 back as approved unless:

1. `baseball-history-mcp` is a separate project in the solution
2. transport is stdio-first
3. `ConnectionStrings:Lahman` remains the runtime database contract
4. shared query seam exists for player/franchise/leaderboard reads
5. metadata + diagnostics + stat-catalog surfaces are discoverable
6. result caps/timeouts are explicit
7. solution build and regression suite stay green

## Artifacts

- `docs/MCP-SERVER-PLAN.md`
- GitHub issues `#21`-`#24`
# Ripley — MCP M1 Final Review

## Decision

**Status: REJECT for handoff**

MCP M1 is close, but it does **not** clear the final lead gate yet because the bounded-query-surface requirement is not consistently enforced.

## What passed

- `baseball-history-mcp` exists as a dedicated project and is wired into `baseball-history.sln`.
- The host is stdio-first in practice (`WithStdioServerTransport`) and starts locally without introducing HTTP MCP hosting.
- The runtime database contract remains `ConnectionStrings:Lahman`.
- Query contracts use projected MCP read models/services rather than Razor `PageModel` or HTML view-model types.
- Metadata, diagnostics, and stat-catalog discovery are present through MCP-visible resources/tools.
- Solution build is green, and the regression suite passed in this environment (`361/361`).

## Why I rejected handoff

The M1 plan explicitly says:

- no unbounded list or query-shaped tool contracts
- every collection-shaped tool/resource must expose an explicit limit or page contract with a hard cap

`list_franchises` currently returns an unpaged collection with optional filters but **no explicit limit parameter, paging contract, or surfaced hard cap**. That breaks the bounded-query-surface rule, which is one of the non-negotiable M1 gates.

## Required follow-up

Before M1 is approved, the franchise listing surface must become explicitly bounded in the contract (for example, limit + hard cap or paging + hard cap) and that bound must be discoverable alongside the existing metadata/diagnostics story.
# Ripley — MCP M1 rereview

**Status:** ✅ APPROVED

## Decision

Approve MCP M1 after Ash's franchise-list revision.

## Why

- The earlier blocker is genuinely resolved: `list_franchises` now has explicit `page`/`pageSize` inputs, clamps against a configured `FranchiseListPageSizeMax`, and reports applied paging metadata back to clients.
- The hard cap is no longer hidden; it is also surfaced through server info/diagnostics metadata and pinned by MCP-focused tests.
- The overall M1 shape remains coherent with the approved gate: dedicated `baseball-history-mcp` host, stdio-only transport, read-only query services/records, explicit limits/timeouts, and no Razor page-model or HTML-contract reuse.

## Verification summary

- Solution includes a separate `baseball-history-mcp` project and keeps transport stdio-first.
- Runtime database contract remains `ConnectionStrings:Lahman`.
- MCP resources/tools expose discoverability for server info, diagnostics, stat catalog, and result caps.
- Local validation passed: solution build green, full regression suite green, MCP tests included.

## Consequence

M1 is ready to hand off as an additive foundation milestone. Follow-on work can build on this host without reopening the original bounded-surface blocker.
---
author: Ripley
date: 2026-06-09
status: approved
milestone: MCP M2
---

# Ripley — MCP M2 Final Lead Review

## Decision

Approve MCP M2 for milestone scope.

## Why

The shipped v1 surface is coherent and stays inside the issue bundle (#25-#28):

- identity/discovery: player, franchise, and exact team-season reads
- curated leaderboards: batting and pitching with enumerated stats
- bounded historical adds: Hall of Fame and salary history/leaders
- in-server guidance: workflow, stats catalog, Hall of Fame guide, salary guide, server info/diagnostics

The surface is deterministic where it matters for milestone acceptance:

- enum-like leaderboard stats are centralized and validated
- paging is capped and reports clamp/adjust metadata
- exact-id reads normalize casing before lookup
- list/leaderboard outputs use explicit ordering and secondary tie-breakers
- invalid inputs fail through normalized MCP invalid-params behavior

## Verification

- `dotnet build baseball-history.sln` ✅
- `dotnet test baseball-history-tests --filter "FullyQualifiedName~baseball_history_tests.Mcp"` ✅ (23/23)
- Reviewed tool/resource registration, workflow/resource vocabulary, and focused MCP tests against issues #25-#28 ✅

## Acceptance Notes

- Workflow guidance does the right job: it routes clients to shipped tools/resources and explicitly names unsupported shapes instead of implying open-ended baseball research.
- The milestone closes acceptance gaps without reopening M1 transport/host/runtime-contract decisions.
- No implementation changes required from lead review.
# Ripley — MCP M2 Scope Confirmation

**Date:** 2026-06-09
**Status:** Proposed for team alignment

## Decision

Treat MCP M1 as complete and stable. MCP M2 should **not** revisit host shape, transport, or the already-shipped bounded player/franchise/leaderboard foundation. M2 is strictly the acceptance-gap milestone for issues #25-#28.

## What M1 already covers

- Separate `baseball-history-mcp` host exists and runs stdio-first.
- Runtime contract remains `ConnectionStrings:Lahman`.
- Read-only query seams already ship for player discovery/detail, franchise discovery/detail, batting leaders, pitching leaders, and server diagnostics/metadata resources.
- Metadata resources already expose server info, diagnostics, stat catalog, limits, and startup guidance.

## Exact M2 gaps to close

1. **Issue #25 — Discovery**
   - Add deterministic **team-season discovery** as an explicit MCP surface; franchise history alone is not enough.
   - Add normalized MCP-friendly validation/errors for invalid discovery inputs instead of silent fallback or null-only behavior where validation should fail.

2. **Issue #26 — Leaderboards**
   - Replace freeform/silent-fallback leaderboard stat handling with enumerated, validated inputs.
   - Add deterministic tie-break ordering after the primary stat sort.
   - Validate invalid season/minimum filters explicitly and cover them in tests.

3. **Issue #27 — Hall of Fame / Salary**
   - Expose bounded Hall of Fame query tools/resources.
   - Expose bounded salary-history or salary-leader tools/resources.
   - Add descriptions/resources that explain what those payloads represent and any season/caveat boundaries.

4. **Issue #28 — Workflow guidance**
   - Add in-server workflow guidance/examples for the supported shapes only.
   - Keep guidance vocabulary aligned with shipped tool names/descriptions.
   - Do not document unsupported workflows until the backing tools exist.

## Rationale

The owner confirmed M2 is the “finish the bounded query surface for v1” milestone, not a second architecture pass. That means review should focus on missing acceptance behavior and discoverability, not on reworking M1’s host/project boundaries.
# Ripley — MCP M3 Documentation Decision (2026-06-09)

## Decision

Convert the existing `docs/MCP-SERVER-PLAN.md` path from a planning artifact into the shipped MCP v1 usage/adoption guide, and anchor the guide to the server's actual exported stdio surface rather than earlier milestone scope.

## Rationale

- README already linked this path, so keeping the file location avoids churn while changing the content from planning to operational guidance.
- The authoritative contract for docs is now the code and MCP-focused tests: seven read-only tools, three metadata resources, `ConnectionStrings:Lahman`, and explicit timeout/page caps.
- Planning language that mentions broader baseball domains or future hosting shapes would over-promise against the v1 surface now in the repository.

## Consequences

- Contributors get one stable doc path for local setup and client configuration.
- Client authors are told exactly what is supported today and what remains out of scope.
- Future MCP scope expansion should update docs only after new tools/resources are actually shipped and validated.
# Ripley — MCP M3 rereview (2026-06-09)

## Decision

Approve MCP M3 on rereview.

## Why

The prior blocker is genuinely resolved. Hall of Fame and salary limits are no longer hidden or service-local:

- shared limit contract: `baseball-history-mcp/Configuration/BaseballMcpOptions.cs`
- shared enforcement path: `baseball-history-mcp/Configuration/BaseballMcpRequestPolicy.cs`
- Hall of Fame enforcement: `baseball-history-mcp/Querying/HallOfFameReadService.cs`
- salary enforcement: `baseball-history-mcp/Querying/SalaryReadService.cs`
- published metadata contract: `baseball-history-mcp/Metadata/BaseballMcpMetadataService.cs`

That same contract is now visible to clients through `baseball-history://server/info` and `baseball-history://server/diagnostics`, and the tests assert those exact values and clamp behaviors:

- metadata coverage: `baseball-history-tests/Mcp/BaseballMcpMetadataTests.cs`
- read-service coverage: `baseball-history-tests/Mcp/BaseballReadServiceTests.cs`
- protocol coverage: `baseball-history-tests/Mcp/McpProtocolIntegrationTests.cs`

The adoption story is also coherent with the shipped v1 surface:

- `README.md` points contributors to the MCP guide instead of roadmap language
- `docs/DEVELOPMENT.md` documents local stdio startup
- `docs/MCP-SERVER-PLAN.md` now functions as the v1 guide, keeps HTTP out of v1, and tells clients to start from `server/info` / guide resources instead of assuming broader scope

## Verification

- `dotnet build baseball-history.sln` ✅
- `dotnet test baseball-history-tests --filter "FullyQualifiedName~baseball_history_tests.Mcp"` ✅

## Acceptance note

I am accepting this because the client-visible contract authority is now the shipped metadata surface, not the older planning prose. Future reviews should still reject any new capped collection if its limit lands only in service code and not in metadata/tests/client guidance.
# Ripley — MCP M3 Final Review

**Date:** 2026-06-09  
**Status:** REJECT / needs one follow-up hardening pass

## Decision

Do **not** approve MCP M3 yet.

## Why

The branch is close: the shipped MCP surface is now explicit, stdio-only posture is clearly documented, protocol-level tests exercise the real tool/resource contract, and the full repository build/test gate is green.

But the hardening/adoption story is still missing one contract detail on the shipped surface:

- `list_hall_of_fame_inductees` returns a paged collection and clamps page size to a hardcoded max of **100** inside `HallOfFameReadService`.
- That cap is **not** centralized with the other exposed MCP limits, **not** surfaced by server metadata, and **not** documented in the MCP adoption docs.

That means the branch is not fully coherent against the actual shipped MCP contract yet: some client-visible bounds are discoverable and documented, while at least one shipped collection tool still relies on an internal-only limit.

## Required follow-up for approval

Choose one path and keep it consistent end to end:

1. **Preferred:** move the Hall of Fame list cap into the same central MCP limit contract used by other paged tools, then surface it through metadata/tests/docs; or
2. **Fallback:** narrow the public docs/metadata language so it no longer implies the v1 bounded-query contract is fully discoverable across the whole shipped surface.

Approval should wait for the first option unless the team intentionally wants a smaller documented contract.
# Ripley — MCP M3 Scope Confirmation

**Date:** 2026-06-09  
**Status:** Proposed for team alignment

## Decision

Treat **MCP M3: Hardening & Adoption** as a hardening/documentation pass over the **actually shipped MCP v1 surface in this repository**, not as a stealth expansion milestone.

## Shipped surface confirmed in code

- `baseball-history-mcp/Program.cs` keeps the host **stdio-only** via `WithStdioServerTransport()`.
- `baseball-history-mcp/Tools/BaseballReferenceTools.cs` currently exposes:
  - `search_players`
  - `get_player`
  - `list_franchises`
  - `get_franchise`
  - `get_batting_leaders`
  - `get_pitching_leaders`
- `baseball-history-mcp/Tools/BaseballServerDiagnosticsTools.cs` adds:
  - `get_server_diagnostics`
- `baseball-history-mcp/Resources/BaseballReferenceResources.cs` currently exposes:
  - `baseball-history://server/info`
  - `baseball-history://server/stats-catalog`
  - `baseball-history://server/diagnostics`

That is the contract M3 must test, harden, and document.

## What M3 should not assume

- Do **not** scope M3 around team-season lookup, Hall of Fame tools, salary tools, or workflow-guide resources unless those surfaces are first present in checked-in code.
- Do **not** treat optional HTTP as implied implementation work. The host is still stdio-only, so HTTP is a review/documentation decision unless the team explicitly opens a new delivery milestone.

## Exact acceptance gaps for #29-#31

### #29 — MCP contract and integration test coverage

Current tests hit read services and metadata services directly, but they do **not** yet prove the server contract the way MCP clients consume it.

Required gaps:
1. Add **tool-listing** coverage against the registered MCP server surface.
2. Add **resource-listing** coverage against the registered MCP server surface.
3. Add representative **successful MCP-surface calls** for the shipped tools/resources, not only underlying service classes.
4. Add **validation/error-path coverage** for bad inputs and failure behavior.
5. Keep the work inside the existing `.NET` test project/setup.

### #30 — Limits, error handling, and optional HTTP posture

Current code already centralizes timeout/page-size settings in `BaseballMcpOptions`, but the hardening story is incomplete.

Required gaps:
1. Replace **silent fallback behavior** on invalid leaderboard stats with normalized MCP-friendly validation failures.
2. Normalize bad-input failures so tool responses do **not** leak raw implementation details.
3. Decide and document a clear **HTTP v1 go/no-go**. Given the current shipped host, the default recommendation is **no HTTP in v1**.
4. If HTTP is revisited later, document required **host validation/CORS hardening** as follow-on work instead of silently enabling transport now.

### #31 — Local setup, sample clients, and rollout path

Contributor-facing docs do not currently explain how to run or adopt `baseball-history-mcp`.

Required gaps:
1. Add local startup/config docs for `baseball-history-mcp`.
2. Include at least one **sample MCP client configuration**.
3. Explain **why v1 is stdio-first** in contributor-facing docs, not only planning notes.
4. Call out explicit **v1 non-goals** and future expansion areas (including optional HTTP).

## Rationale

Milestone acceptance has to track the contract users can actually install and run. If M3 is allowed to assume unshipped M2 surfaces, the tests and docs will drift from reality and we will harden the wrong thing.
# Ripley — MCP Server Planning Decision (2026-06-09)

## Decision

Plan the baseball statistics MCP work as a **dedicated `baseball-history-mcp` project** with a **stdio-first** v1 rollout.

## Rationale

- Keeping MCP in a separate host avoids coupling LLM protocol concerns to the existing Razor Pages and REST delivery surfaces.
- The MCP C# SDK guidance supports starting with `ModelContextProtocol` for stdio servers; HTTP transport through `ModelContextProtocol.AspNetCore` should be treated as optional follow-on scope because it introduces additional host-validation and CORS hardening concerns.
- The repository already has strong read-only EF Core projection patterns; those should be reused through shared query services/records rather than through Razor `PageModel` or HTML view model contracts.

## Consequences

- Initial implementation should optimize for local MCP clients first.
- Query surface should be curated (player, team/franchise, leaderboards, Hall of Fame, salary history) rather than aiming for immediate REST parity.
- HTTP transport and Aspire orchestration are adoption decisions to evaluate in the hardening milestone, not day-one requirements.

## Artifacts

- Plan doc: `docs/MCP-SERVER-PLAN.md`
- GitHub milestones: `MCP M1: Foundation & Contracts`, `MCP M2: Query Surface v1`, `MCP M3: Hardening & Adoption`
- GitHub issues: `#21`-`#31`
