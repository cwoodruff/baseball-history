namespace baseball_history_web.Api.Dtos;

public sealed record AwardWinnerDto(
    string PlayerId, string? PlayerName,
    string AwardId, int Year, string LgId, string? Notes);

public sealed record AwardVotingDto(
    string AwardId, short Year, string LgId,
    IReadOnlyList<AwardVoteDto> Votes);

public sealed record AwardVoteDto(
    string PlayerId, string? PlayerName,
    short? PointsWon, short? PointsMax, string? VotesFirst,
    double VoteShare);
