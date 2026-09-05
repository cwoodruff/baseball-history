namespace baseball_history_web.Api.Dtos;

public sealed record PlayerListItem(
    string PlayerId,
    string? FirstName,
    string? LastName,
    int? DebutYear,
    int? FinalYear,
    bool IsHallOfFamer,
    int? TotalGames,
    int? TotalHits,
    int? TotalHomeRuns,
    string? LastTeamId,
    bool IsPartialRecord = false);

public sealed record PlayerDetail(
    string PlayerId,
    string? FirstName,
    string? LastName,
    string? GivenName,
    string? Height,
    string? Weight,
    string? Bats,
    string? Throws,
    string? BirthDate,
    string? BirthCity,
    string? BirthState,
    string? BirthCountry,
    string? DeathDate,
    int? DebutYear,
    int? FinalYear,
    bool IsHallOfFamer,
    int? HofInductionYear,
    CareerBattingDto? CareerBatting,
    CareerPitchingDto? CareerPitching,
    IReadOnlyList<PlayerTeamDto> Teams,
    bool IsPartialRecord = false);

public sealed record CareerBattingDto(
    int Games, int AtBats, int Runs, int Hits, int Doubles, int Triples,
    int HomeRuns, int Rbi, int StolenBases, int Walks, int Strikeouts,
    double BattingAverage, double Obp, double Slg, double Ops);

public sealed record CareerPitchingDto(
    int Games, int GamesStarted, int Wins, int Losses, int Saves,
    int CompleteGames, int Shutouts, double InningsPitched,
    int Hits, int EarnedRuns, int HomeRuns, int Walks, int Strikeouts,
    double Era, double Whip);

public sealed record SeasonBattingDto(
    short Year, string TeamId, string? TeamName, string LgId,
    int Games, int AtBats, int Runs, int Hits, int Doubles, int Triples,
    int HomeRuns, int Rbi, int StolenBases, int Walks, int Strikeouts,
    double BattingAverage);

public sealed record SeasonPitchingDto(
    short Year, string TeamId, string? TeamName, string LgId,
    int Games, int GamesStarted, int Wins, int Losses, int Saves,
    double InningsPitched, int Hits, int EarnedRuns, int Strikeouts,
    int Walks, double Era);

public sealed record SeasonFieldingDto(
    short Year, string TeamId, string LgId, string Position,
    int Games, int PutOuts, int Assists, int Errors, int DoublePlays);

public sealed record PlayerAwardDto(int Year, string AwardId, string LgId, string? Notes);

public sealed record PlayerTeamDto(string TeamId, string? TeamName, short FirstYear, short LastYear, int Seasons);

public sealed record PostseasonBattingDto(
    short Year, string Round, string TeamId, string LgId,
    int Games, int AtBats, int Hits, int HomeRuns, int Rbi, double BattingAverage);

public sealed record PostseasonPitchingDto(
    short Year, string Round, string? TeamId, string? LgId,
    int Games, int Wins, int Losses, int Saves, double InningsPitched,
    int Strikeouts, double Era);
