# Project Context

- **Owner:** ash
- **Project:** baseball-history
- **Role Summary:** Platform validation & guardrails lead: cache, migrations, and export validation.

## Core Context

ash has been contributing in their role: Platform validation & guardrails lead: cache, migrations, and export validation. Key facts condensed: regression safety & guardrails are authoritative for sprint gates; shell/primitives extraction progressed under guarded reviews; Lahman Postgres export artifacts produced and validated (2026-06-08).

## Recent Updates

✅ Zero-division errors → All calculated stats have conditional guards  

### Team Decision: Shared Expression Tree Extraction

**Status:** DEFERRED to Sprint 5

**Rationale:**
- Expression tree helpers duplicated between Batting/Pitching pages + API endpoints
- Sprint 4 design review explicitly locked against refactoring shared helpers
- Migration risk outweighs maintenance burden for 2 pages + 1 API endpoint

**Future Work:**
- Sprint 5: Extract to `Utilities/LeaderboardExpressions.cs`
- Add unit tests for zero-division edge cases

### Decisions Written

- `.squad/decisions/inbox/ash-sprint4-guardrails.md` — Full platform audit + 7 guardrails + constraints

### New Insights

- **Leaderboard two-stage materialization pattern is intentional** — Career mode GroupBy aggregates in DB, then fetches only paginated player names (100 max). This prevents loading 20k+ names into memory.
- **ERA/WHIP ascending sort is correct** — Lower is better. Expression trees use `double.MaxValue` for zero IP, which correctly sorts pitchers with no IP to the bottom when using ascending order.
- **Expression tree ordering is complex but sound** — Dynamic property name resolution, zero-division guards, calculated stat expressions all compile to SQL. Property name typos are the main risk (runtime exceptions).
- **Response cache TTL matches filter behavior** — 3600s (1 hour) is appropriate for leaderboard pages because filter options are cached for 24h. Queries run once/hour max per unique filter combination.

### Sprint 4 Approval

Issue #12 (Batting) and #13 (Pitching) are platform-safe to proceed. Parker must preserve all 7 guardrails during migration. No data-access architectural changes required.

**Post-merge validation:**
- Monitor response cache behavior under filter changes
- Verify ERA/WHIP ascending sort remains correct
- Check pagination boundary conditions (page=0, >maxPage)
- Validate cache hit rates for filtered queries

## Sprint 5 Cleanup & Documentation (2026-04-21)

### Cleanup Result
- Removed the dead `~/js/site.js` layout import after confirming `wwwroot/js/site.js` was empty and all shell lifecycle behavior already lived inline in `_Layout.cshtml`.
- Retained `rhx-button.css` and `rhx-badge.css` because About, Teams, Batting, and Pitching still render those htmxRazor components.

### Documentation Result
- Added cache follow-through notes to `README.md` and `docs/FRONTEND.md`, including the restart-based SOP for `lahman.db` refreshes.
- Documented that response cache separation by `HX-Request` remains the migration-critical guardrail for full-page vs partial responses.

### Backlog Decision
- Shared leaderboard ordering extraction is still not safe as “cleanup only” because Razor Pages and `/api/leaders` have drifted in alias/stat coverage.
- Any future extraction should be gated by parity tests, not bundled into UI migration polish.

## Sprint 5 Issue #15 Completion (2026-04-21)

**Status:** ✅ COMPLETED

Cache invalidation SOP documented, asset audit completed, dead-asset cleanup executed. Cache behavior and htmxRazor CSS usage clarified for future sprints.

### Key Deliverables
1. **Cache Invalidation SOP** — Documented 24-hour TTL strategy and query patterns
2. **Asset Audit** — Inventoried htmxRazor CSS imports; verified all active
3. **Dead-Asset Removal** — Removed unused `site.js` import
4. **Documentation Updates** — Cache patterns, asset lifecycle, component structure recorded

### Platform Guardrails Locked
- **Projection-first (CRITICAL)** — All EF queries materialize via `.Select()` in handler
- **Response cache metadata (CRITICAL)** — All pages include `[ResponseCache(..., VaryByHeader="HX-Request")]`
- **Cache key consistency** — New pages use unique keys with 24h TTL
- **Shell authority** — `_ShellHeader.cshtml` + `_Layout.cshtml` own search/modal/boost

