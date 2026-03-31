namespace baseball_history_web.Api.Dtos;

public sealed record PostseasonSeriesDto(
    short Year, string Round,
    string WinnerTeamId, string WinnerLgId,
    string? LoserTeamId, string? LoserLgId,
    short? Wins, short? Losses, short? Ties);
