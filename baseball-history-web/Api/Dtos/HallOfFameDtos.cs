namespace baseball_history_web.Api.Dtos;

public sealed record HallOfFameInducteeDto(
    string PlayerId, string? FirstName, string? LastName,
    int InductionYear, string Category, string? VotedBy,
    double? VotePercentage,
    int? DebutYear, int? FinalYear);

public sealed record HallOfFameVotingHistoryDto(
    string PlayerId, string? FullName,
    IReadOnlyList<VotingYearDto> VotingHistory);

public sealed record VotingYearDto(
    int Year, string? VotedBy, string? Votes, string? Ballots,
    double? VotePercentage, bool Inducted);
