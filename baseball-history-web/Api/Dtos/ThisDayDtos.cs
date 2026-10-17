namespace baseball_history_web.Api.Dtos;

public sealed record ThisDayDto(
    int Month, int Day, string Label,
    IReadOnlyList<ThisDayEntryDto> Birthdays,
    IReadOnlyList<ThisDayEntryDto> Debuts,
    IReadOnlyList<ThisDayEntryDto> FinalGames);

/// <summary>Year is the birth year for birthdays and the game year for debuts and final games</summary>
public sealed record ThisDayEntryDto(string PlayerId, string FullName, int? Year, bool IsHallOfFamer);
