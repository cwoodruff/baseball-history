namespace baseball_history_mcp.Querying;

public sealed record PlayerLookupRequest(
    string? Query = null,
    string? LastNameStartsWith = null,
    int Page = 1,
    int PageSize = 25,
    string? Position = null,
    int? Era = null,
    string? Sort = null);

public sealed record PlayerLookupItem(
    string PlayerId,
    string FullName,
    int? DebutYear,
    int? FinalYear,
    bool IsHallOfFamer,
    int? TotalGames,
    int? TotalHits,
    int? TotalHomeRuns,
    string? LastTeamId);

public sealed record PlayerTeamTenure(
    string TeamId,
    string? TeamName,
    short FirstYear,
    short LastYear,
    int Seasons);

public sealed record PlayerCareerBattingSummary(
    int Games,
    int AtBats,
    int Runs,
    int Hits,
    int Doubles,
    int Triples,
    int HomeRuns,
    int Rbi,
    int StolenBases,
    int Walks,
    int Strikeouts,
    double BattingAverage,
    double Obp,
    double Slg,
    double Ops);

public sealed record PlayerCareerPitchingSummary(
    int Games,
    int GamesStarted,
    int Wins,
    int Losses,
    int Saves,
    int CompleteGames,
    int Shutouts,
    double InningsPitched,
    int Hits,
    int EarnedRuns,
    int HomeRuns,
    int Walks,
    int Strikeouts,
    double Era,
    double Whip);

public sealed record PlayerReadModel(
    string PlayerId,
    string FullName,
    string? GivenName,
    string? Height,
    string? Weight,
    string? Bats,
    string? Throws,
    int? DebutYear,
    int? FinalYear,
    bool IsHallOfFamer,
    int? HallOfFameInductionYear,
    PlayerCareerBattingSummary? CareerBatting,
    PlayerCareerPitchingSummary? CareerPitching,
    IReadOnlyList<PlayerTeamTenure> Teams,
    int? CareerOpsIndex = null,
    bool? CareerQualified = null,
    int AllStarSelectionCount = 0,
    IReadOnlyList<short>? AllStarYears = null,
    bool HasManagerialCareer = false);

/// <summary>
/// League-indexed season batting rates (stints combined). OpsIndex is OPS
/// relative to league, 100 = average, not park-adjusted; BbPct and KPct are
/// percentages. Null means the inputs weren't recorded that season.
/// </summary>
public sealed record PlayerAdvancedBattingSeason(
    short Year,
    string? LeagueId,
    long PlateAppearances,
    double? Iso,
    double? Babip,
    double? BbPct,
    double? KPct,
    double? OpsIndex,
    double? HrPer162,
    bool Qualified);

/// <summary>Season pitching rates (stints combined); null when innings weren't recorded</summary>
public sealed record PlayerAdvancedPitchingSeason(
    short Year,
    string? LeagueId,
    int Games,
    int GamesStarted,
    double? InningsPitched,
    double? Era,
    double? Whip,
    double? K9,
    double? Bb9,
    bool Qualified);

public sealed record PlayerAdvancedStatsReadModel(
    string PlayerId,
    string FullName,
    int? CareerOpsIndex,
    bool? CareerQualified,
    IReadOnlyList<PlayerAdvancedBattingSeason> BattingSeasons,
    IReadOnlyList<PlayerAdvancedPitchingSeason> PitchingSeasons);

public interface IPlayerReadService
{
    Task<PagedReadResult<PlayerLookupItem>> SearchPlayersAsync(PlayerLookupRequest request, CancellationToken cancellationToken = default);
    Task<PlayerReadModel?> GetPlayerAsync(string playerId, CancellationToken cancellationToken = default);
    Task<PlayerAdvancedStatsReadModel?> GetPlayerAdvancedStatsAsync(string playerId, CancellationToken cancellationToken = default);
}