### Deferrals to Backlog
- Filter form extraction
- Search PageModel extraction (unless future sprint forces it)
- Leaderboard ordering extraction
- Standalone search redesign
- Support page copy/content polish

### Sprint 5 Gate Achievement
Audit complete. Platform stable. All guardrails locked. Ready for future sprints.


2026-06-08T23:55:53Z — Team update: Ash generated Postgres per-table INSERT exports; Lambert reviewed and approved; Coordinator updated identity now.

## Learnings

### 2026-06-08 — Lahman PostgreSQL schema export
- The live `/Users/cwoodruff/Git/baseball-history/lahman.db` database is the authoritative source for Lahman table shape; generating schema from checked-in SQL dumps risks drifting from current constraints.
- PostgreSQL-safe Lahman DDL works best when every identifier is double-quoted, SQLite `COLLATE NOCASE` clauses are dropped, and composite primary-key columns are forced to `NOT NULL` even when SQLite metadata leaves them nullable.
- The generated schema deliverables now live in `database/postgres-schema/`, with a reusable generator at `scripts/generate_postgres_schema.py` and an ordered replay file at `database/postgres-schema/all_tables.sql`.
- `AllstarFull` should keep only the `playerID -> People.playerID` foreign key in Postgres because the live SQLite source does not enforce a team foreign key for historical all-star rows.

2026-06-08T23:55:53Z — Team update: Ash generated Postgres-compatible per-table CREATE TABLE scripts from /Users/cwoodruff/Git/baseball-history/lahman.db into `database/postgres-schema/` and added `scripts/generate_postgres_schema.py`.

## 2026-06-09 — PostgreSQL Migration Configuration Guidance

**Status:** ✅ COMPLETED

Created complete configuration documentation for PostgreSQL migration, safe for both current state (SQLite) and post-migration (Postgres). Work done:

### Deliverables

1. **New**: `docs/POSTGRES-MIGRATION.md` (8.8 KB)
   - Local dev setup via User Secrets (`dotnet user-secrets`)
   - Azure deployment via App Service Configuration + Key Vault + Managed Identity
   - Security architecture clearly defined per environment
   - Troubleshooting guide for connection string issues
   - Migration path for developers

2. **Updated**: `docs/DEVELOPMENT.md`
   - Database Setup section now covers both SQLite and PostgreSQL contexts
   - Configuration section includes User Secrets example for PostgreSQL
   - Cross-references POSTGRES-MIGRATION.md

3. **Updated**: `README.md`
   - Technology Stack clarified: "SQLite → PostgreSQL (migrating)"
   - Documentation index now includes POSTGRES-MIGRATION.md
   - Migration Runtime Notes separated current behavior from PostgreSQL context

4. **Decision**: `.squad/decisions/inbox/ash-postgres-config.md`
   - Documented configuration pattern for team
   - Rationale for User Secrets + Key Vault approach
   - Safe for pre-Parker state; no changes needed when Parker merges his app changes

### Platform-Level Guarantees

- ✅ **No secrets in repo**: Connection strings with credentials never committed; User Secrets + Key Vault architecture explicitly documented
- ✅ **Safe now and after migration**: Docs describe what will happen; Parker's app changes don't require doc updates
- ✅ **Environment-aware**: Clear responsibility matrix (local dev = User Secrets, Azure = Key Vault, fallback = appsettings.json)
- ✅ **Troubleshooting coverage**: Connection failures traced to root causes with fixes

### Configuration Hierarchy (Locked)

When app switches to PostgreSQL, configuration resolution is:
1. **Local dev**: User Secrets (via `dotnet user-secrets set "ConnectionStrings:Lahman" "...";`)
2. **Azure**: Key Vault reference (via App Service config: `@Microsoft.KeyVault(VaultName=...;SecretName=Lahman-ConnectionString)`)
3. **Fallback**: appsettings.json value (development fallback only)

Application code (Program.cs) reads `builder.Configuration.GetConnectionString("Lahman")` — no change needed there.

### Ready for Parker

