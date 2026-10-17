using baseball_history_web.Api.Dtos;
using baseball_history_web.Services;

namespace baseball_history_web.Api.Endpoints;

public static class NegroLeagueEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetLeagues)
            .WithSummary("The seven Negro Leagues recognized as major leagues (1920-1948)");
        group.MapGet("/{lgId}", GetLeague)
            .WithSummary("League seasons with champions, and its clubs' aggregate records")
            .WithDescription("lgId is one of NNL, ECL, ANL, EWL, NSL, NN2, NAL (case-insensitive).");
        group.MapGet("/{lgId}/{year:int}", GetSeason)
            .WithSummary("League season standings and top-five leaders (AVG, HR, ERA, W)");
    }

    private static async Task<IResult> GetLeagues(NegroLeagueService negroLeagueService)
    {
        var cards = await negroLeagueService.GetLeagueCardsAsync();
        return Results.Ok(cards
            .Select(c => new NegroLeagueItem(
                c.Info.Id, c.Info.Name, c.Info.FirstYear, c.Info.LastYear, c.Info.Summary,
                c.TeamSeasons, c.ClubCount))
            .ToList());
    }

    private static async Task<IResult> GetLeague(string lgId, NegroLeagueService negroLeagueService)
    {
        var league = await negroLeagueService.GetLeagueAsync(lgId);
        return league == null ? Results.NotFound() : Results.Ok(league.ToDto());
    }

    private static async Task<IResult> GetSeason(string lgId, int year, NegroLeagueService negroLeagueService)
    {
        if (year is < short.MinValue or > short.MaxValue) return Results.NotFound();

        var season = await negroLeagueService.GetSeasonAsync(lgId, (short)year);
        return season == null ? Results.NotFound() : Results.Ok(season.ToDto());
    }
}
