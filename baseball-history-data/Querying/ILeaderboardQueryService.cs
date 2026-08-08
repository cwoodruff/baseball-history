namespace BaseballHistory.Data.Querying;

public interface ILeaderboardQueryService
{
    Task<PagedResult<BattingLeaderRow>> GetBattingLeadersAsync(LeaderboardRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PitchingLeaderRow>> GetPitchingLeadersAsync(LeaderboardRequest request, CancellationToken cancellationToken = default);
}
