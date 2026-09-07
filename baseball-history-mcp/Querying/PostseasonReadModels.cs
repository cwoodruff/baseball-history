namespace baseball_history_mcp.Querying;

public sealed record PostseasonBattingRowReadModel(
    short Year,
    string Round,
    string TeamId,
    string? TeamName,
    string LeagueId,
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
    int Strikeouts,
    double BattingAverage);

public sealed record PostseasonPitchingRowReadModel(
    short Year,
    string Round,
    string TeamId,
    string? TeamName,
    string? LeagueId,
    int Wins,
    int Losses,
    int Games,
    int GamesStarted,
    int CompleteGames,
    int Shutouts,
    int Saves,
    double InningsPitched,
    int Hits,
    int EarnedRuns,
    int HomeRuns,
    int Walks,
    int Strikeouts,
    double? Era);

public sealed record PlayerPostseasonReadModel(
    string PlayerId,
    string FullName,
    int TotalBattingRowCount,
    int TotalPitchingRowCount,
    int MaxRowsPerCategory,
    bool WasBattingCapped,
    bool WasPitchingCapped,
    IReadOnlyList<PostseasonBattingRowReadModel> Batting,
    IReadOnlyList<PostseasonPitchingRowReadModel> Pitching);

public interface IPostseasonReadService
{
    Task<PlayerPostseasonReadModel?> GetPlayerPostseasonAsync(string playerId, CancellationToken cancellationToken = default);
}
