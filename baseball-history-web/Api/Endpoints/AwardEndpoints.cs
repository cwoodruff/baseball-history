using baseball_history_web.Api.Dtos;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Api.Endpoints;

public static class AwardEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/winners", GetWinners).WithSummary("Award winners with optional filters");
        group.MapGet("/voting/{awardId}/{year:int}/{lgId}", GetVoting)
            .WithSummary("Full voting/race for a specific award, year, and league");
    }

    private static async Task<IResult> GetWinners(
        BaseballDbContext context, string? awardId = null, int? year = null,
        string? lgId = null, int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.AwardsPlayers.AsQueryable();
        if (!string.IsNullOrEmpty(awardId)) query = query.Where(a => a.AwardId == awardId);
        if (year.HasValue) query = query.Where(a => a.YearId == year.Value);
        if (!string.IsNullOrEmpty(lgId)) query = query.Where(a => a.LgId == lgId);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var data = await query
            .OrderByDescending(a => a.YearId)
            .ThenBy(a => a.AwardId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AwardWinnerDto(
                a.PlayerId,
                (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                a.AwardId, a.YearId, a.LgId, a.Notes))
            .ToListAsync();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }

    private static async Task<IResult> GetVoting(
        string awardId, int year, string lgId, BaseballDbContext context)
    {
        var votes = await context.AwardsSharePlayers
            .Where(a => a.AwardId == awardId && a.YearId == year && a.LgId == lgId)
            .OrderByDescending(a => a.PointsWon)
            .Select(a => new AwardVoteDto(
                a.PlayerId,
                (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                a.PointsWon, a.PointsMax, a.VotesFirst,
                (a.PointsMax ?? 0) > 0
                    ? Math.Round((double)(a.PointsWon ?? 0) / (a.PointsMax ?? 1) * 100, 1)
                    : 0))
            .ToListAsync();

        if (votes.Count == 0) return Results.NotFound();

        return Results.Ok(new AwardVotingDto(awardId, (short)year, lgId, votes));
    }
}
