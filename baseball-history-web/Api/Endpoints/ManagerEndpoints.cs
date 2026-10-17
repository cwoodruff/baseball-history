using baseball_history_web.Api.Dtos;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;

namespace baseball_history_web.Api.Endpoints;

public static class ManagerEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetManagers)
            .WithSummary("List managers with career records")
            .WithDescription("q matches first or last name (case-insensitive). sort is wins (default, career wins descending) or name.");
        group.MapGet("/{playerId}", GetManagerDetail)
            .WithSummary("Manager career: season stints, split-season halves and manager awards");
    }

    private static async Task<IResult> GetManagers(
        ManagerQueryService managerService,
        string? q = null, string? sort = null, int page = 1, int pageSize = 50)
    {
        if (sort != null && sort != "wins" && sort != "name")
            return Results.BadRequest("sort must be 'wins' or 'name'.");

        var viewModel = new ManagerListViewModel
        {
            SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            Sort = sort ?? "wins",
            CurrentPage = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100)
        };

        await managerService.LoadManagerListAsync(viewModel);

        var data = viewModel.Managers.Select(m => m.ToDto()).ToList();
        return Results.Ok(PagedResponse.Create(data, viewModel.CurrentPage, viewModel.PageSize, viewModel.TotalManagers));
    }

    private static async Task<IResult> GetManagerDetail(string playerId, ManagerQueryService managerService)
    {
        var manager = await managerService.GetManagerDetailAsync(playerId);
        return manager == null ? Results.NotFound() : Results.Ok(manager.ToDto());
    }
}
