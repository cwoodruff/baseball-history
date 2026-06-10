namespace baseball_history_mcp.Querying;

public sealed record TeamSeasonBattingReadModel(
    int Runs,
    int AtBats,
    int Hits,
    int Doubles,
    int Triples,
    int HomeRuns,
    int Walks,
    int Strikeouts,
    int StolenBases,
    double BattingAverage);

public sealed record TeamSeasonPitchingReadModel(
    int RunsAllowed,
    int EarnedRuns,
    double Era,
    int CompleteGames,
    int Shutouts,
    int Saves,
    int HitsAllowed,
    int HomeRunsAllowed,
    int WalksAllowed,
    int StrikeoutsThrown);

public sealed record TeamSeasonBatterReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int AtBats,
    int Hits,
    int HomeRuns,
    int Rbi,
    double BattingAverage,
    bool IsHallOfFamer);

public sealed record TeamSeasonPitcherReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int Wins,
    int Losses,
    int Saves,
    int Strikeouts,
    double Era,
    bool IsHallOfFamer);

public sealed record TeamSeasonManagerReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int Wins,
    int Losses,
    int Order,
    bool IsHallOfFamer);

public sealed record TeamSeasonReadModel(
    string TeamId,
    string TeamName,
    short Year,
    string League,
    string? Division,
    string? FranchiseId,
    string? FranchiseName,
    short Wins,
    short Losses,
    double WinPct,
    byte? Rank,
    bool WonDivision,
    bool WonWildCard,
    bool WonPennant,
    bool WonWorldSeries,
    string? ParkName,
    int? Attendance,
    TeamSeasonBattingReadModel? Batting,
    TeamSeasonPitchingReadModel? Pitching,
    IReadOnlyList<TeamSeasonBatterReadModel> Batters,
    IReadOnlyList<TeamSeasonPitcherReadModel> Pitchers,
    IReadOnlyList<TeamSeasonManagerReadModel> Managers);

public interface ITeamSeasonReadService
{
    Task<TeamSeasonReadModel?> GetTeamSeasonAsync(string teamId, string league, int year, CancellationToken cancellationToken = default);
}