Parker's upcoming migration (SQLite → .UseNpgsql()) doesn't require doc updates because:
- Connection string key stays "Lahman"
- Configuration hierarchy unchanged
- Examples already show PostgreSQL patterns
- Developers onboarding post-merge get the full picture immediately


2026-06-09T12:45:00Z — Followed up PostgreSQL docs/config gap: created docs/POSTGRES-MIGRATION.md, aligned README + DEVELOPMENT + FRONTEND with the now-required ConnectionStrings:Lahman PostgreSQL runtime, clarified Azure App Service + Key Vault setup, and moved lahman.db guidance into historical migration-only context.

## 2026-06-09 — PostgreSQL validation follow-up

**Status:** ⚠️ BLOCKED ON SECRET ACCESS

- Re-verified the repo is wired for PostgreSQL runtime (`Program.cs` + `TestDatabaseFactory.cs`) and that `dotnet user-secrets` is already enabled for `baseball-history-web`.
- Confirmed no local `ConnectionStrings:Lahman` user-secret or environment variable was present in this session, so the web host and integration tests still fail fast before exercising the live Azure database.
- Validation results in this session:
  - ✅ `dotnet build baseball-history.sln`
  - ⚠️ `dotnet test baseball-history.sln --no-build` → **229 passed / 119 failed**, with failures caused by missing `ConnectionStrings:Lahman`, not by a demonstrated migration/runtime bug.
- Re-checked tracked repo files for hard-coded live PostgreSQL material; only placeholder/example connection strings are present.

2026-06-09T12:55:00Z — Attempted live PostgreSQL validation after Parker's migration/docs follow-up. Build passed, but this session had no accessible real `ConnectionStrings:Lahman` value in user-secrets or environment, so 119 integration tests still failed fast at startup and no live-db bug was reproduced.

## 2026-06-09 — `/Health` route ambiguity fix

**Status:** ✅ COMPLETED

- Root cause: ASP.NET Core endpoint matching is case-insensitive, so the Aspire/service-defaults readiness endpoint at `/health` conflicted with the Razor support page at `/Health` and produced `AmbiguousMatchException`.
- Fix: moved the machine-readable readiness endpoint from `/health` to `/healthz`, kept `/alive` unchanged, and left the intended `/Health` Razor page route intact.
- Added regression coverage for `/healthz` readiness and `/alive` liveness endpoints.
- Validation results:
  - ✅ Targeted health-route tests passed
  - ✅ `dotnet test baseball-history-tests --nologo` → **350/350 passed**

2026-06-09T12:55:00Z — Resolved the `/Health` vs `/health` routing collision by moving the service-defaults readiness probe to `/healthz`, preserving the support page at `/Health`, and confirming the full integration suite passes against PostgreSQL-backed tests.

## 2026-06-09 PostgreSQL Documentation & Health Route Fix Complete

### Summary
Completed PostgreSQL migration documentation and fixed health route ambiguity. Delivered commits `8a59a17` (docs) and `6a5f202` (health route).

### Documentation Work
1. **Created `docs/POSTGRES-MIGRATION.md`:** Comprehensive guide for local setup (User Secrets) and Azure deployment (Key Vault + Managed Identity)
2. **Updated `docs/DEVELOPMENT.md`:** Cross-reference to migration guide with SQLite→PostgreSQL context
3. **Updated `README.md`:** Technology stack reflects migration; documentation section includes POSTGRES-MIGRATION.md
4. **Configuration Pattern:** `ConnectionStrings:Lahman` as single runtime contract across all environments
5. **Security:** Connection strings never committed; credentials live in User Secrets (local) or Key Vault (Azure)

### Health Route Fix
1. **Problem:** `/Health` Razor page collided with `/health` machine readiness probe (case-insensitive routing)
2. **Solution:** Moved readiness endpoint to `/healthz` in baseball-history-servicedefaults
3. **Result:** `/Health` preserved for humans; `/healthz` for machines; `/alive` for liveness
4. **Impact:** No breaking changes; fully compatible with local, App Service, and Aspire scenarios

### Verification
- ✅ Documentation complete and verified
- ✅ Route ambiguity resolved
- ✅ Configuration contract consistent across environments
- ✅ No secrets exposed in docs

### Handoff Status
Documentation and routing changes accepted for merge.

