namespace baseball_history_mcp.Querying;

public sealed record BattingLeaderboardQuery(
    string Stat = "hr",
    int? FromYear = null,
    int? ToYear = null,
    string? League = null,
    int MinAtBats = 0,
    bool SingleSeason = false,
    int Page = 1,
    int PageSize = 50);

public sealed record PitchingLeaderboardQuery(
    string Stat = "w",
    int? FromYear = null,
    int? ToYear = null,
    string? League = null,
    int MinInningsPitched = 0,
    bool SingleSeason = false,
    int Page = 1,
    int PageSize = 50);

public sealed record BattingLeaderboardEntry(
    int Rank,
    string PlayerId,
    string PlayerName,
    short? Year,
    string? TeamId,
    string? TeamName,
    bool IsHallOfFamer,
    int Games,
    int AtBats,
    int Runs,
    int Hits,
    int Doubles,
    int Triples,
    int HomeRuns,
    int Rbi,
    int StolenBases,
    int Walks,
    double BattingAverage,
    double Obp,
    double Slg,
    double Ops);

public sealed record PitchingLeaderboardEntry(
    int Rank,
    string PlayerId,
    string PlayerName,
    short? Year,
    string? TeamId,
    string? TeamName,
    bool IsHallOfFamer,
    int Games,
    int GamesStarted,
    int Wins,
    int Losses,
    int Saves,
    int CompleteGames,
    int Shutouts,
    double InningsPitched,
    int Hits,
    int EarnedRuns,
    int HomeRuns,
    int Walks,
    int Strikeouts,
    double Era,
    double Whip);

public interface ILeaderboardReadService
{
    Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(BattingLeaderboardQuery query, CancellationToken cancellationToken = default);
    Task<PagedReadResult<PitchingLeaderboardEntry>> GetPitchingLeadersAsync(PitchingLeaderboardQuery query, CancellationToken cancellationToken = default);
}
