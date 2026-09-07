namespace baseball_history_mcp.Querying;

public sealed record ManagerSeasonReadModel(
    short Year,
    string TeamId,
    string? TeamName,
    string LeagueId,
    byte Inseason,
    int Games,
    int Wins,
    int Losses,
    byte? Rank,
    bool WasPlayerManager,
    bool TeamWonPennant,
    bool TeamWonWorldSeries);

public sealed record ManagerAwardReadModel(
    short Year,
    string AwardId,
    string LeagueId);

public sealed record ManagerReadModel(
    string PlayerId,
    string FullName,
    bool IsInHallOfFame,
    bool WasPlayer,
    bool WasPlayerManager,
    short? FirstYear,
    short? LastYear,
    int SeasonCount,
    int Games,
    int Wins,
    int Losses,
    double WinningPercentage,
    int Pennants,
    int WorldSeriesTitles,
    int TotalSeasonRowCount,
    int MaxSeasonRows,
    bool WasSeasonListCapped,
    IReadOnlyList<ManagerAwardReadModel> Awards,
    IReadOnlyList<ManagerSeasonReadModel> Seasons);

public interface IManagerReadService
{
    Task<ManagerReadModel?> GetManagerAsync(string playerId, CancellationToken cancellationToken = default);
}
