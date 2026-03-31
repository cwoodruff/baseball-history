namespace baseball_history_web.Api.Dtos;

public sealed record FranchiseListItem(
    string FranchiseId, string FranchiseName, bool IsActive,
    short? FirstYear, short? LastYear, int TotalSeasons,
    int TotalWins, int TotalLosses, double WinPct,
    int WorldSeriesWins, int PennantWins,
    string? CurrentTeamId, string? CurrentLeague, string? CurrentDivision);

public sealed record FranchiseDetail(
    string FranchiseId, string FranchiseName, bool IsActive,
    short? FirstYear, short? LastYear,
    int TotalSeasons, int TotalWins, int TotalLosses, double WinPct,
    int WorldSeriesWins, int PennantWins,
    IReadOnlyList<FranchiseSeasonItem> Seasons);

public sealed record FranchiseSeasonItem(
    short Year, string TeamId, string? TeamName, string LgId, string? DivId,
    short Wins, short Losses, double WinPct, byte? Rank,
    bool WonDivision, bool WonPennant, bool WonWorldSeries);

public sealed record TeamSeasonDetail(
    string TeamId, string? TeamName, short Year, string LgId, string? DivId,
    string? FranchiseId, string? FranchiseName,
    short Wins, short Losses, double WinPct, byte? Rank,
    bool WonDivision, bool WonWildCard, bool WonPennant, bool WonWorldSeries,
    string? ParkName, int? Attendance,
    ApiTeamBattingDto? Batting, ApiTeamPitchingDto? Pitching,
    IReadOnlyList<RosterBatterDto> Batters,
    IReadOnlyList<RosterPitcherDto> Pitchers,
    IReadOnlyList<ApiManagerDto> Managers);

public sealed record ApiTeamBattingDto(
    int Runs, int AtBats, int Hits, int Doubles, int Triples,
    int HomeRuns, int Walks, int Strikeouts, int StolenBases, double BattingAverage);

public sealed record ApiTeamPitchingDto(
    int RunsAllowed, int EarnedRuns, double Era, int CompleteGames,
    int Shutouts, int Saves, int HitsAllowed, int HomeRunsAllowed,
    int WalksAllowed, int StrikeoutsThrown);

public sealed record RosterBatterDto(
    string PlayerId, string FullName, int Games,
    int AtBats, int Hits, int HomeRuns, int Rbi,
    double BattingAverage, bool IsHallOfFamer);

public sealed record RosterPitcherDto(
    string PlayerId, string FullName, int Games,
    int Wins, int Losses, int Saves, int Strikeouts,
    double Era, bool IsHallOfFamer);

public sealed record ApiManagerDto(
    string PlayerId, string FullName, int Games,
    int Wins, int Losses, int Order, bool IsHallOfFamer);
