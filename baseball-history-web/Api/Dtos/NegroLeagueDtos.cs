namespace baseball_history_web.Api.Dtos;

public sealed record NegroLeagueItem(
    string LgId, string Name, short FirstYear, short LastYear, string Summary,
    int TeamSeasons, int Clubs);

public sealed record NegroLeagueDetail(
    string LgId, string Name, short FirstYear, short LastYear, string Summary,
    IReadOnlyList<NegroLeagueSeasonItem> Seasons,
    IReadOnlyList<NegroLeagueClubDto> Clubs);

public sealed record NegroLeagueSeasonItem(short Year, int TeamCount, string? ChampionTeamId, string? ChampionName);

public sealed record NegroLeagueClubDto(
    string TeamId, string Name, short FirstYear, short LastYear, int Seasons,
    int Wins, int Losses, double WinPct, int Pennants);

public sealed record NegroLeagueSeasonDetail(
    string LgId, string LeagueName, short Year, short? PreviousYear, short? NextYear,
    IReadOnlyList<NegroLeagueStandingsDto> Standings,
    NegroLeagueLeadersDto Leaders);

public sealed record NegroLeagueStandingsDto(
    string TeamId, string Name, byte? Rank, int Wins, int Losses, int? Games,
    double WinPct, double? GamesBehind, bool WonPennant);

/// <summary>Top five per category; average and ERA use season-relative qualification</summary>
public sealed record NegroLeagueLeadersDto(
    IReadOnlyList<SeasonLeaderDto> BattingAverage,
    IReadOnlyList<SeasonLeaderDto> HomeRuns,
    IReadOnlyList<SeasonLeaderDto> Era,
    IReadOnlyList<SeasonLeaderDto> Wins);

public sealed record SeasonLeaderDto(
    int Rank, string PlayerId, string PlayerName, string? TeamName, double Value, bool IsHallOfFamer);
