using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Api.Endpoints;

public static class PostseasonEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/series", GetSeries).WithSummary("Postseason series results");
        group.MapGet("/series/{year:int}", GetSeriesByYear).WithSummary("All postseason series in a given year");
    }

    private static async Task<IResult> GetSeries(
        BaseballDbContext context, int? year = null, string? round = null,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.SeriesPost.AsQueryable();
        if (year.HasValue) query = query.Where(s => s.YearId == year.Value);
        if (!string.IsNullOrEmpty(round)) query = query.Where(s => s.Round == round);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var data = await query
            .OrderByDescending(s => s.YearId)
            .ThenBy(s => s.Round)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new PostseasonSeriesDto(
                s.YearId, s.Round, s.TeamIdwinner, s.LgIdwinner,
                s.TeamIdloser, s.LgIdloser, s.Wins, s.Losses, s.Ties))
            .ToListAsync();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }

    private static async Task<IResult> GetSeriesByYear(int year, BaseballDbContext context)
    {
        var series = await context.SeriesPost
            .Where(s => s.YearId == year)
            .OrderBy(s => s.Round)
            .Select(s => new PostseasonSeriesDto(
                s.YearId, s.Round, s.TeamIdwinner, s.LgIdwinner,
                s.TeamIdloser, s.LgIdloser, s.Wins, s.Losses, s.Ties))
            .ToListAsync();

        return series.Count == 0 ? Results.NotFound() : Results.Ok(series);
    }
}
