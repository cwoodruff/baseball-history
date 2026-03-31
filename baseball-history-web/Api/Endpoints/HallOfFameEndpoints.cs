using baseball_history_web.Api.Dtos;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Api.Endpoints;

public static class HallOfFameEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetInductees).WithSummary("List Hall of Fame inductees");
        group.MapGet("/{playerId}/voting", GetVotingHistory).WithSummary("Full HOF voting history for a player");
    }

    private static async Task<IResult> GetInductees(
        BaseballDbContext context, int? year = null, string? category = null,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.HallOfFame
            .Where(h => h.Inducted == "Y");

        if (year.HasValue) query = query.Where(h => h.Yearid == year.Value);
        if (!string.IsNullOrEmpty(category)) query = query.Where(h => h.Category == category);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var inductees = await query
            .OrderByDescending(h => h.Yearid)
            .ThenBy(h => h.Player.NameLast)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.PlayerId,
                h.Player.NameFirst,
                h.Player.NameLast,
                h.Yearid,
                h.Category,
                h.VotedBy,
                h.Votes,
                h.Ballots,
                DebutYear = h.Player.Debut,
                FinalYear = h.Player.FinalGame
            })
            .ToListAsync();

        var data = inductees.Select(h =>
        {
            double? votePct = null;
            if (int.TryParse(h.Votes, out var votes) && int.TryParse(h.Ballots, out var ballots) && ballots > 0)
                votePct = Math.Round((double)votes / ballots * 100, 1);

            return new HallOfFameInducteeDto(
                h.PlayerId, h.NameFirst, h.NameLast,
                h.Yearid, h.Category ?? "Player", h.VotedBy, votePct,
                h.DebutYear?.Year, h.FinalYear?.Year);
        }).ToList();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }

    private static async Task<IResult> GetVotingHistory(string playerId, BaseballDbContext context)
    {
        var person = await context.People.FirstOrDefaultAsync(p => p.PlayerId == playerId);
        if (person == null) return Results.NotFound();

        var history = await context.HallOfFame
            .Where(h => h.PlayerId == playerId)
            .OrderBy(h => h.Yearid)
            .Select(h => new { h.Yearid, h.VotedBy, h.Votes, h.Ballots, h.Inducted })
            .ToListAsync();

        if (history.Count == 0) return Results.NotFound();

        var votingYears = history.Select(h =>
        {
            double? votePct = null;
            if (int.TryParse(h.Votes, out var votes) && int.TryParse(h.Ballots, out var ballots) && ballots > 0)
                votePct = Math.Round((double)votes / ballots * 100, 1);

            return new VotingYearDto(h.Yearid, h.VotedBy, h.Votes, h.Ballots, votePct, h.Inducted == "Y");
        }).ToList();

        return Results.Ok(new HallOfFameVotingHistoryDto(
            playerId,
            $"{person.NameFirst} {person.NameLast}".Trim(),
            votingYears));
    }
}
