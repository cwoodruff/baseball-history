namespace baseball_history_mcp.Querying;

/// <summary>Year is the birth year for birthdays and the game year for debuts and final games</summary>
public sealed record ThisDayEntryReadModel(
    string PlayerId,
    string FullName,
    int? Year,
    bool IsInHallOfFame);

public sealed record ThisDayReadModel(
    int Month,
    int Day,
    string Label,
    int MaxEntriesPerCategory,
    IReadOnlyList<ThisDayEntryReadModel> Birthdays,
    IReadOnlyList<ThisDayEntryReadModel> Debuts,
    IReadOnlyList<ThisDayEntryReadModel> FinalGames);

public interface IThisDayReadService
{
    Task<ThisDayReadModel> GetThisDayAsync(string? date, int limit, CancellationToken cancellationToken = default);
}
