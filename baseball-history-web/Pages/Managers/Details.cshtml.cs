using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.Managers;

[HtmxPageResponseCache]
public class DetailsModel(ManagerQueryService managerService) : HtmxPageModel
{
    public ManagerDetailViewModel Manager { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return NotFound();
        }

        var manager = await managerService.GetManagerDetailAsync(playerId);
        if (manager == null)
        {
            return NotFound();
        }

        Manager = manager;
        return HtmxOrPage("_ManagerDetail", Manager);
    }
}
