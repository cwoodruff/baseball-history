namespace baseball_history_web.Api.Dtos;

public sealed record ManagerListItem(
    string PlayerId, string FullName, short FirstYear, short LastYear, int Seasons,
    int Games, int Wins, int Losses, double WinPct, bool IsHallOfFamer);

public sealed record ManagerDetailDto(
    string PlayerId, string FullName, bool IsHallOfFamer, bool WasPlayer, bool WasPlayerManager,
    short? FirstYear, short? LastYear, int Seasons, int Teams,
    int Games, int Wins, int Losses, double WinPct, int Pennants, int WorldSeriesTitles,
    IReadOnlyList<ManagerSeasonDto> SeasonRecords,
    IReadOnlyList<ManagerHalfDto> SplitSeasonHalves,
    IReadOnlyList<ManagerAwardDto> Awards);

/// <summary>One managerial stint; Order is the in-season sequence (1 = opened the season)</summary>
public sealed record ManagerSeasonDto(
    short Year, string TeamId, string LgId, string? TeamName, int Order,
    int Games, int Wins, int Losses, double WinPct, byte? Rank,
    bool IsPlayerManager, bool WonPennant, bool WonWorldSeries);

public sealed record ManagerHalfDto(
    short Year, string TeamId, string LgId, string? TeamName, int Half,
    int Games, int Wins, int Losses, byte? Rank);

public sealed record ManagerAwardDto(short Year, string AwardId, string LgId, bool HasVotingData);
