namespace baseball_history_mcp.Querying;

public sealed record TeamSeasonReadModel(
    string TeamId,
    string? TeamName,
    short Year,
    string LeagueId,
    string? DivisionId,
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
    TeamBattingSummaryReadModel? Batting,
    TeamPitchingSummaryReadModel? Pitching,
    IReadOnlyList<TeamBatterReadModel> Batters,
    IReadOnlyList<TeamPitcherReadModel> Pitchers,
    IReadOnlyList<TeamManagerReadModel> Managers);

public sealed record TeamBattingSummaryReadModel(
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

public sealed record TeamPitchingSummaryReadModel(
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

public sealed record TeamBatterReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int AtBats,
    int Hits,
    int HomeRuns,
    int Rbi,
    double BattingAverage,
    bool IsHallOfFamer);

public sealed record TeamPitcherReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int Wins,
    int Losses,
    int Saves,
    int Strikeouts,
    double Era,
    bool IsHallOfFamer);

public sealed record TeamManagerReadModel(
    string PlayerId,
    string FullName,
    int Games,
    int Wins,
    int Losses,
    int Order,
    bool IsHallOfFamer);

public interface ITeamReadService
{
    Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        string teamId,
        string league,
        int year,
        CancellationToken cancellationToken = default);
}
