namespace baseball_history_mcp.Querying;

public sealed record ParkSearchRequest(
    string? Query = null,
    string? State = null,
    int Page = 1,
    int PageSize = 25);

public sealed record ParkSummaryReadModel(
    string ParkKey,
    string? Name,
    string? Aliases,
    string? City,
    string? State,
    string? Country,
    short? FirstYear,
    short? LastYear);

public sealed record ParkTenantReadModel(
    string TeamId,
    string LeagueId,
    string? TeamName,
    short FirstYear,
    short LastYear,
    int Seasons,
    int Games);

public sealed record ParkSeasonReadModel(
    short Year,
    string TeamId,
    string LeagueId,
    string? TeamName,
    short? Games,
    short? Openings,
    int? Attendance);

public sealed record ParkReadModel(
    string ParkKey,
    string? Name,
    IReadOnlyList<string> Aliases,
    string? City,
    string? State,
    string? Country,
    short? FirstYear,
    short? LastYear,
    int TotalGames,
    long TotalAttendance,
    int TotalSeasonRowCount,
    int MaxSeasonRows,
    bool WasSeasonListCapped,
    IReadOnlyList<ParkTenantReadModel> Tenants,
    IReadOnlyList<ParkSeasonReadModel> Seasons);

public interface IParkReadService
{
    Task<PagedReadResult<ParkSummaryReadModel>> SearchParksAsync(ParkSearchRequest request, CancellationToken cancellationToken = default);
    Task<ParkReadModel?> GetParkAsync(string parkKey, CancellationToken cancellationToken = default);
}
