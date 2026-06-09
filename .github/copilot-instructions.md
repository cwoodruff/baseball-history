# Copilot Instructions for `baseball-history`

## Build and test commands

```bash
# Restore packages
dotnet restore

# Build the full solution
dotnet build baseball-history.sln

# Run the full test suite
dotnet test baseball-history-tests

# Run a single test class
dotnet test baseball-history-tests --filter "FullyQualifiedName~PitchingLeaderboardTests"

# Run a single test method
dotnet test baseball-history-tests --filter "FullyQualifiedName~HtmxExtensionsTests.IsHtmxRequest_WithHeader_ReturnsTrue"

# Preferred local startup: Aspire AppHost
aspire start --apphost baseball-history-aspire/baseball-history-aspire.csproj

# Inspect the AppHost-assigned dashboard and web endpoint
aspire describe --apphost baseball-history-aspire/baseball-history-aspire.csproj

# Standalone web startup still supported
dotnet run --project baseball-history-web
```

## High-level architecture

- The repository is a .NET 10 solution with three main projects:
  - `baseball-history-web`: the production ASP.NET Core Razor Pages app
  - `baseball-history-tests`: xUnit tests using `WebApplicationFactory<Program>` for integration coverage
  - `baseball-history-aspire`: a dev-only Aspire AppHost that orchestrates the web project locally without changing the web app's runtime contracts
- `baseball-history-web/Program.cs` wires together the full runtime: service defaults, EF Core SQLite access, memory cache, response compression, Razor Pages, `htmxRazor`, OpenAPI/Scalar, the minimal API endpoint map, and a startup step that enables SQLite WAL mode.
- The app has two delivery surfaces backed by the same `BaseballDbContext`:
  - Razor Pages under `baseball-history-web/Pages/**` for HTML UI
  - minimal APIs under `baseball-history-web/Api/Endpoints/**`, grouped in `Api/ApiEndpointExtensions.cs`
- Data access is centered on the large scaffolded `BaseballDbContext` in `baseball-history-web/Models/BaseballDbContext.cs`. Many Lahman tables use composite keys, several columns are stored as strings in the source database, and `DateOnly?` values are normalized through a converter there.
- The UI is intentionally in a migration state toward htmxRazor components: the shared shell lives in `Pages/Shared/_Layout.cshtml`, page-specific handlers live in `*.cshtml.cs`, reusable HTML fragments live in partials (`_*.cshtml`), and shared presentational fragments live in `Pages/Shared/Components`.
- The Players area shows the main runtime pattern end to end:
  - `Pages/Players/Index.cshtml.cs` builds a page-specific view model from EF projections
  - `Services/PlayerCacheService.cs` pre-warms and refreshes the default Players page in `IMemoryCache`
  - HTMX requests return partials while full/boosted navigation returns full pages
- API DTOs and Razor view models are deliberately separate. JSON contracts live in `Api/Dtos/**`; HTML-facing shaping and formatting live in `ViewModels/**`.

## Key conventions

- Prefer incremental htmx/htmxRazor adoption over rewrites. The app already runs with `hx-boost="true"` at the layout level, so shell behavior and globally needed scripts/styles belong in `Pages/Shared/_Layout.cshtml`, not inside page bodies that get swapped.
- For Razor Pages that support HTMX, use the existing request split:
  - non-boosted HTMX request: return a partial
  - boosted HTMX navigation or normal request: return the full page
  - use `Request.IsHtmxNonBoostedRequest()` from `Extensions/HtmxExtensions.cs` instead of duplicating header checks
- Keep response caching aligned with HTMX behavior. Pages that serve both full pages and partials use `ResponseCache` varying by `HX-Request` so boosted/full-page and fragment responses do not collide.
- Reuse the existing cache strategy before adding new ones:
  - 24-hour `IMemoryCache` entries for shared lookup data and warmed defaults
  - `PlayerCacheService` for the default Players landing page
  - restart app instances after replacing `lahman.db` so warmed caches rebuild against fresh data
- Query patterns matter:
  - the DbContext is configured globally as `NoTracking`
  - project only the columns needed for the page or DTO
  - aggregate in SQL where possible
  - when Lahman numeric fields are stored as strings, parse after materialization instead of putting unsupported conversions into LINQ
- Keep minimal API routing consistent with the existing structure: register endpoint groups in `Api/ApiEndpointExtensions.cs`, keep implementation in `Api/Endpoints/*Endpoints.cs`, and keep response records in `Api/Dtos/**`.
- Treat `baseball-history-aspire` as local orchestration only. It should reference the web app as a project resource and use the existing `/` health check; do not move Aspire-specific runtime concerns into `baseball-history-web` unless the change is explicitly about that.
- htmxRazor component assets are centralized in `_Layout.cshtml`. If a page starts using additional `rhx-*` components, add the required component CSS there so the styles survive boosted navigation.
- Team and league presentation is driven by shared components and team-color CSS variables rather than page-specific styling. Reuse components under `Pages/Shared/Components/**` before introducing new markup patterns.
- The SQLite connection defaults to `lahman.db`, and the web project is configured to copy `lahman.db` to output/publish. Be careful about working directory assumptions when changing startup, tests, or deployment behavior.
