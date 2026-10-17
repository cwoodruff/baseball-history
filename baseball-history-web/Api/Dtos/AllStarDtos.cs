namespace baseball_history_web.Api.Dtos;

public sealed record AllStarYearItem(short Year, int GameCount, int Selections, bool HasEastWestGame);

public sealed record AllStarYearDetail(
    short Year, short? PreviousYear, short? NextYear, IReadOnlyList<AllStarGameDto> Games);

/// <summary>GameType is "AllStar" (AL vs. NL), "EastWest" or "NorthSouth" (Negro Leagues showcases)</summary>
public sealed record AllStarGameDto(
    string Title, string GameType, int GameNum, DateOnly? GameDate,
    IReadOnlyList<AllStarRosterDto> Rosters);

public sealed record AllStarRosterDto(string LgId, string SquadName, IReadOnlyList<AllStarPlayerDto> Players);

/// <summary>TeamLgId is the club's season league, which differs from the squad code for Negro Leagues games</summary>
public sealed record AllStarPlayerDto(
    string PlayerId, string FullName, string TeamId, string? TeamName, string? TeamLgId,
    bool IsStarter, int? StartingPos, string? StartingPosition, bool Played, bool IsHallOfFamer);

public sealed record PlayerAllStarDto(
    short Year, string LgId, string TeamId, int GameNum, bool IsStarter, int? StartingPos, bool Played);
