namespace baseball_history_mcp.Querying;

public sealed record SalaryLeaderQuery(
    int? Year = null,
    int Page = 1,
    int PageSize = 50);

public sealed record SalaryEntryReadModel(
    short Year,
    string TeamId,
    string LgId,
    string PlayerId,
    string FullName,
    long? Salary);

public sealed record SalarySeasonReadModel(
    short Year,
    string TeamId,
    string LgId,
    long? Salary);

public sealed record PlayerSalaryHistoryReadModel(
    string PlayerId,
    string FullName,
    IReadOnlyList<SalarySeasonReadModel> Seasons,
    long CareerTotal)
{
    public int RequestedItemCount { get; init; } = Seasons.Count;

    public int MaxItemCount { get; init; } = Seasons.Count;

    public bool WasItemCountClamped { get; init; }
}

public sealed record TeamPayrollReadModel(
    short Year,
    string TeamId,
    long TotalPayroll,
    int PlayerCount,
    IReadOnlyList<SalaryEntryReadModel> Players)
{
    public int RequestedItemCount { get; init; } = Players.Count;

    public int MaxItemCount { get; init; } = Players.Count;

    public bool WasItemCountClamped { get; init; }
}

public interface ISalaryReadService
{
    Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(string playerId, int? itemCount = null, CancellationToken cancellationToken = default);
    Task<TeamPayrollReadModel?> GetTeamPayrollAsync(string teamId, int year, int? itemCount = null, CancellationToken cancellationToken = default);
    Task<PagedReadResult<SalaryEntryReadModel>> GetSalaryLeadersAsync(SalaryLeaderQuery query, CancellationToken cancellationToken = default);
}
