# PostgreSQL Migration Guide

This application now runs against PostgreSQL and reads its database connection
from `ConnectionStrings:Lahman`. The web app fails fast at startup if that key
is missing or still contains placeholder text.

`lahman.db` is no longer a runtime dependency. Keep it only as a historical
source when regenerating PostgreSQL schema or data-export artifacts.

## Runtime Contract

- **Required key:** `ConnectionStrings:Lahman`
- **Consumed by:** `builder.Configuration.GetConnectionString("Lahman")`
- **Current provider:** Npgsql / PostgreSQL
- **Safe checked-in sample:** `baseball-history-web/appsettings.json` contains a
  placeholder only and must be overridden outside git

## Local Development

The `baseball-history-web` project already has a `UserSecretsId`, so you can set
local secrets without creating extra config files.

1. Provision or obtain a PostgreSQL database loaded with the Lahman schema/data.
2. Store the real connection string in user-secrets:

```bash
dotnet user-secrets set --project baseball-history-web \
  "ConnectionStrings:Lahman" \
  "Host=<server>;Port=5432;Database=lahman;Username=<user>;Password=<local-password>;SSL Mode=Require;Trust Server Certificate=true"
```

3. Start the app:

```bash
aspire start --apphost baseball-history-aspire/baseball-history-aspire.csproj
# or
dotnet run --project baseball-history-web
```

4. Verify connectivity at `/Health`.

### Notes

- Do not commit a real password or checked-in `appsettings.*` override.
- If you rotate the connection string, restart the web app or Aspire AppHost so
  the process reconnects with the new value.
- Environment variables also work locally via `ConnectionStrings__Lahman`, but
  user-secrets is the preferred developer workflow.

## Azure App Service + Key Vault

Recommended production pattern:

1. Enable a managed identity for the App Service.
2. Store the real PostgreSQL connection string in Azure Key Vault.
3. Grant the managed identity permission to read that secret.
4. In App Service **Configuration**, add an application setting named
   `ConnectionStrings__Lahman` whose value is a Key Vault reference:

```text
@Microsoft.KeyVault(VaultName=<vault-name>;SecretName=Lahman-ConnectionString)
```

5. Restart the app (or let App Service recycle it) after the setting resolves.

This keeps the application contract consistent: all environments still populate
`ConnectionStrings:Lahman`, but production secret material stays in Key Vault
instead of the repository or portal copy/paste history.

### Direct App Service Configuration

If Key Vault is temporarily unavailable, App Service can store the connection
string directly in an app setting named `ConnectionStrings__Lahman`. Treat that
as an exception path and move back to Key Vault for long-term operation.

## Historical `lahman.db` Context

The legacy SQLite file still matters for migration support work, but only as a
source artifact:

- generating PostgreSQL DDL under `database/postgres-schema/`
- validating export completeness during migration work
- replaying historical data conversion steps in docs/scripts

Do not document `lahman.db` as something a new runtime deployment must copy into
`baseball-history-web`; that guidance is now outdated.

## Troubleshooting

### `ConnectionStrings:Lahman must be set`

The runtime did not receive a real connection string.

- Re-run `dotnet user-secrets set --project baseball-history-web ...`
- Check App Service configuration for `ConnectionStrings__Lahman`
- Make sure the configured value no longer contains placeholder markers such as
  `<server>` or `<password>`

### Authentication or SSL failures

- Verify the PostgreSQL server, database, username, and password
- Confirm firewall/network rules allow the app host to reach PostgreSQL
- Use the server's required SSL settings (`SSL Mode`, certificate options)

### Azure Key Vault reference does not resolve

- Confirm the secret name matches the App Service reference exactly
- Verify the managed identity has permission to read secrets
- Restart the app after role or policy changes
