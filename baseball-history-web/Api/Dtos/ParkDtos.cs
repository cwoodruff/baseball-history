namespace baseball_history_web.Api.Dtos;

public sealed record ParkDto(
    string? ParkKey, string? ParkName,
    string? City, string? State, string? Country);

public sealed record ParkDetailDto(
    string? ParkKey, string? ParkName,
    string? City, string? State, string? Country,
    IReadOnlyList<ParkSeasonDto> Seasons);

public sealed record ParkSeasonDto(
    short Year, string TeamId, string LgId, int? Games, int? Attendance);
