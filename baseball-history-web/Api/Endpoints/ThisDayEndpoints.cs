using baseball_history_web.Services;
using baseball_history_web.ViewModels;

namespace baseball_history_web.Api.Endpoints;

public static class ThisDayEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetThisDay)
            .WithSummary("This Day in Baseball: birthdays, debuts and final games on a calendar day")
            .WithDescription("date is MM-DD (e.g. 02-06) and defaults to today's server date. limit caps each category (1-25, default 5); Hall of Famers rank first, then most recent.");
    }

    private static async Task<IResult> GetThisDay(
        ThisDayService thisDayService, string? date = null, int limit = ThisDayService.DefaultEntriesPerCategory)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        (int Month, int Day) day = (today.Month, today.Day);
        if (date != null && !ThisDayViewModel.TryParseDay(date, out day))
            return Results.BadRequest("date must be MM-DD, e.g. 02-06.");

        limit = Math.Clamp(limit, 1, ThisDayService.MaxEntriesPerCategory);

        var thisDay = await thisDayService.GetAsync(day.Month, day.Day, limit);
        return Results.Ok(thisDay.ToDto());
    }
}
