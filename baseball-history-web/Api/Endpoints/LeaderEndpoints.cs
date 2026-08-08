using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Querying;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class LeaderEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/batting", GetBattingLeaders).WithSummary("Batting leaderboard (career or single-season)");
        group.MapGet("/pitching", GetPitchingLeaders).WithSummary("Pitching leaderboard (career or single-season)");
    }

    private static async Task<IResult> GetBattingLeaders(
        ILeaderboardQueryService leaderboardService,
        string stat = "hr", int? fromYear = null, int? toYear = null,
        string? league = null, int? minAb = null, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var request = new LeaderboardRequest(
            Stat: stat,
            FromYear: fromYear,
            ToYear: toYear,
            League: league,
            SingleSeason: singleSeason,
            Qualified: !minAb.HasValue,  // If explicit minAb is provided, don't use qualification
            MinAtBats: minAb,
            MinInningsPitched: null,
            Page: page,
            PageSize: pageSize
        );

        var result = await leaderboardService.GetBattingLeadersAsync(request);

        // Map to API DTOs
        var leaders = result.Rows.Select(r => new BattingLeaderDto(
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

        return Results.Ok(PagedResponse.Create(leaders, result.Page, result.PageSize, result.TotalCount));
    }

    private static async Task<IResult> GetPitchingLeaders(
        ILeaderboardQueryService leaderboardService,
        string stat = "w", int? fromYear = null, int? toYear = null,
        string? league = null, int? minIp = null, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var request = new LeaderboardRequest(
            Stat: stat,
            FromYear: fromYear,
            ToYear: toYear,
            League: league,
            SingleSeason: singleSeason,
            Qualified: !minIp.HasValue,  // If explicit minIp is provided, don't use qualification
            MinAtBats: null,
            MinInningsPitched: minIp,
            Page: page,
            PageSize: pageSize
        );

        var result = await leaderboardService.GetPitchingLeadersAsync(request);

        // Map to API DTOs
        var leaders = result.Rows.Select(r => new PitchingLeaderDto(
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
            EarnedRuns: 0,  // Not included in PitchingLeaderRow - it's used only for ERA calculation
            HomeRuns: r.HR,
            Walks: r.BB,
            Strikeouts: r.SO,
            Era: (double)(r.ERA ?? 0),
            Whip: (double)(r.WHIP ?? 0)
        )).ToList();

        return Results.Ok(PagedResponse.Create(leaders, result.Page, result.PageSize, result.TotalCount));
    }
}
