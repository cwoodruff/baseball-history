namespace baseball_history_mcp.Querying;

public sealed record FranchiseLookupRequest(
    string? League = null,
    bool ActiveOnly = false,
    int Page = 1,
    int PageSize = 25);

public sealed record FranchiseLookupItem(
    string FranchiseId,
    string FranchiseName,
    bool IsActive,
    short? FirstYear,
    short? LastYear,
    int TotalSeasons,
    int TotalWins,
    int TotalLosses,
    double WinPct,
    int WorldSeriesWins,
    int PennantWins,
    string? CurrentTeamId,
    string? CurrentLeague,
    string? CurrentDivision);

public sealed record FranchiseSeasonReadModel(
    short Year,
    string TeamId,
    string? TeamName,
    string LgId,
    string? DivId,
    short Wins,
    short Losses,
    double WinPct,
    byte? Rank,
    bool WonDivision,
    bool WonPennant,
    bool WonWorldSeries);

public sealed record FranchiseReadModel(
    string FranchiseId,
    string FranchiseName,
    bool IsActive,
    short? FirstYear,
    short? LastYear,
    int TotalSeasons,
    int TotalWins,
    int TotalLosses,
    double WinPct,
    int WorldSeriesWins,
    int PennantWins,
    IReadOnlyList<FranchiseSeasonReadModel> Seasons);

public interface IFranchiseReadService
{
    Task<PagedReadResult<FranchiseLookupItem>> ListFranchisesAsync(FranchiseLookupRequest request, CancellationToken cancellationToken = default);
    Task<FranchiseReadModel?> GetFranchiseAsync(string franchiseId, CancellationToken cancellationToken = default);
}
