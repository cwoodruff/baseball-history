using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.NegroLeagues;

[HtmxPageResponseCache]
public class SeasonModel(NegroLeagueService negroLeagueService) : HtmxPageModel
{
    public NegroLeagueSeasonViewModel ViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string lgId, short year)
    {
        var viewModel = await negroLeagueService.GetSeasonAsync(lgId, year);
        if (viewModel == null)
        {
            return NotFound();
        }

        ViewModel = viewModel;
        return HtmxOrPage("_LeagueSeason", ViewModel);
    }
}
