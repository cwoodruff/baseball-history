using baseball_history_web.Api.Endpoints;

namespace baseball_history_web.Api;

public static class ApiEndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        PlayerEndpoints.Map(api.MapGroup("/players").WithTags("Players"));
        TeamEndpoints.Map(api.MapGroup("/teams").WithTags("Teams"));
        LeaderEndpoints.Map(api.MapGroup("/leaders").WithTags("Leaders"));
        HallOfFameEndpoints.Map(api.MapGroup("/hall-of-fame").WithTags("Hall of Fame"));
        SearchEndpoints.Map(api.MapGroup("/search").WithTags("Search"));
        SalaryEndpoints.Map(api.MapGroup("/salaries").WithTags("Salaries"));
        ParkEndpoints.Map(api.MapGroup("/parks").WithTags("Parks"));
        PostseasonEndpoints.Map(api.MapGroup("/postseason").WithTags("Postseason"));
        AwardEndpoints.Map(api.MapGroup("/awards").WithTags("Awards"));

        return app;
    }
}
