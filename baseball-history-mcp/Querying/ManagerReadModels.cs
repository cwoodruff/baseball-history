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

public sealed record ManagerLookupRequest(
    string? Query = null,
    string? Sort = null,
    int Page = 1,
    int PageSize = 25);

public sealed record ManagerLookupItem(
    string PlayerId,
    string FullName,
    short FirstYear,
    short LastYear,
    int Seasons,
    int Games,
    int Wins,
    int Losses,
    double WinningPercentage,
    bool IsInHallOfFame);

public interface IManagerReadService
{
    Task<PagedReadResult<ManagerLookupItem>> ListManagersAsync(ManagerLookupRequest request, CancellationToken cancellationToken = default);
    Task<ManagerReadModel?> GetManagerAsync(string playerId, CancellationToken cancellationToken = default);
}
