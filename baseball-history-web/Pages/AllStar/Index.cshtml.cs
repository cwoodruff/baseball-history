using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.AllStar;

[HtmxPageResponseCache]
public class IndexModel(AllStarService allStarService) : HtmxPageModel
{
    public AllStarIndexViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewModel.Years = await allStarService.GetYearSummariesAsync();

        return HtmxOrPage("_AllStarYearList", ViewModel);
    }
}
