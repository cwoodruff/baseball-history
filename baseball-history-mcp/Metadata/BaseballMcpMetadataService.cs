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
        "get_batting_leaders",
        "get_pitching_leaders",
        "get_server_diagnostics"
    ];

    private static readonly IReadOnlyList<ServerResourceLink> ResourceLinks =
    [
        new("baseball-history://server/info", "Server Info", "Read server identity, startup requirements, and configured limits."),
        new("baseball-history://server/stats-catalog", "Stats Catalog", "Discover supported batting and pitching stat categories plus the supported year span."),
        new("baseball-history://server/diagnostics", "Server Diagnostics", "Inspect safe runtime posture, configured limits, and connectivity without exposing secrets.")
    ];

    private static readonly IReadOnlyList<SupportedStatCategory> SupportedCategories =
    [
        new(
            "batting",
            true,
            true,
            [
                new("hr", "Home runs", ["homeruns"], "descending", "Ranks by total home runs."),
                new("h", "Hits", ["hits"], "descending", "Ranks by total hits."),
                new("r", "Runs", ["runs"], "descending", "Ranks by total runs scored."),
                new("rbi", "Runs batted in", [], "descending", "Ranks by total RBI."),
                new("sb", "Stolen bases", ["stolenbases"], "descending", "Ranks by total stolen bases."),
                new("2b", "Doubles", ["doubles"], "descending", "Ranks by doubles."),
                new("3b", "Triples", ["triples"], "descending", "Ranks by triples."),
                new("bb", "Walks", ["walks"], "descending", "Ranks by bases on balls."),
                new("g", "Games", ["games"], "descending", "Ranks by games played."),
                new("ab", "At-bats", ["atbats"], "descending", "Ranks by official at-bats."),
                new("avg", "Batting average", ["battingaverage"], "descending", "Ranks by hits divided by at-bats."),
                new("obp", "On-base percentage", [], "descending", "Ranks by on-base percentage using hits plus walks."),
                new("slg", "Slugging percentage", [], "descending", "Ranks by slugging percentage."),
                new("ops", "OPS", [], "descending", "Ranks by on-base percentage plus slugging percentage.")
            ]),
        new(
            "pitching",
            true,
            true,
            [
                new("w", "Wins", ["wins"], "descending", "Ranks by total wins."),
                new("l", "Losses", ["losses"], "descending", "Ranks by total losses."),
                new("so", "Strikeouts", ["strikeouts"], "descending", "Ranks by total strikeouts."),
                new("sv", "Saves", ["saves"], "descending", "Ranks by total saves."),
                new("cg", "Complete games", [], "descending", "Ranks by complete games."),
                new("sho", "Shutouts", [], "descending", "Ranks by shutouts."),
                new("ip", "Innings pitched", [], "descending", "Ranks by innings pitched workload."),
                new("g", "Games", ["games"], "descending", "Ranks by pitching appearances."),
                new("gs", "Games started", [], "descending", "Ranks by games started."),
                new("hr", "Home runs allowed", [], "descending", "Ranks by home runs allowed."),
                new("k9", "Strikeouts per nine", [], "descending", "Ranks by strikeouts per nine innings."),
                new("wpct", "Winning percentage", [], "descending", "Ranks by winning percentage."),
                new("era", "Earned run average", [], "ascending", "Ranks by earned run average; lower is better."),
                new("whip", "WHIP", [], "ascending", "Ranks by walks plus hits per inning pitched; lower is better."),
                new("bb9", "Walks per nine", [], "ascending", "Ranks by walks allowed per nine innings; lower is better.")
            ])
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
            ReadOnly: true,
            ConnectionStringKey: "ConnectionStrings:Lahman",
            Limits: CreateLimitSnapshot(),
            ToolNames,
            ResourceLinks,
            StartupRequirements:
            [
                "ConnectionStrings:Lahman must be configured before the stdio server starts.",
                "Placeholder connection strings containing '<' are rejected at startup.",
                $"Player search page size is capped at {options.Value.Limits.PlayerSearchPageSizeMax} rows, franchise listing page size is capped at {options.Value.Limits.FranchiseListPageSizeMax} rows, and leaderboard page size is capped at {options.Value.Limits.LeaderboardPageSizeMax} rows.",
                $"Database commands use a configured timeout of {options.Value.QueryTimeoutSeconds} seconds."
            ],
            supportedYearSpan);
    }

    public async Task<StatsCatalogDocument> GetStatsCatalogAsync(CancellationToken cancellationToken = default) =>
        new(await GetSupportedYearSpanAsync(cancellationToken), SupportedCategories);

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
            options.Value.Limits.LeaderboardPageSizeMax);

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
