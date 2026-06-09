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

