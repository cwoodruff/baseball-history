namespace baseball_history_mcp.Querying;

public sealed record AllStarYearSummaryReadModel(
    short Year,
    int GameCount,
    int Selections,
    bool HasEastWestGame);

/// <summary>TeamLeagueId is the club's season league, which differs from the squad code in Negro Leagues games</summary>
public sealed record AllStarRosterPlayerReadModel(
    string PlayerId,
    string FullName,
    string TeamId,
    string? TeamName,
    string? TeamLeagueId,
    bool IsStarter,
    int? StartingPos,
    bool Played,
    bool IsInHallOfFame);

public sealed record AllStarRosterReadModel(
    string LeagueId,
    string SquadName,
    IReadOnlyList<AllStarRosterPlayerReadModel> Players);

/// <summary>GameType is AllStar (AL vs. NL), EastWest, or NorthSouth (Negro Leagues showcases)</summary>
public sealed record AllStarGameReadModel(
    string Title,
    string GameType,
    int GameNum,
    DateOnly? GameDate,
    IReadOnlyList<AllStarRosterReadModel> Rosters);

public sealed record AllStarYearReadModel(
    short Year,
    short? PreviousYear,
    short? NextYear,
    IReadOnlyList<AllStarGameReadModel> Games);

public interface IAllStarReadService
{
    Task<IReadOnlyList<AllStarYearSummaryReadModel>> ListAllStarYearsAsync(CancellationToken cancellationToken = default);
    Task<AllStarYearReadModel?> GetAllStarYearAsync(int year, CancellationToken cancellationToken = default);
}
