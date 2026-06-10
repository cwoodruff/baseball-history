using System.ComponentModel;
using baseball_history_mcp.Querying;
using ModelContextProtocol.Server;

namespace baseball_history_mcp.Tools;

[McpServerToolType]
public sealed class BaseballReferenceTools(
    IPlayerReadService players,
    IFranchiseReadService franchises,
    ITeamSeasonReadService teamSeasons,
    IHallOfFameReadService hallOfFame,
    ILeaderboardReadService leaderboards,
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

    [McpServerTool(Name = "get_batting_leaders", ReadOnly = true, Title = "Get Batting Leaders"), Description("Read batting leaderboards in career or single-season form.")]
    public Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(
        [Description("Stat to rank by, for example hr, hits, avg, obp, slg, or ops.")] string stat = "hr",
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

    [McpServerTool(Name = "get_pitching_leaders", ReadOnly = true, Title = "Get Pitching Leaders"), Description("Read pitching leaderboards in career or single-season form.")]
    public Task<PagedReadResult<PitchingLeaderboardEntry>> GetPitchingLeadersAsync(
        [Description("Stat to rank by, for example w, so, era, whip, k9, bb9, or wpct.")] string stat = "w",
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

    [McpServerTool(Name = "get_hall_of_fame_voting_history", ReadOnly = true, Title = "Get Hall of Fame Voting History"), Description("Get the full Hall of Fame voting history for one player when Lahman voting data exists.")]
    public Task<HallOfFameVotingHistoryReadModel?> GetHallOfFameVotingHistoryAsync(
        [Description("Lahman player id, for example ruthba01.")] string playerId,
        CancellationToken cancellationToken = default)
        => hallOfFame.GetVotingHistoryAsync(McpInputValidator.NormalizePlayerId(playerId), cancellationToken);

    [McpServerTool(Name = "get_player_salary_history", ReadOnly = true, Title = "Get Player Salary History"), Description("Get a player's bounded salary history and career salary total when Lahman salary rows exist for that player.")]
    public Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(
        [Description("Lahman player id for a player with salary data, for example troutmi01.")] string playerId,
        [Description("Optional number of most recent salary rows to return, up to the configured server cap.")] int? itemCount = null,
        CancellationToken cancellationToken = default)
        => salaries.GetPlayerSalaryHistoryAsync(McpInputValidator.NormalizePlayerId(playerId), itemCount, cancellationToken);

    [McpServerTool(Name = "get_team_payroll", ReadOnly = true, Title = "Get Team Payroll"), Description("Get one team's payroll for a single season with the highest-paid roster entries for that club-year.")]
    public Task<TeamPayrollReadModel?> GetTeamPayrollAsync(
        [Description("Team id such as NYA or BOS.")] string teamId,
        [Description("Season year for the payroll snapshot.")] int year,
        [Description("Optional number of highest-paid player rows to return, up to the configured server cap.")] int? itemCount = null,
        CancellationToken cancellationToken = default)
    {
        if (year is < 1985 or > 2100)
        {
            throw new BaseballMcpUsageException("Salary history is only available from 1985 forward.");
        }

        return salaries.GetTeamPayrollAsync(McpInputValidator.NormalizeTeamId(teamId), year, itemCount, cancellationToken);
    }

    [McpServerTool(Name = "get_salary_leaders", ReadOnly = true, Title = "Get Salary Leaders"), Description("Read the highest-paid player rows with optional year filtering and bounded paging.")]
    public Task<PagedReadResult<SalaryEntryReadModel>> GetSalaryLeadersAsync(
        [Description("Optional salary year filter.")] int? year = null,
        [Description("1-based results page.")] int page = 1,
        [Description("Page size from 1 up to the configured server max.")] int pageSize = 50,
        CancellationToken cancellationToken = default)
        => salaries.GetSalaryLeadersAsync(new SalaryLeaderQuery(year, page, pageSize), cancellationToken);
}
