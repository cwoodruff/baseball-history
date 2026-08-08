using baseball_history_mcp.Configuration;
using BaseballHistory.Data.Querying;

namespace baseball_history_mcp.Querying;

/// <summary>
/// Adapter implementation that bridges MCP's ILeaderboardReadService interface
/// to the shared ILeaderboardQueryService from the data layer.
/// </summary>
public sealed class LeaderboardReadService(
    ILeaderboardQueryService queryService,
    BaseballMcpRequestPolicy requestPolicy) : ILeaderboardReadService
{
    public async Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(
        BattingLeaderboardQuery request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);

        // Map MCP request to shared service request
        var serviceRequest = new LeaderboardRequest(
            Stat: normalizedRequest.Stat,
            FromYear: normalizedRequest.FromYear,
            ToYear: normalizedRequest.ToYear,
            League: normalizedRequest.League,
            SingleSeason: normalizedRequest.SingleSeason,
            Qualified: normalizedRequest.MinAtBats == 0,  // If explicit min is 0, use qualification
            MinAtBats: normalizedRequest.MinAtBats > 0 ? normalizedRequest.MinAtBats : null,
            MinInningsPitched: null,
            Page: normalizedRequest.Page,
            PageSize: normalizedRequest.PageSize
        );

        var result = await queryService.GetBattingLeadersAsync(serviceRequest, cancellationToken);

        // Map service response to MCP response
        var entries = result.Rows.Select(r => new BattingLeaderboardEntry(
            Rank: r.Rank,
            PlayerId: r.PlayerId,
            PlayerName: r.PlayerName,
            Year: r.YearId,
            TeamId: r.TeamId,
            TeamName: r.TeamName,
            IsHallOfFamer: r.IsHallOfFamer,
            Games: r.G,
            AtBats: r.AB,
            Runs: r.R,
            Hits: r.H,
            Doubles: r.Doubles,
            Triples: r.Triples,
            HomeRuns: r.HR,
            Rbi: r.RBI,
            StolenBases: r.SB,
            Walks: r.BB,
            BattingAverage: (double)(r.AVG ?? 0),
            Obp: (double)(r.OBP ?? 0),
            Slg: (double)(r.SLG ?? 0),
            Ops: (double)(r.OPS ?? 0)
        )).ToList();

        return new PagedReadResult<BattingLeaderboardEntry>(
            Items: entries,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages
        )
        {
            RequestedPageSize = request.PageSize,
            MaxPageSize = normalizedRequest.PageSize,
            WasPageSizeClamped = request.PageSize != normalizedRequest.PageSize
        };
    }

    public async Task<PagedReadResult<PitchingLeaderboardEntry>> GetPitchingLeadersAsync(
        PitchingLeaderboardQuery request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);

        // Map MCP request to shared service request
        var serviceRequest = new LeaderboardRequest(
            Stat: normalizedRequest.Stat,
            FromYear: normalizedRequest.FromYear,
            ToYear: normalizedRequest.ToYear,
            League: normalizedRequest.League,
            SingleSeason: normalizedRequest.SingleSeason,
            Qualified: normalizedRequest.MinInningsPitched == 0,  // If explicit min is 0, use qualification
            MinAtBats: null,
            MinInningsPitched: normalizedRequest.MinInningsPitched > 0 ? normalizedRequest.MinInningsPitched : null,
            Page: normalizedRequest.Page,
            PageSize: normalizedRequest.PageSize
        );

        var result = await queryService.GetPitchingLeadersAsync(serviceRequest, cancellationToken);

        // Map service response to MCP response
        var entries = result.Rows.Select(r => new PitchingLeaderboardEntry(
            Rank: r.Rank,
            PlayerId: r.PlayerId,
            PlayerName: r.PlayerName,
            Year: r.YearId,
            TeamId: r.TeamId,
            TeamName: r.TeamName,
            IsHallOfFamer: r.IsHallOfFamer,
            Games: r.G,
            GamesStarted: r.GS,
            Wins: r.W,
            Losses: r.L,
            Saves: r.SV,
            CompleteGames: r.CG,
            Shutouts: r.SHO,
            InningsPitched: (double)r.IP,
            Hits: r.H,
            EarnedRuns: 0,  // Not included in service response; used only for ERA calculation
            HomeRuns: r.HR,
            Walks: r.BB,
            Strikeouts: r.SO,
            Era: (double)(r.ERA ?? 0),
            Whip: (double)(r.WHIP ?? 0)
        )).ToList();

        return new PagedReadResult<PitchingLeaderboardEntry>(
            Items: entries,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: result.TotalPages
        )
        {
            RequestedPageSize = request.PageSize,
            MaxPageSize = normalizedRequest.PageSize,
            WasPageSizeClamped = request.PageSize != normalizedRequest.PageSize
        };
    }
}
