using baseball_history_web.Api.Dtos;
using baseball_history_web.Services;

namespace baseball_history_web.Api.Endpoints;

public static class AllStarEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetYears)
            .WithSummary("All-Star seasons with game and selection counts")
            .WithDescription("One row per season, newest first. 1959-1962 had two AL-NL games; 1933-1948 also include the Negro Leagues East-West Game.");
        group.MapGet("/{year:int}", GetYear)
            .WithSummary("One season's All-Star games with rosters by league or squad");
    }

    private static async Task<IResult> GetYears(AllStarService allStarService)
    {
        var years = await allStarService.GetYearSummariesAsync();
        return Results.Ok(years
            .Select(y => new AllStarYearItem(y.Year, y.GameCount, y.Selections, y.HasEastWestGame))
            .ToList());
    }

    private static async Task<IResult> GetYear(int year, AllStarService allStarService)
    {
        if (year is < short.MinValue or > short.MaxValue) return Results.NotFound();

        var viewModel = await allStarService.GetYearAsync((short)year);
        return viewModel == null ? Results.NotFound() : Results.Ok(viewModel.ToDto());
    }
}
