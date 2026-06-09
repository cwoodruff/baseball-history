namespace baseball_history_mcp.Metadata;

public sealed record ServerBuildMetadata(
    string Name,
    string Title,
    string Version,
    string Description);

public sealed record SupportedYearSpan(
    int StartYear,
    int EndYear,
    string Source);

public sealed record SupportedStatDefinition(
    string Key,
    string Label,
    IReadOnlyList<string> Aliases,
    string SortDirection,
    string Description);

public sealed record SupportedStatCategory(
    string Category,
    bool SupportsCareer,
    bool SupportsSingleSeason,
    IReadOnlyList<SupportedStatDefinition> Stats);

public sealed record McpLimitSnapshot(
    int QueryTimeoutSeconds,
    int PlayerSearchPageSizeMax,
    int FranchiseListPageSizeMax,
    int LeaderboardPageSizeMax,
    int HallOfFamePageSizeMax,
    int HallOfFameVotingHistoryYearsMax,
    int SalaryHistorySeasonsMax,
    int SalaryLeaderboardPageSizeMax);

public sealed record ServerResourceLink(
    string Uri,
    string Title,
    string Purpose);

public sealed record ServerInfoDocument(
    string Name,
    string Title,
    string Version,
    string Description,
    string Transport,
    bool ReadOnly,
    string ConnectionStringKey,
    McpLimitSnapshot Limits,
    IReadOnlyList<string> ToolNames,
    IReadOnlyList<ServerResourceLink> Resources,
    IReadOnlyList<string> StartupRequirements,
    SupportedYearSpan? SupportedYearSpan);

public sealed record StatsCatalogDocument(
    SupportedYearSpan? SupportedYearSpan,
    IReadOnlyList<SupportedStatCategory> Categories);

public sealed record ServerDiagnosticsDocument(
    string Name,
    string Version,
    string Transport,
    string DatabaseProvider,
    string QueryTrackingBehavior,
    bool UsesPooledDbContextFactory,
    bool RetriesTransientFailures,
    bool ConnectionStringConfigured,
    bool DatabaseReachable,
    string ConnectionStringKey,
    McpLimitSnapshot Limits,
    SupportedYearSpan? SupportedYearSpan,
    int ToolCount,
    int ResourceCount);

public sealed record HallOfFameGuideDocument(
    SupportedYearSpan? SupportedYearSpan,
    int InducteePageSizeMax,
    int VotingHistoryYearsMax,
    IReadOnlyList<string> ToolNames,
    IReadOnlyList<string> Caveats);

public sealed record SalaryGuideDocument(
    SupportedYearSpan? SupportedYearSpan,
    int SalaryHistorySeasonsMax,
    int SalaryLeaderboardPageSizeMax,
    IReadOnlyList<string> ToolNames,
    IReadOnlyList<string> Caveats);

public sealed record WorkflowResourceGuide(
    string Uri,
    string Purpose,
    IReadOnlyList<string> BestFor);

public sealed record WorkflowScenarioGuide(
    string Name,
    IReadOnlyList<string> CommonQuestions,
    IReadOnlyList<string> RecommendedResources,
    IReadOnlyList<string> RecommendedTools,
    IReadOnlyList<string> SupportedQueryShapes,
    IReadOnlyList<string> UnsupportedQueryShapes);

public sealed record WorkflowGuideDocument(
    string Summary,
    IReadOnlyList<WorkflowResourceGuide> Resources,
    IReadOnlyList<WorkflowScenarioGuide> Workflows,
    IReadOnlyList<string> UnsupportedCapabilities);
