using System.Text.Json;
using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Metadata;

public sealed class BaseballMcpMetadataService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IConfiguration configuration,
    IMemoryCache cache,
    IOptions<BaseballMcpOptions> options,
    ServerBuildMetadata buildMetadata)
{
    private const string SupportedYearSpanCacheKey = "baseball-history-mcp:supported-year-span";

    private static readonly IReadOnlyList<string> ToolNames =
    [
        "search_players",
        "get_player",
        "list_franchises",
        "get_franchise",
        "get_team_season",
        "get_batting_leaders",
        "get_pitching_leaders",
        "list_hall_of_fame_inductees",
        "get_hall_of_fame_voting_history",
        "get_player_salary_history",
        "get_team_payroll",
        "get_salary_leaders",
        "get_server_diagnostics"
    ];

    private static readonly IReadOnlyList<ServerResourceLink> ResourceLinks =
    [
        new("baseball-history://server/info", "Server Info", "Read server identity, startup requirements, and configured limits."),
        new("baseball-history://server/stats-catalog", "Stats Catalog", "Discover supported batting and pitching stat categories plus the supported year span."),
        new("baseball-history://server/diagnostics", "Server Diagnostics", "Inspect safe runtime posture, configured limits, and connectivity without exposing secrets."),
        new("baseball-history://server/transport-policy", "Transport Policy", "Read the v1 HTTP go/no-go recommendation and the MCP C# SDK guidance behind it."),
        new("baseball-history://guides/getting-started", "Getting Started Guide", "Start with discoverability and choose the right domain tools for common baseball questions."),
        new("baseball-history://guides/workflows", "Workflow Guide", "See representative multi-step MCP workflows that only reference shipped v1 tools and resources.")
    ];

    public async Task<ServerInfoDocument> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        var supportedYearSpan = await GetSupportedYearSpanAsync(cancellationToken);

        return new ServerInfoDocument(
            buildMetadata.Name,
            buildMetadata.Title,
            buildMetadata.Version,
            buildMetadata.Description,
            Transport: "stdio",
            HttpTransportEnabled: false,
            HttpTransportRecommendation: "No-go for v1. Keep the shipped server stdio-only until the team is ready to own explicit HTTP host validation and narrowly scoped CORS.",
            ReadOnly: true,
            ConnectionStringKey: "ConnectionStrings:Lahman",
            Limits: CreateLimitSnapshot(),
            ToolNames,
            ResourceLinks,
            StartupRequirements:
            [
                "ConnectionStrings:Lahman must be configured before the stdio server starts.",
                "Placeholder connection strings containing '<' are rejected at startup.",
                $"Player search page size is capped at {options.Value.Limits.PlayerSearchPageSizeMax} rows, franchise listing page size is capped at {options.Value.Limits.FranchiseListPageSizeMax} rows, Hall of Fame page size is capped at {options.Value.Limits.HallOfFamePageSizeMax} rows, batting/pitching/salary leaderboard page size is capped at {options.Value.Limits.LeaderboardPageSizeMax} rows, player salary history is capped at {options.Value.Limits.SalaryHistorySeasonCountMax} seasons, and team payroll is capped at {options.Value.Limits.TeamPayrollPlayerCountMax} player rows.",
                $"Database commands use a configured timeout of {options.Value.QueryTimeoutSeconds} seconds.",
                "HTTP transport is intentionally out of v1. The MCP C# SDK guidance requires explicit AllowedHosts host validation and restrictive CORS if browser access is intentionally enabled; see baseball-history://server/transport-policy.",
                "Client workflows should start with baseball-history://server/info plus the guide resources before calling domain tools."
            ],
            supportedYearSpan);
    }

    public async Task<StatsCatalogDocument> GetStatsCatalogAsync(CancellationToken cancellationToken = default) =>
        new(await GetSupportedYearSpanAsync(cancellationToken), LeaderboardStatCatalog.Categories);

    public TransportPolicyDocument GetTransportPolicy() =>
        new(
            CurrentTransport: "stdio",
            HttpEnabled: false,
            V1Recommendation: "No-go for v1. Keep baseball-history-mcp stdio-only.",
            DecisionDrivers:
            [
                "The shipped MCP surface is local, read-only, and stdio-first today.",
                "Issue #30 is a hardening milestone, not an HTTP expansion milestone.",
                "Partially enabling HTTP would add rollout and support burden without a committed browser or remote-hosting use case."
            ],
            SdkGuidance:
            [
                "The MCP C# SDK transport guidance says local HTTP servers should restrict AllowedHosts to loopback values instead of '*', because Kestrel does not validate Host headers by default.",
                "The SDK guidance also says CORS should only be enabled when browser-based cross-origin access is intentional, and that CORS is not a substitute for host validation.",
                "If sessions or resumability are enabled over HTTP, the SDK guidance requires additional CORS headers such as Mcp-Session-Id to be allowed and exposed."
            ],
            RevisitCriteria:
            [
                "A real remote-hosting requirement exists for the MCP server.",
                "The deployment shape owns explicit AllowedHosts configuration at every ingress layer.",
                "A narrowly scoped CORS allowlist and test coverage are ready for the intended browser clients."
            ]);

    public async Task<ServerDiagnosticsDocument> GetServerDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Lahman");
        var connectionStringConfigured = !string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains('<');
        var databaseReachable = false;

        if (connectionStringConfigured)
        {
            try
            {
                await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                databaseReachable = await context.Database.CanConnectAsync(cancellationToken);
            }
            catch when (!cancellationToken.IsCancellationRequested)
            {
                databaseReachable = false;
            }
        }

        return new ServerDiagnosticsDocument(
            buildMetadata.Name,
            buildMetadata.Version,
            Transport: "stdio",
            DatabaseProvider: "Npgsql",
            QueryTrackingBehavior: nameof(QueryTrackingBehavior.NoTracking),
            UsesPooledDbContextFactory: true,
            RetriesTransientFailures: true,
            ConnectionStringConfigured: connectionStringConfigured,
            DatabaseReachable: databaseReachable,
            ConnectionStringKey: "ConnectionStrings:Lahman",
            Limits: CreateLimitSnapshot(),
            SupportedYearSpan: databaseReachable ? await GetSupportedYearSpanAsync(cancellationToken) : null,
            ToolCount: ToolNames.Count,
            ResourceCount: ResourceLinks.Count);
    }

    public static string Serialize<T>(T document) => JsonSerializer.Serialize(document, JsonOptions.Value);

    private McpLimitSnapshot CreateLimitSnapshot() =>
        new(
            options.Value.QueryTimeoutSeconds,
            options.Value.Limits.PlayerSearchPageSizeMax,
            options.Value.Limits.FranchiseListPageSizeMax,
            options.Value.Limits.HallOfFamePageSizeMax,
            options.Value.Limits.LeaderboardPageSizeMax,
            options.Value.Limits.SalaryHistorySeasonCountMax,
            options.Value.Limits.TeamPayrollPlayerCountMax);

    private async Task<SupportedYearSpan?> GetSupportedYearSpanAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(SupportedYearSpanCacheKey, out SupportedYearSpan? cached))
        {
            return cached;
        }

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var span = await context.Teams
                .GroupBy(_ => 1)
                .Select(g => new SupportedYearSpan(
                    g.Min(t => (int)t.YearId),
                    g.Max(t => (int)t.YearId),
                    "Teams.yearID"))
                .SingleOrDefaultAsync(cancellationToken);

            cache.Set(SupportedYearSpanCacheKey, span, TimeSpan.FromHours(12));
            return span;
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private static readonly Lazy<JsonSerializerOptions> JsonOptions = new(() => new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    });
}
