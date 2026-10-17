using baseball_history_web.Api.Dtos;
using baseball_history_web.ViewModels;

namespace baseball_history_web.Api;

/// <summary>
/// Maps the view models shared with the web pages onto API DTOs, so pages and
/// endpoints are backed by the same queries.
/// </summary>
internal static class ApiMappings
{
    public static double WinPct(int wins, int losses) =>
        wins + losses > 0 ? Math.Round((double)wins / (wins + losses), 3) : 0;

    public static ManagerSeasonDto ToDto(this ManagerSeasonRow s) => new(
        s.Year, s.TeamId, s.LgId, s.TeamName, s.Inseason,
        s.Games ?? 0, s.Wins ?? 0, s.Losses ?? 0, WinPct(s.Wins ?? 0, s.Losses ?? 0), s.Rank,
        s.IsPlayerManager, s.WonPennant, s.WonWorldSeries);

    public static ManagerListItem ToDto(this ManagerSummary m) => new(
        m.PlayerId, m.FullName.Trim(), m.FirstYear, m.LastYear, m.Seasons,
        m.Games, m.Wins, m.Losses, WinPct(m.Wins, m.Losses), m.IsInHallOfFame);

    public static ManagerDetailDto ToDto(this ManagerDetailViewModel m) => new(
        m.PlayerId, m.FullName, m.IsInHallOfFame, m.WasPlayer, m.WasPlayerManager,
        m.FirstYear, m.LastYear, m.SeasonCount, m.TeamCount,
        m.Games, m.Wins, m.Losses, WinPct(m.Wins, m.Losses), m.Pennants, m.WorldSeriesTitles,
        m.Seasons.Select(s => s.ToDto()).ToList(),
        m.Halves.Select(h => new ManagerHalfDto(
            h.Year, h.TeamId, h.LgId, h.TeamName, h.Half,
            h.Games ?? 0, h.Wins ?? 0, h.Losses ?? 0, h.Rank)).ToList(),
        m.Awards.Select(a => new ManagerAwardDto(a.Year, a.AwardId, a.LgId, a.HasVotingData)).ToList());

    public static AllStarYearDetail ToDto(this AllStarYearViewModel y) => new(
        y.Year, y.PreviousYear, y.NextYear,
        y.Games.Select(g => new AllStarGameDto(
            g.Title,
            AllStarGameType(g.GroupKey),
            g.GameNum,
            g.GameDate,
            g.Rosters.Select(r => new AllStarRosterDto(
                r.LgId,
                r.DisplayName,
                r.Players.Select(p => new AllStarPlayerDto(
                    p.PlayerId, p.FullName, p.TeamId, p.TeamName, p.LinkLgId,
                    p.IsStarter, p.StartingPos, p.IsStarter ? p.PositionName : null,
                    p.Played, p.IsInHallOfFame)).ToList())).ToList())).ToList());

    private static string AllStarGameType(string groupKey) => groupKey switch
    {
        AllStarGames.EastWestKey => "EastWest",
        AllStarGames.NorthSouthKey => "NorthSouth",
        _ when groupKey.StartsWith("MLB|") => "AllStar",
        _ => "Other"
    };

    public static NegroLeagueDetail ToDto(this NegroLeagueDetailViewModel l) => new(
        l.Info.Id, l.Info.Name, l.Info.FirstYear, l.Info.LastYear, l.Info.Summary,
        l.Seasons.Select(s => new NegroLeagueSeasonItem(s.Year, s.TeamCount, s.ChampionTeamId, s.ChampionName))
            .ToList(),
        l.Clubs.Select(c => new NegroLeagueClubDto(
            c.TeamId, c.Name, c.FirstYear, c.LastYear, c.Seasons,
            c.Wins, c.Losses, WinPct(c.Wins, c.Losses), c.Pennants)).ToList());

    public static NegroLeagueSeasonDetail ToDto(this NegroLeagueSeasonViewModel s) => new(
        s.Info.Id, s.Info.Name, s.Year, s.PreviousYear, s.NextYear,
        s.Standings.Select(r => new NegroLeagueStandingsDto(
            r.TeamId, r.Name, r.Rank, r.Wins, r.Losses, r.Games,
            WinPct(r.Wins, r.Losses), r.GamesBehind, r.WonPennant)).ToList(),
        new NegroLeagueLeadersDto(
            s.BattingAverageLeaders.ToDto(),
            s.HomeRunLeaders.ToDto(),
            s.EraLeaders.ToDto(),
            s.WinLeaders.ToDto()));

    private static List<SeasonLeaderDto> ToDto(this List<LeaderLine> lines) => lines
        .Select(l => new SeasonLeaderDto(
            l.Rank, l.PlayerId, l.PlayerName, l.TeamName, (double)l.RawValue, l.IsInHallOfFame))
        .ToList();

    public static ThisDayDto ToDto(this ThisDayViewModel d) => new(
        d.Month, d.Day, d.DateLabel,
        d.Birthdays.ToDto(), d.Debuts.ToDto(), d.Finales.ToDto());

    private static List<ThisDayEntryDto> ToDto(this List<ThisDayEntry> entries) => entries
        .Select(e => new ThisDayEntryDto(e.PlayerId, e.FullName, e.Year, e.IsInHallOfFame))
        .ToList();
}
