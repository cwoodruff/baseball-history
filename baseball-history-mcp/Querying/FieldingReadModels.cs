namespace baseball_history_mcp.Querying;

public sealed record FieldingSeasonRowReadModel(
    short Year,
    byte Stint,
    string TeamId,
    string? TeamName,
    string LeagueId,
    string Position,
    int Games,
    int GamesStarted,
    double? Innings,
    int Putouts,
    int Assists,
    int Errors,
    int DoublePlays,
    double? FieldingPercentage);

public sealed record FieldingCareerPositionReadModel(
    string Position,
    int Games,
    int Putouts,
    int Assists,
    int Errors,
    int DoublePlays,
    double? FieldingPercentage);

public sealed record PlayerFieldingReadModel(
    string PlayerId,
    string FullName,
    int TotalSeasonRowCount,
    int MaxSeasonRows,
    bool WasSeasonListCapped,
    IReadOnlyList<FieldingCareerPositionReadModel> CareerByPosition,
    IReadOnlyList<FieldingSeasonRowReadModel> Seasons);

public interface IFieldingReadService
{
    Task<PlayerFieldingReadModel?> GetPlayerFieldingAsync(string playerId, CancellationToken cancellationToken = default);
}
