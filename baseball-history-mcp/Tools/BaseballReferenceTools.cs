using System.ComponentModel;
using baseball_history_mcp.Querying;
using ModelContextProtocol.Server;

namespace baseball_history_mcp.Tools;

[McpServerToolType]
public sealed class BaseballReferenceTools(
    IPlayerReadService players,
    IFranchiseReadService franchises,
    ILeaderboardReadService leaderboards)
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
        => players.GetPlayerAsync(playerId, cancellationToken);

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
        => franchises.GetFranchiseAsync(franchiseId, cancellationToken);

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
        => leaderboards.GetBattingLeadersAsync(
            new BattingLeaderboardQuery(stat, fromYear, toYear, league, minAtBats, singleSeason, page, pageSize),
            cancellationToken);

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
        => leaderboards.GetPitchingLeadersAsync(
            new PitchingLeaderboardQuery(stat, fromYear, toYear, league, minInningsPitched, singleSeason, page, pageSize),
            cancellationToken);
}
