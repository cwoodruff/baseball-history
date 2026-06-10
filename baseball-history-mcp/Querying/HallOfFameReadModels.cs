namespace baseball_history_mcp.Querying;

public sealed record HallOfFameLookupRequest(
    int? Year = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 50);

public sealed record HallOfFameInducteeReadModel(
    string PlayerId,
    string FullName,
    int InductionYear,
    string Category,
    string? VotedBy,
    double? VotePercentage,
    int? DebutYear,
    int? FinalYear);

public sealed record HallOfFameVotingYearReadModel(
    int Year,
    string? Category,
    string? VotedBy,
    string? Votes,
    string? Ballots,
    double? VotePercentage,
    bool Inducted);

public sealed record HallOfFameVotingHistoryReadModel(
    string PlayerId,
    string FullName,
    int TotalYearCount,
    int ReturnedYearCount,
    int MaxYearCount,
    bool WasHistoryCapped,
    IReadOnlyList<HallOfFameVotingYearReadModel> VotingHistory);
