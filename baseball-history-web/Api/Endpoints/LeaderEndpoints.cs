using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Querying;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class LeaderEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/batting", GetBattingLeaders)
            .WithSummary("Batting leaderboard (career or single-season)")
            .WithDescription("Returns batting leaderboard. qualified=true (default) applies season-relative qualification (~3.1 PA per team game); qualified=false disables it; explicit minAb overrides qualified entirely.");
        group.MapGet("/pitching", GetPitchingLeaders)
            .WithSummary("Pitching leaderboard (career or single-season)")
            .WithDescription("Returns pitching leaderboard. qualified=true (default) applies season-relative qualification (~1 IP per team game); qualified=false disables it; explicit minIp overrides qualified entirely.");
    }

    private static async Task<IResult> GetBattingLeaders(
        ILeaderboardQueryService leaderboardService,
        string stat = "hr", int? fromYear = null, int? toYear = null,
        string? league = null, bool qualified = true, int? minAb = null, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var request = new LeaderboardRequest(
            Stat: stat,
            FromYear: fromYear,
            ToYear: toYear,
            League: league,
            SingleSeason: singleSeason,
            Qualified: minAb.HasValue ? false : qualified,  // Explicit minAb overrides qualified
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
        string? league = null, bool qualified = true, int? minIp = null, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var request = new LeaderboardRequest(
            Stat: stat,
            FromYear: fromYear,
            ToYear: toYear,
            League: league,
            SingleSeason: singleSeason,
            Qualified: minIp.HasValue ? false : qualified,  // Explicit minIp overrides qualified
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
