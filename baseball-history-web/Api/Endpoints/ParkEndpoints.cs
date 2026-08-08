using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Api.Endpoints;

public static class ParkEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetParks).WithSummary("List ballparks");
        group.MapGet("/{parkKey}", GetParkDetail).WithSummary("Park detail with season history");
    }

    private static async Task<IResult> GetParks(
        BaseballDbContext context, string? state = null, int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.Parks.AsQueryable();
        if (!string.IsNullOrEmpty(state)) query = query.Where(p => p.State == state);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var data = await query
            .OrderBy(p => p.Parkname)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ParkDto(p.Parkkey, p.Parkname, p.City, p.State, p.Country))
            .ToListAsync();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }

    private static async Task<IResult> GetParkDetail(string parkKey, BaseballDbContext context)
    {
        var park = await context.Parks.FirstOrDefaultAsync(p => p.Parkkey == parkKey);
        if (park == null) return Results.NotFound();

        var seasons = await context.HomeGames
            .Where(h => h.Parkkey == parkKey)
            .OrderByDescending(h => h.Yearkey)
            .Select(h => new ParkSeasonDto(
                h.Yearkey, h.Teamkey, h.Leaguekey,
                h.Games, h.Attendance))
            .ToListAsync();

        return Results.Ok(new ParkDetailDto(
            park.Parkkey, park.Parkname, park.City, park.State, park.Country, seasons));
    }
}
