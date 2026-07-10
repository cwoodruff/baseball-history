using System.Text.Json;
using baseball_history_mcp.Configuration;
using baseball_history_mcp.Querying;
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
    private const string TransportName = "http";

    private const string SupportedYearSpanCacheKey = "baseball-history-mcp:supported-year-span";
    private const string HallOfFameYearSpanCacheKey = "baseball-history-mcp:hall-of-fame-year-span";
    private const string SalaryYearSpanCacheKey = "baseball-history-mcp:salary-year-span";

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
        "get_salary_leaders",
        "get_server_diagnostics"
    ];

    private static readonly IReadOnlyList<SupportedStatCategory> SupportedCategories =
    [
        new("batting", SupportsCareer: true, SupportsSingleSeason: true, LeaderboardStatCatalog.GetBattingMetadata()),
        new("pitching", SupportsCareer: true, SupportsSingleSeason: true, LeaderboardStatCatalog.GetPitchingMetadata())
    ];

    private static readonly IReadOnlyList<ServerResourceLink> ResourceLinks =
    [
        new("baseball-history://server/info", "Server Info", "Read server identity, startup requirements, and configured limits."),
        new("baseball-history://server/workflow-guide", "Workflow Guide", "Choose the right tool or guide resource for player, franchise, team-season, leaderboard, Hall of Fame, salary, and diagnostics questions."),
        new("baseball-history://server/stats-catalog", "Stats Catalog", "Discover supported batting and pitching stat categories plus the supported year span."),
        new("baseball-history://server/diagnostics", "Server Diagnostics", "Inspect safe runtime posture, configured limits, and connectivity without exposing secrets."),
        new("baseball-history://hall-of-fame/guide", "Hall of Fame Guide", "Review Hall of Fame tool limits, year coverage, and voting-history caveats."),
        new("baseball-history://salary/guide", "Salary Guide", "Review salary tool limits, year coverage, and how salary history rows are shaped.")
    ];

    public async Task<ServerInfoDocument> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        var supportedYearSpan = await GetSupportedYearSpanAsync(cancellationToken);

        return new ServerInfoDocument(
            buildMetadata.Name,
            buildMetadata.Title,
            buildMetadata.Version,
            buildMetadata.Description,
            Transport: TransportName,
            ReadOnly: true,
            ConnectionStringKey: "ConnectionStrings:Lahman",
            Limits: CreateLimitSnapshot(),
            ToolNames,
            ResourceLinks,
            StartupRequirements:
            [
                "ConnectionStrings:Lahman must be configured before the server starts.",
                "Placeholder connection strings containing '<' are rejected at startup.",
                $"Player search page size is capped at {options.Value.Limits.PlayerSearchPageSizeMax} rows, franchise listing page size is capped at {options.Value.Limits.FranchiseListPageSizeMax} rows, Hall of Fame page size is capped at {options.Value.Limits.HallOfFamePageSizeMax} rows, batting and pitching leaderboard page size is capped at {options.Value.Limits.LeaderboardPageSizeMax} rows, salary history is capped at {options.Value.Limits.SalaryHistorySeasonsMax} seasons, and salary leaderboard page size is capped at {options.Value.Limits.SalaryLeaderboardPageSizeMax} rows.",
                $"Database commands use a configured timeout of {options.Value.QueryTimeoutSeconds} seconds.",
                "Client workflows should start with baseball-history://server/info and baseball-history://server/workflow-guide before calling domain tools."
            ],
            supportedYearSpan);
    }

    public async Task<StatsCatalogDocument> GetStatsCatalogAsync(CancellationToken cancellationToken = default) =>
        new(await GetSupportedYearSpanAsync(cancellationToken), SupportedCategories);

    public Task<WorkflowGuideDocument> GetWorkflowGuideAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(
            new WorkflowGuideDocument(
                Summary: "Use this guide to route common baseball questions to the shipped read-only MCP tools and JSON resources without assuming unsupported search or analytics.",
                Resources:
                [
                    new(
                        "baseball-history://server/info",
                        "Server identity, startup requirements, and global limits.",
                        ["Verify the server surface before choosing tools.", "Check page-size and timeout caps."]),
                    new(
                        "baseball-history://server/workflow-guide",
                        "Workflow routing for common baseball question types.",
                        ["Choose between player, franchise, team-season, leaderboard, Hall of Fame, salary, and diagnostics workflows."]),
                    new(
                        "baseball-history://server/stats-catalog",
                        "Supported batting and pitching leaderboard stats plus year-span metadata.",
                        ["Pick a valid stat key before calling get_batting_leaders or get_pitching_leaders.", "Confirm whether batting vs pitching stats are supported."]),
                    new(
                        "baseball-history://hall-of-fame/guide",
                        "Hall of Fame coverage, limits, and voting-history caveats.",
                        ["Check Hall of Fame year coverage.", "Confirm voting-history limits before requesting one player."]),
                    new(
                        "baseball-history://salary/guide",
                        "Salary coverage, limits, and row-shape caveats.",
                        ["Check salary history season caps.", "Confirm salary leaderboard limits and shape."]),
                    new(
                        "baseball-history://server/diagnostics",
                        "Safe runtime posture and connectivity snapshot.",
                        ["Inspect configured limits and reachability without exposing secrets."])
                ],
                Workflows:
                [
                    new(
                        Name: "player-search-and-detail",
                        CommonQuestions:
                        [
                            "Which player matches this name or partial name?",
                            "Show me one player's structured career batting, pitching, and team-tenure detail."
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://server/info"
                        ],
                        RecommendedTools:
                        [
                            "search_players",
                            "get_player"
                        ],
                        SupportedQueryShapes:
                        [
                            $"Free-text player lookup with search_players(query, page, pageSize) up to {options.Value.Limits.PlayerSearchPageSizeMax} rows per page.",
                            "Alphabetical last-name prefix lookup with search_players(lastNameStartsWith, page, pageSize).",
                            "Exact player detail by Lahman player id with get_player(playerId)."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Direct natural-language player detail without first resolving a Lahman player id.",
                            "Narrative biographies, scouting reports, or non-Lahman player facts outside the returned structured payload."
                        ]),
                    new(
                        Name: "franchise-lookup",
                        CommonQuestions:
                        [
                            "Which franchises are active or in a specific league?",
                            "Show me one franchise and its season-by-season history."
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://server/info"
                        ],
                        RecommendedTools:
                        [
                            "list_franchises",
                            "get_franchise"
                        ],
                        SupportedQueryShapes:
                        [
                            $"Paged franchise summaries with list_franchises(league?, activeOnly, page, pageSize) up to {options.Value.Limits.FranchiseListPageSizeMax} rows per page.",
                            "Exact franchise detail by franchiseId with get_franchise(franchiseId)."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Fuzzy franchise search by nickname or city substring.",
                            "A deterministic single team-season row without an explicit team id, league, and year."
                        ]),
                    new(
                        Name: "deterministic-team-season",
                        CommonQuestions:
                        [
                            "Show me the exact Yankees AL 1998 team-season row.",
                            "After I know the exact identifiers, fetch one precise team-season."
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://server/info"
                        ],
                        RecommendedTools:
                        [
                            "get_team_season",
                            "get_franchise"
                        ],
                        SupportedQueryShapes:
                        [
                            "Exact team-season lookup with get_team_season(teamId, league, year).",
                            "Use get_franchise(franchiseId) first when you need franchise history to discover a valid team-season before the deterministic lookup."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Approximate team-season lookup by nickname alone.",
                            "Bulk multi-season team retrieval from get_team_season."
                        ]),
                    new(
                        Name: "curated-leaderboards",
                        CommonQuestions:
                        [
                            "Who leads batting or pitching in a supported stat?",
                            "Give me career or single-season leaders inside a year range or league."
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://server/stats-catalog"
                        ],
                        RecommendedTools:
                        [
                            "get_batting_leaders",
                            "get_pitching_leaders"
                        ],
                        SupportedQueryShapes:
                        [
                            $"Curated batting leaderboards with get_batting_leaders(stat, fromYear?, toYear?, league?, minAtBats, singleSeason, page, pageSize) up to {options.Value.Limits.LeaderboardPageSizeMax} rows per page.",
                            $"Curated pitching leaderboards with get_pitching_leaders(stat, fromYear?, toYear?, league?, minInningsPitched, singleSeason, page, pageSize) up to {options.Value.Limits.LeaderboardPageSizeMax} rows per page.",
                            "Stat keys must come from baseball-history://server/stats-catalog."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Fielding, postseason, or arbitrary custom-stat leaderboards.",
                            "Free-form sorting over unsupported columns or formulas."
                        ]),
                    new(
                        Name: "hall-of-fame-history",
                        CommonQuestions:
                        [
                            "Which players were inducted in a given year or category?",
                            "What Hall of Fame ballot history exists for one player?"
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://hall-of-fame/guide"
                        ],
                        RecommendedTools:
                        [
                            "list_hall_of_fame_inductees",
                            "get_hall_of_fame_voting_history"
                        ],
                        SupportedQueryShapes:
                        [
                            $"Paged inductee lookup with list_hall_of_fame_inductees(year?, category?, page, pageSize) up to {options.Value.Limits.HallOfFamePageSizeMax} rows per page.",
                            $"One-player ballot history with get_hall_of_fame_voting_history(playerId), capped at {options.Value.Limits.HallOfFameVotingHistoryYearsMax} rows."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Hall of Fame biographies or prose explanations beyond Lahman ballot rows.",
                            "Cross-player Hall of Fame comparisons in a single tool call."
                        ]),
                    new(
                        Name: "salary-history-and-leaders",
                        CommonQuestions:
                        [
                            "What salary history does this player have?",
                            "Who had the highest recorded salaries overall or in one year?"
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://salary/guide"
                        ],
                        RecommendedTools:
                        [
                            "get_player_salary_history",
                            "get_salary_leaders"
                        ],
                        SupportedQueryShapes:
                        [
                            $"One-player salary history with get_player_salary_history(playerId), capped at {options.Value.Limits.SalaryHistorySeasonsMax} seasons.",
                            $"Paged salary leader lookup with get_salary_leaders(year?, page, pageSize) up to {options.Value.Limits.SalaryLeaderboardPageSizeMax} rows per page."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Inflation-adjusted salary analysis, contract terms, or salary projections.",
                            "Salary search by team without using the returned player-team-season rows."
                        ]),
                    new(
                        Name: "diagnostics-and-server-capabilities",
                        CommonQuestions:
                        [
                            "What can this server do and what limits are configured?",
                            "Is the configured database reachable without exposing secrets?"
                        ],
                        RecommendedResources:
                        [
                            "baseball-history://server/info",
                            "baseball-history://server/diagnostics",
                            "baseball-history://server/stats-catalog"
                        ],
                        RecommendedTools:
                        [
                            "get_server_diagnostics"
                        ],
                        SupportedQueryShapes:
                        [
                            "Read-only metadata and capability discovery through the server/info, server/workflow-guide, server/stats-catalog, hall-of-fame/guide, and salary/guide resources.",
                            "Safe runtime posture through baseball-history://server/diagnostics or get_server_diagnostics."
                        ],
                        UnsupportedQueryShapes:
                        [
                            "Database writes or runtime configuration changes.",
                            "Secret retrieval or raw connection-string inspection."
                        ])
                ],
                UnsupportedCapabilities:
                [
                    "The server does not mutate baseball data or runtime configuration.",
                    "The server does not expose arbitrary SQL, custom joins, or raw table browsing tools.",
                    "The server does not provide unsupported stat families beyond the shipped player, franchise, team-season, leaderboard, Hall of Fame, salary, and diagnostics surfaces."
                ]));

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
            Transport: TransportName,
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

    public async Task<HallOfFameGuideDocument> GetHallOfFameGuideAsync(CancellationToken cancellationToken = default) =>
        new(
            await GetHallOfFameYearSpanAsync(cancellationToken),
            options.Value.Limits.HallOfFamePageSizeMax,
            options.Value.Limits.HallOfFameVotingHistoryYearsMax,
            ["list_hall_of_fame_inductees", "get_hall_of_fame_voting_history"],
            [
                "Inductee results include only HallOfFame rows where inducted == 'Y'.",
                "Voting history returns the most recent ballot rows first internally, then presents them in chronological order.",
                $"Voting history is capped at {options.Value.Limits.HallOfFameVotingHistoryYearsMax} rows per player."
            ]);

    public async Task<SalaryGuideDocument> GetSalaryGuideAsync(CancellationToken cancellationToken = default) =>
        new(
            await GetSalaryYearSpanAsync(cancellationToken),
            options.Value.Limits.SalaryHistorySeasonsMax,
            options.Value.Limits.SalaryLeaderboardPageSizeMax,
            ["get_player_salary_history", "get_salary_leaders"],
            [
                "Salary history returns player-team-season rows from the Lahman Salaries table.",
                $"Salary history is capped at {options.Value.Limits.SalaryHistorySeasonsMax} seasons per player, ordered from most recent to oldest.",
                $"Salary leader pages are capped at {options.Value.Limits.SalaryLeaderboardPageSizeMax} rows."
            ]);

    public static string Serialize<T>(T document) => JsonSerializer.Serialize(document, JsonOptions.Value);

    private McpLimitSnapshot CreateLimitSnapshot() =>
        new(
            options.Value.QueryTimeoutSeconds,
            options.Value.Limits.PlayerSearchPageSizeMax,
            options.Value.Limits.FranchiseListPageSizeMax,
            options.Value.Limits.HallOfFamePageSizeMax,
            options.Value.Limits.LeaderboardPageSizeMax,
            options.Value.Limits.SalaryHistorySeasonCountMax,
            options.Value.Limits.TeamPayrollPlayerCountMax,
            options.Value.Limits.HallOfFameVotingHistoryYearsMax,
            options.Value.Limits.SalaryHistorySeasonsMax,
            options.Value.Limits.SalaryLeaderboardPageSizeMax);

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

    private async Task<SupportedYearSpan?> GetHallOfFameYearSpanAsync(CancellationToken cancellationToken) =>
        await GetCachedYearSpanAsync(
            HallOfFameYearSpanCacheKey,
            "HallOfFame.yearid",
            context => context.HallOfFame.Select(row => (int)row.Yearid),
            cancellationToken);

    private async Task<SupportedYearSpan?> GetSalaryYearSpanAsync(CancellationToken cancellationToken) =>
        await GetCachedYearSpanAsync(
            SalaryYearSpanCacheKey,
            "Salaries.yearID",
            context => context.Salaries.Select(row => (int)row.YearId),
            cancellationToken);

    private async Task<SupportedYearSpan?> GetCachedYearSpanAsync(
        string cacheKey,
        string source,
        Func<BaseballDbContext, IQueryable<int>> sourceFactory,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(cacheKey, out SupportedYearSpan? cached))
        {
            return cached;
        }

        try
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var span = await sourceFactory(context)
                .GroupBy(_ => 1)
                .Select(group => new SupportedYearSpan(group.Min(), group.Max(), source))
                .SingleOrDefaultAsync(cancellationToken);

            cache.Set(cacheKey, span, TimeSpan.FromHours(12));
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
