namespace baseball_history_mcp.Querying;

public sealed record PlayerLookupRequest(
    string? Query = null,
    string? LastNameStartsWith = null,
    int Page = 1,
    int PageSize = 25);

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
    IReadOnlyList<PlayerTeamTenure> Teams);

public interface IPlayerReadService
{
    Task<PagedReadResult<PlayerLookupItem>> SearchPlayersAsync(PlayerLookupRequest request, CancellationToken cancellationToken = default);
    Task<PlayerReadModel?> GetPlayerAsync(string playerId, CancellationToken cancellationToken = default);
}
