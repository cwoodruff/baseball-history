namespace baseball_history_mcp.Querying;

public sealed record NegroLeagueSummaryReadModel(
    string LeagueId,
    string Name,
    short FirstYear,
    short LastYear,
    string Summary,
    int TeamSeasons,
    int Clubs);

public sealed record NegroLeagueSeasonSummaryReadModel(
    short Year,
    int TeamCount,
    string? ChampionTeamId,
    string? ChampionName);

public sealed record NegroLeagueClubReadModel(
    string TeamId,
    string Name,
    short FirstYear,
    short LastYear,
    int Seasons,
    int Wins,
    int Losses,
    double WinningPercentage,
    int Pennants);

public sealed record NegroLeagueReadModel(
    string LeagueId,
    string Name,
    short FirstYear,
    short LastYear,
    string Summary,
    IReadOnlyList<NegroLeagueSeasonSummaryReadModel> Seasons,
    IReadOnlyList<NegroLeagueClubReadModel> Clubs);

public sealed record NegroLeagueStandingsReadModel(
    string TeamId,
    string Name,
    byte? Rank,
    int Wins,
    int Losses,
    int? Games,
    double WinningPercentage,
    double GamesBehind,
    bool WonPennant);

public sealed record NegroLeagueLeaderReadModel(
    int Rank,
    string PlayerId,
    string PlayerName,
    string? TeamName,
    double Value,
    bool IsInHallOfFame);

/// <summary>Batting average and ERA leaders use season-relative qualification; HR and wins don't</summary>
public sealed record NegroLeagueSeasonReadModel(
    string LeagueId,
    string LeagueName,
    short Year,
    short? PreviousYear,
    short? NextYear,
    IReadOnlyList<NegroLeagueStandingsReadModel> Standings,
    IReadOnlyList<NegroLeagueLeaderReadModel> BattingAverageLeaders,
    IReadOnlyList<NegroLeagueLeaderReadModel> HomeRunLeaders,
    IReadOnlyList<NegroLeagueLeaderReadModel> EraLeaders,
    IReadOnlyList<NegroLeagueLeaderReadModel> WinLeaders);

public interface INegroLeagueReadService
{
    Task<IReadOnlyList<NegroLeagueSummaryReadModel>> ListLeaguesAsync(CancellationToken cancellationToken = default);
    Task<NegroLeagueReadModel?> GetLeagueAsync(string leagueId, CancellationToken cancellationToken = default);
    Task<NegroLeagueSeasonReadModel?> GetLeagueSeasonAsync(string leagueId, int year, CancellationToken cancellationToken = default);
}
