using System.ComponentModel;
using baseball_history_mcp.Querying;
using ModelContextProtocol.Server;

namespace baseball_history_mcp.Tools;

[McpServerToolType]
public sealed class BaseballReferenceTools(
    IPlayerReadService players,
    IFranchiseReadService franchises,
    ITeamReadService teams,
    ILeaderboardReadService leaderboards,
    IHallOfFameReadService hallOfFame,
    ISalaryReadService salaries,
    IPostseasonReadService postseason,
    IFieldingReadService fielding,
    IParkReadService parks,
    IManagerReadService managers,
    IAllStarReadService allStar,
    INegroLeagueReadService negroLeagues,
    IThisDayReadService thisDay)
{
    [McpServerTool(Name = "search_players", ReadOnly = true, Title = "Search Players"), Description("Search players by free-text query or last-name prefix, optionally filtered by position and era and sorted by career totals, with paging.")]
    public Task<PagedReadResult<PlayerLookupItem>> SearchPlayersAsync(
        [Description("Optional free-text player search across player id, first name, and last name.")] string? query = null,
        [Description("Optional last-name prefix when you want alphabetical lookup instead of full-text search.")] string? lastNameStartsWith = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 25,
        [Description("Optional position ever fielded: P, C, 1B, 2B, 3B, SS, OF (includes LF/CF/RF), or DH.")] string? position = null,
        [Description("Optional decade such as 1920 (any year in it works); keeps players active during that decade.")] int? era = null,
        [Description("Optional sort: name (default), or hr, hits, games for career totals descending.")] string? sort = null,
        CancellationToken cancellationToken = default)
        => players.SearchPlayersAsync(new PlayerLookupRequest(query, lastNameStartsWith, page, pageSize, position, era, sort), cancellationToken);

    [McpServerTool(Name = "get_player", ReadOnly = true, Title = "Get Player"), Description("Get read-only detail for one player, including career batting, career pitching, team tenures, career OPS vs. league, All-Star selection years, and whether the player also managed.")]
    public Task<PlayerReadModel?> GetPlayerAsync(
        [Description("Lahman player id, for example ruthba01.")] string playerId,
        CancellationToken cancellationToken = default)
        => players.GetPlayerAsync(McpInputValidator.NormalizePlayerId(playerId), cancellationToken);

    [McpServerTool(Name = "get_player_advanced_stats", ReadOnly = true, Title = "Get Player Advanced Stats"), Description("Get one player's season advanced rates with stints combined: batting PA, ISO, BABIP, BB%, K%, OPS vs. league (100 = average, not park-adjusted), HR per 162; pitching IP, ERA, WHIP, K/9, BB/9; each with season-relative qualification.")]
    public Task<PlayerAdvancedStatsReadModel?> GetPlayerAdvancedStatsAsync(
        [Description("Lahman player id, for example ruthba01.")] string playerId,
        CancellationToken cancellationToken = default)
        => players.GetPlayerAdvancedStatsAsync(McpInputValidator.NormalizePlayerId(playerId), cancellationToken);

    [McpServerTool(Name = "list_franchises", ReadOnly = true, Title = "List Franchises"), Description("List franchise summaries with optional league, name, and era filters and bounded paging.")]
    public Task<PagedReadResult<FranchiseLookupItem>> ListFranchisesAsync(
        [Description("Optional league filter such as AL or NL.")] string? league = null,
        [Description("Set true to return only active franchises.")] bool activeOnly = false,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 25,
        [Description("Optional franchise name contains filter (case-insensitive), for example Giants.")] string? query = null,
        [Description("Optional decade such as 1900 (any year in it works); keeps franchises that fielded a team during it.")] int? era = null,
        CancellationToken cancellationToken = default)
        => franchises.ListFranchisesAsync(new FranchiseLookupRequest(league, activeOnly, page, pageSize, query, era), cancellationToken);

    [McpServerTool(Name = "get_franchise", ReadOnly = true, Title = "Get Franchise"), Description("Get one franchise with season-by-season history.")]
    public Task<FranchiseReadModel?> GetFranchiseAsync(
        [Description("Franchise id such as NYY.")] string franchiseId,
        CancellationToken cancellationToken = default)
        => franchises.GetFranchiseAsync(McpInputValidator.NormalizeFranchiseId(franchiseId), cancellationToken);

    [McpServerTool(Name = "get_team_season", ReadOnly = true, Title = "Get Team Season"), Description("Get one exact team-season by team id, league, and year so franchise-era lookups stay deterministic.")]
    public Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        [Description("Team id such as NYA.")] string teamId,
        [Description("Exact league id such as AL or NL.")] string league,
        [Description("Exact season year.")] int year,
        CancellationToken cancellationToken = default)
        => teams.GetTeamSeasonAsync(teamId, league, year, cancellationToken);

    [McpServerTool(Name = "get_batting_leaders", ReadOnly = true, Title = "Get Batting Leaders"), Description("Read batting leaderboards in career or single-season form. Supported stats: hr, h, r, rbi, sb, 2b, 3b, bb, so, g, ab, avg, obp, slg, ops.")]
    public Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(
        [Description("Stat to rank by. Use one of: hr, h, r, rbi, sb, 2b, 3b, bb, so, g, ab, avg, obp, slg, ops. Aliases like hits, homeruns, and strikeouts are also accepted.")] string stat = "hr",
        [Description("Optional lower year bound.")] int? fromYear = null,
        [Description("Optional upper year bound.")] int? toYear = null,
        [Description("Optional league filter such as AL or NL.")] string? league = null,
        [Description("Apply season-relative qualification (default true: ~3.1 PA per team game). Set false to disable qualification. Overridden by explicit minAtBats.")] bool qualified = true,
        [Description("Explicit minimum at-bats threshold. When provided, overrides the qualified parameter entirely.")] int minAtBats = 0,
        [Description("Set true for single-season rows; false for career aggregates.")] bool singleSeason = false,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        McpInputValidator.ValidateYearRange(fromYear, toYear);
        return leaderboards.GetBattingLeadersAsync(
            new BattingLeaderboardQuery(
                McpInputValidator.NormalizeBattingStat(stat),
                fromYear,
                toYear,
                McpInputValidator.NormalizeLeague(league),
                qualified,
                minAtBats,
                singleSeason,
                page,
                pageSize),
            cancellationToken);
    }

    [McpServerTool(Name = "get_pitching_leaders", ReadOnly = true, Title = "Get Pitching Leaders"), Description("Read pitching leaderboards in career or single-season form. Supported stats: w, l, so, sv, cg, sho, ip, g, gs, hr, k9, wpct, era, whip, bb9.")]
    public Task<PagedReadResult<PitchingLeaderboardEntry>> GetPitchingLeadersAsync(
        [Description("Stat to rank by. Use one of: w, l, so, sv, cg, sho, ip, g, gs, hr, k9, wpct, era, whip, bb9. Aliases like wins and strikeouts are also accepted.")] string stat = "w",
        [Description("Optional lower year bound.")] int? fromYear = null,
        [Description("Optional upper year bound.")] int? toYear = null,
        [Description("Optional league filter such as AL or NL.")] string? league = null,
        [Description("Apply season-relative qualification (default true: ~1 IP per team game). Set false to disable qualification. Overridden by explicit minInningsPitched.")] bool qualified = true,
        [Description("Explicit minimum innings-pitched threshold. When provided, overrides the qualified parameter entirely.")] int minInningsPitched = 0,
        [Description("Set true for single-season rows; false for career aggregates.")] bool singleSeason = false,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        McpInputValidator.ValidateYearRange(fromYear, toYear);
        return leaderboards.GetPitchingLeadersAsync(
            new PitchingLeaderboardQuery(
                McpInputValidator.NormalizePitchingStat(stat),
                fromYear,
                toYear,
                McpInputValidator.NormalizeLeague(league),
                qualified,
                minInningsPitched,
                singleSeason,
                page,
                pageSize),
            cancellationToken);
    }

    [McpServerTool(Name = "list_hall_of_fame_inductees", ReadOnly = true, Title = "List Hall of Fame Inductees"), Description("List inducted Hall of Fame rows with optional year/category filters and bounded paging.")]
    public Task<PagedReadResult<HallOfFameInducteeReadModel>> ListHallOfFameInducteesAsync(
        [Description("Optional induction year filter.")] int? year = null,
        [Description("Optional Hall of Fame category filter such as Player, Manager, or Pioneer/Executive.")] string? category = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured Hall of Fame max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => hallOfFame.ListInducteesAsync(new HallOfFameLookupRequest(year, category, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_hall_of_fame_voting_history", ReadOnly = true, Title = "Get Hall of Fame Voting History"), Description("Get bounded Hall of Fame voting history for one player. Returned rows are Lahman HallOfFame ballot rows, not a prose biography.")]
    public Task<HallOfFameVotingHistoryReadModel?> GetHallOfFameVotingHistoryAsync(
        [Description("Lahman player id, for example ripkeca01.")] string playerId,
        CancellationToken cancellationToken = default)
        => hallOfFame.GetVotingHistoryAsync(playerId, cancellationToken);

    [McpServerTool(Name = "get_player_salary_history", ReadOnly = true, Title = "Get Player Salary History"), Description("Get bounded salary history for one player. Returned rows are Lahman Salaries player-team-season records ordered from most recent to oldest.")]
    public Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(
        [Description("Lahman player id, for example aaronha01.")] string playerId,
        CancellationToken cancellationToken = default)
        => salaries.GetPlayerSalaryHistoryAsync(playerId, cancellationToken);

    [McpServerTool(Name = "get_salary_leaders", ReadOnly = true, Title = "Get Salary Leaders"), Description("List highest salary rows with optional year filter and bounded paging.")]
    public Task<PagedReadResult<SalaryLeaderEntry>> GetSalaryLeadersAsync(
        [Description("Optional salary season year.")] int? year = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured salary leader max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => salaries.GetSalaryLeadersAsync(new SalaryLeaderRequest(year, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_player_postseason", ReadOnly = true, Title = "Get Player Postseason"), Description("Get bounded postseason batting and pitching lines for one player, ordered by year then round (wild card through World Series).")]
    public Task<PlayerPostseasonReadModel?> GetPlayerPostseasonAsync(
        [Description("Lahman player id, for example jeterde01.")] string playerId,
        CancellationToken cancellationToken = default)
        => postseason.GetPlayerPostseasonAsync(playerId, cancellationToken);

    [McpServerTool(Name = "get_player_fielding", ReadOnly = true, Title = "Get Player Fielding"), Description("Get bounded season-by-season fielding rows plus career-by-position totals for one player.")]
    public Task<PlayerFieldingReadModel?> GetPlayerFieldingAsync(
        [Description("Lahman player id, for example ruthba01.")] string playerId,
        CancellationToken cancellationToken = default)
        => fielding.GetPlayerFieldingAsync(playerId, cancellationToken);

    [McpServerTool(Name = "search_parks", ReadOnly = true, Title = "Search Parks"), Description("Search ballparks by name, former name, or city with optional state filter and bounded paging. Returns park keys for get_park.")]
    public Task<PagedReadResult<ParkSummaryReadModel>> SearchParksAsync(
        [Description("Optional free-text search across park name, former names, and city.")] string? query = null,
        [Description("Optional state code filter such as MA.")] string? state = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured park search max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => parks.SearchParksAsync(new ParkSearchRequest(query, state, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_park", ReadOnly = true, Title = "Get Park"), Description("Get one ballpark by park key: location, former names, home-team tenures, and bounded per-season home games and attendance.")]
    public Task<ParkReadModel?> GetParkAsync(
        [Description("Lahman park key, for example BOS07 for Fenway Park.")] string parkKey,
        CancellationToken cancellationToken = default)
        => parks.GetParkAsync(parkKey, cancellationToken);

    [McpServerTool(Name = "list_managers", ReadOnly = true, Title = "List Managers"), Description("List managers with career records and Hall of Fame flags, optionally filtered by name, sorted by career wins or name, with bounded paging. Returns player ids for get_manager.")]
    public Task<PagedReadResult<ManagerLookupItem>> ListManagersAsync(
        [Description("Optional first- or last-name contains filter, for example Mack.")] string? query = null,
        [Description("Optional sort: wins (default, career wins descending) or name.")] string? sort = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured manager list max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => managers.ListManagersAsync(new ManagerLookupRequest(query, sort, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_manager", ReadOnly = true, Title = "Get Manager"), Description("Get one manager's career by player id: bounded season-by-season records with pennant/World Series flags, player-manager stints, and manager awards.")]
    public Task<ManagerReadModel?> GetManagerAsync(
        [Description("Lahman player id, for example mackco01 for Connie Mack.")] string playerId,
        CancellationToken cancellationToken = default)
        => managers.GetManagerAsync(playerId, cancellationToken);

    [McpServerTool(Name = "list_all_star_years", ReadOnly = true, Title = "List All-Star Years"), Description("List every All-Star season, newest first, with game count, distinct selections, and whether the Negro Leagues East-West Game was played (1933-1948). 1959-1962 had two AL-NL games.")]
    public Task<IReadOnlyList<AllStarYearSummaryReadModel>> ListAllStarYearsAsync(CancellationToken cancellationToken = default)
        => allStar.ListAllStarYearsAsync(cancellationToken);

    [McpServerTool(Name = "get_all_star_year", ReadOnly = true, Title = "Get All-Star Year"), Description("Get one season's All-Star games with rosters by league or squad: starters first with position numbers (1=P ... 9=RF, 10=DH), whether each player appeared, and Hall of Fame flags. Includes the Negro Leagues East-West and North-South games.")]
    public Task<AllStarYearReadModel?> GetAllStarYearAsync(
        [Description("All-Star season year, for example 1943.")] int year,
        CancellationToken cancellationToken = default)
        => allStar.GetAllStarYearAsync(year, cancellationToken);

    [McpServerTool(Name = "list_negro_leagues", ReadOnly = true, Title = "List Negro Leagues"), Description("List the seven Negro Leagues MLB recognizes as major leagues (1920-1948) with league ids, years, a summary, and team-season and club counts.")]
    public Task<IReadOnlyList<NegroLeagueSummaryReadModel>> ListNegroLeaguesAsync(CancellationToken cancellationToken = default)
        => negroLeagues.ListLeaguesAsync(cancellationToken);

    [McpServerTool(Name = "get_negro_league", ReadOnly = true, Title = "Get Negro League"), Description("Get one Negro League's seasons with champions and its clubs' aggregate records.")]
    public Task<NegroLeagueReadModel?> GetNegroLeagueAsync(
        [Description("League id: NNL, ECL, ANL, EWL, NSL, NN2, or NAL (case-insensitive).")] string leagueId,
        CancellationToken cancellationToken = default)
        => negroLeagues.GetLeagueAsync(leagueId, cancellationToken);

    [McpServerTool(Name = "get_negro_league_season", ReadOnly = true, Title = "Get Negro League Season"), Description("Get one Negro League season: standings with games behind plus the top five in batting average and ERA (qualified), home runs, and wins.")]
    public Task<NegroLeagueSeasonReadModel?> GetNegroLeagueSeasonAsync(
        [Description("League id: NNL, ECL, ANL, EWL, NSL, NN2, or NAL (case-insensitive).")] string leagueId,
        [Description("Season year, for example 1943.")] int year,
        CancellationToken cancellationToken = default)
        => negroLeagues.GetLeagueSeasonAsync(leagueId, year, cancellationToken);

    [McpServerTool(Name = "get_this_day", ReadOnly = true, Title = "Get This Day in Baseball"), Description("Get birthdays, major league debuts, and final games on one calendar day, Hall of Famers first and then most recent.")]
    public Task<ThisDayReadModel> GetThisDayAsync(
        [Description("Optional month and day as MM-DD, for example 02-06; defaults to today's server date.")] string? date = null,
        [Description("Entries per category, 1 to 25.")] int limit = 5,
        CancellationToken cancellationToken = default)
        => thisDay.GetThisDayAsync(date, limit, cancellationToken);
}
