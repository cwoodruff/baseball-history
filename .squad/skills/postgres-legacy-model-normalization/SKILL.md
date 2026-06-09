# PostgreSQL Legacy EF Model Normalization

## Context

Use when an EF Core model was scaffolded from SQLite (or another permissive store) but the runtime provider is now PostgreSQL and the app code still depends on legacy CLR property shapes.

## Pattern

1. Keep the runtime provider swap small at the composition root:
   ```csharp
   builder.Services.AddDbContext<BaseballDbContext>(options =>
       options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
   ```
2. Preserve the existing connection-string key so handlers/pages do not change (`ConnectionStrings:Lahman` here).
3. In `OnModelCreating`, normalize provider-specific scaffold metadata after entity configuration:
   - remove SQLite collations like `NOCASE`
   - clear explicit store-type annotations that were scaffolded for the old provider
   - add value converters for legacy `string` properties that now read numeric PostgreSQL columns
4. Validate with provider-level smoke tests that call `ToQueryString()` on representative queries; this catches translation/mapping issues even when a live database secret is unavailable.

## Why it works

This keeps page models, handlers, and view models stable while letting EF Core adapt to stricter PostgreSQL typing rules. It is especially useful when a full model re-scaffold would create unnecessary churn across a mature application.

## Baseball History references

- `baseball-history-web/Program.cs`
- `baseball-history-web/Models/BaseballDbContext.cs`
- `baseball-history-tests/Database/PostgreSqlModelTests.cs`
