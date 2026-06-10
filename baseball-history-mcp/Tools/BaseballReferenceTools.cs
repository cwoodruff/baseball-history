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
    ISalaryReadService salaries)
{
    [McpServerTool(Name = "search_players", ReadOnly = true, Title = "Search Players"), Description("Search players by free-text query or last-name prefix with paging.")]
    public Task<PagedReadResult<PlayerLookupItem>> SearchPlayersAsync(
        [Description("Optional free-text player search across player id, first name, and last name.")] string? query = null,
        [Description("Optional last-name prefix when you want alphabetical lookup instead of full-text search.")] string? lastNameStartsWith = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => players.SearchPlayersAsync(new PlayerLookupRequest(query, lastNameStartsWith, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_player", ReadOnly = true, Title = "Get Player"), Description("Get read-only detail for one player, including career batting, career pitching, and team tenures.")]
    public Task<PlayerReadModel?> GetPlayerAsync(
        [Description("Lahman player id, for example ruthba01.")] string playerId,
        CancellationToken cancellationToken = default)
        => players.GetPlayerAsync(McpInputValidator.NormalizePlayerId(playerId), cancellationToken);

    [McpServerTool(Name = "list_franchises", ReadOnly = true, Title = "List Franchises"), Description("List franchise summaries with optional filters and bounded paging.")]
    public Task<PagedReadResult<FranchiseLookupItem>> ListFranchisesAsync(
        [Description("Optional league filter such as AL or NL.")] string? league = null,
        [Description("Set true to return only active franchises.")] bool activeOnly = false,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => franchises.ListFranchisesAsync(new FranchiseLookupRequest(league, activeOnly, page, pageSize), cancellationToken);

    [McpServerTool(Name = "get_franchise", ReadOnly = true, Title = "Get Franchise"), Description("Get one franchise with season-by-season history.")]
    public Task<FranchiseReadModel?> GetFranchiseAsync(
        [Description("Franchise id such as NYY.")] string franchiseId,
        CancellationToken cancellationToken = default)
        => franchises.GetFranchiseAsync(McpInputValidator.NormalizeFranchiseId(franchiseId), cancellationToken);

    [McpServerTool(Name = "get_team_season", ReadOnly = true, Title = "Get Team Season"), Description("Get one team season with franchise context, roster summaries, and club-level batting/pitching snapshots.")]
    public Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        [Description("Team id such as NYA or BOS.")] string teamId,
        [Description("League id such as AL or NL.")] string league,
        [Description("Season year within the Lahman dataset.")] int year,
        CancellationToken cancellationToken = default)
    {
        var request = McpInputValidator.NormalizeTeamSeason(teamId, league, year);
        return teamSeasons.GetTeamSeasonAsync(request.TeamId, request.League, request.Year, cancellationToken);
    }

    [McpServerTool(Name = "get_team_season", ReadOnly = true, Title = "Get Team Season"), Description("Get one exact team-season by team id, league, and year so franchise-era lookups stay deterministic.")]
    public Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        [Description("Team id such as NYA.")] string teamId,
        [Description("Exact league id such as AL or NL.")] string league,
        [Description("Exact season year.")] int year,
        CancellationToken cancellationToken = default)
        => teams.GetTeamSeasonAsync(teamId, league, year, cancellationToken);

    [McpServerTool(Name = "get_batting_leaders", ReadOnly = true, Title = "Get Batting Leaders"), Description("Read batting leaderboards in career or single-season form. Supported stats: hr, h, r, rbi, sb, 2b, 3b, bb, g, ab, avg, obp, slg, ops.")]
    public Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(
        [Description("Stat to rank by. Use one of: hr, h, r, rbi, sb, 2b, 3b, bb, g, ab, avg, obp, slg, ops. Aliases like hits and homeruns are also accepted.")] string stat = "hr",
        [Description("Optional lower year bound.")] int? fromYear = null,
        [Description("Optional upper year bound.")] int? toYear = null,
        [Description("Optional league filter such as AL or NL.")] string? league = null,
        [Description("Minimum at-bats threshold.")] int minAtBats = 0,
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
        [Description("Minimum innings-pitched threshold.")] int minInningsPitched = 0,
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
                minInningsPitched,
                singleSeason,
                page,
                pageSize),
            cancellationToken);
    }

    [McpServerTool(Name = "list_hall_of_fame_inductees", ReadOnly = true, Title = "List Hall of Fame Inductees"), Description("List Hall of Fame inductees with optional year/category filters and bounded paging.")]
    public Task<PagedReadResult<HallOfFameInducteeReadModel>> ListHallOfFameInducteesAsync(
        [Description("Optional Hall of Fame induction year filter.")] int? year = null,
        [Description("Optional category filter such as Player, Manager, or Pioneer/Executive.")] string? category = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 50,
        CancellationToken cancellationToken = default)
        => hallOfFame.ListInducteesAsync(
            new HallOfFameLookupRequest(year, category, page, pageSize),
            cancellationToken);

    [McpServerTool(Name = "list_hall_of_fame_inductees", ReadOnly = true, Title = "List Hall of Fame Inductees"), Description("List inducted Hall of Fame rows with optional year/category filters and bounded paging.")]
    public Task<PagedReadResult<HallOfFameInducteeReadModel>> ListHallOfFameInducteesAsync(
        [Description("Optional induction year filter.")] int? year = null,
        [Description("Optional Hall of Fame category filter such as Player, Manager, or Pioneer/Executive.")] string? category = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured Hall of Fame max.")] int pageSize = 25,
        CancellationToken cancellationToken = default)
        => hallOfFame.GetInducteesAsync(new HallOfFameInducteeRequest(year, category, page, pageSize), cancellationToken);

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
}
