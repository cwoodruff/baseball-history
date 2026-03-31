namespace baseball_history_web.Api.Dtos;

public sealed record SalaryDto(
    short Year, string TeamId, string LgId,
    string PlayerId, string? PlayerName, long? Salary);

public sealed record PlayerSalaryHistoryDto(
    string PlayerId, string? FullName,
    IReadOnlyList<SalarySeasonDto> Seasons,
    long CareerTotal);

public sealed record SalarySeasonDto(short Year, string TeamId, string LgId, long? Salary);

public sealed record TeamSalaryDto(
    short Year, string TeamId,
    long TotalPayroll, int PlayerCount,
    IReadOnlyList<SalaryDto> Players);
