namespace baseball_history_mcp.Querying;

public sealed record SalaryLeaderRequest(
    int? Year = null,
    int Page = 1,
    int PageSize = 25);

public sealed record SalarySeasonReadModel(
    short Year,
    string TeamId,
    string LeagueId,
    long? Salary);

public sealed record PlayerSalaryHistoryReadModel(
    string PlayerId,
    string FullName,
    long CareerTotal,
    int TotalSeasonCount,
    int ReturnedSeasonCount,
    int MaxSeasonCount,
    bool WasSeasonHistoryCapped,
    IReadOnlyList<SalarySeasonReadModel> Seasons);

public sealed record SalaryLeaderEntry(
    short Year,
    string TeamId,
    string LeagueId,
    string PlayerId,
    string PlayerName,
    long? Salary);

public interface ISalaryReadService
{
    Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(string playerId, CancellationToken cancellationToken = default);
    Task<PagedReadResult<SalaryLeaderEntry>> GetSalaryLeadersAsync(SalaryLeaderRequest request, CancellationToken cancellationToken = default);
}
