# Ash — PostgreSQL doc/config follow-up

## Decision
Treat `ConnectionStrings:Lahman` as the single runtime contract everywhere, and document `lahman.db` only as historical migration/export input rather than an app startup requirement.

## Why
`Program.cs` now hard-fails without a real PostgreSQL connection string, so README/development docs must match runtime reality. Keeping local user-secrets, Azure App Service configuration, and Key Vault on the same key avoids drift between environments.

## Team Impact
- README now points to a real `docs/POSTGRES-MIGRATION.md`
- Local setup story is user-secrets first
- Azure setup story is App Service configuration with Key Vault reference preferred
- No real connection strings or passwords belong in git
