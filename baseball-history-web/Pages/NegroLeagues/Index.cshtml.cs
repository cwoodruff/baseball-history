using baseball_history_web.Extensions;
using baseball_history_web.Filters;
using baseball_history_web.Pages.Shared;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Pages.NegroLeagues;

[HtmxPageResponseCache]
public class IndexModel(NegroLeagueService negroLeagueService) : HtmxPageModel
{
    public NegroLeaguesHubViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewModel.Leagues = await negroLeagueService.GetLeagueCardsAsync();

        return HtmxOrPage("_HubContent", ViewModel);
    }
}
