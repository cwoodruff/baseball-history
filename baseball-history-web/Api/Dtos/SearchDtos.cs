namespace baseball_history_web.Api.Dtos;

public sealed record SearchResponse(
    string Query,
    IReadOnlyList<PlayerSearchResult> Players,
    IReadOnlyList<FranchiseSearchResult> Franchises,
    int TotalPlayerCount,
    int TotalFranchiseCount);

public sealed record PlayerSearchResult(
    string PlayerId, string FullName, int? DebutYear, int? FinalYear, bool IsHallOfFamer);

public sealed record FranchiseSearchResult(
    string FranchiseId, string FranchiseName, bool IsActive, string? CurrentTeamId);
