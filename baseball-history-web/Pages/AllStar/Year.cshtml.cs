using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.AllStar;

[HtmxPageResponseCache]
public class YearModel(AllStarService allStarService) : HtmxPageModel
{
    public AllStarYearViewModel ViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(short year)
    {
        var viewModel = await allStarService.GetYearAsync(year);
        if (viewModel == null)
        {
            return NotFound();
        }

        ViewModel = viewModel;
        return HtmxOrPage("_AllStarYear", ViewModel);
    }
}
