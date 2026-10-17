using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.Managers;

[HtmxPageResponseCache]
public class IndexModel(ManagerQueryService managerService) : HtmxPageModel
{
    public ManagerListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync([FromQuery] string? q = null, [FromQuery] string? sort = null,
        [FromQuery] int page = 1)
    {
        ViewModel.SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        ViewModel.Sort = sort == "name" ? "name" : "wins";
        ViewModel.CurrentPage = Math.Max(1, page);

        await managerService.LoadManagerListAsync(ViewModel);

        return HtmxOrPage("_ManagersContent", ViewModel);
    }
}
