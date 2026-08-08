using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class SearchEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", Search).WithSummary("Search players and franchises by name");
    }

    private static async Task<IResult> Search(
        BaseballDbContext context, IMemoryCache cache, string? q, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Results.BadRequest("Query must be at least 2 characters.");

        limit = Math.Clamp(limit, 1, 50);
        var searchTerm = q.Trim().ToLower();

        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        // Search players
        var playerQuery = context.People
            .Where(p =>
                (p.NameFirst != null && p.NameFirst.ToLower().Contains(searchTerm)) ||
                (p.NameLast != null && p.NameLast.ToLower().Contains(searchTerm)) ||
                (p.NameFirst != null && p.NameLast != null &&
                 (p.NameFirst.ToLower() + " " + p.NameLast.ToLower()).Contains(searchTerm)));

        var totalPlayerCount = await playerQuery.CountAsync();

        var players = await playerQuery
            .OrderByDescending(p => (p.Battings.Sum(b => (int?)b.Hr) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.W) ?? 0))
            .ThenBy(p => p.NameLast)
            .Take(limit)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, p.Debut, p.FinalGame })
            .ToListAsync();

        var playerResults = players.Select(p => new PlayerSearchResult(
            p.PlayerId,
            $"{p.NameFirst} {p.NameLast}".Trim(),
            p.Debut?.Year, p.FinalGame?.Year,
            hofPlayerIds.Contains(p.PlayerId)
        )).ToList();

        // Search franchises
        var franchiseQuery = context.TeamsFranchises
            .Where(f => f.FranchName != null && f.FranchName.ToLower().Contains(searchTerm));

        var totalFranchiseCount = await franchiseQuery.CountAsync();

        var franchises = await franchiseQuery
            .OrderByDescending(f => f.Active == "Y")
            .ThenBy(f => f.FranchName)
            .Take(limit)
            .ToListAsync();

        var franchiseIds = franchises.Select(f => f.FranchId).ToList();
        var latestTeams = await context.Teams
            .Where(t => t.FranchId != null && franchiseIds.Contains(t.FranchId))
            .GroupBy(t => t.FranchId)
            .Select(g => new { FranchId = g.Key, TeamId = g.OrderByDescending(t => t.YearId).First().TeamId })
            .ToDictionaryAsync(x => x.FranchId!, x => x.TeamId);

        var franchiseResults = franchises.Select(f => new FranchiseSearchResult(
            f.FranchId, f.FranchName ?? f.FranchId,
            f.Active == "Y",
            latestTeams.GetValueOrDefault(f.FranchId)
        )).ToList();

        return Results.Ok(new SearchResponse(q, playerResults, franchiseResults, totalPlayerCount, totalFranchiseCount));
    }
}
