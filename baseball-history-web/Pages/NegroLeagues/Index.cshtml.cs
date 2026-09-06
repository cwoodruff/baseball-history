using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.NegroLeagues;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context) : PageModel
{
    public NegroLeaguesHubViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var leagueIds = Services.NegroLeagues.All.Select(l => l.Id).ToList();

        var counts = await context.Teams
            .Where(t => leagueIds.Contains(t.LgId))
            .GroupBy(t => t.LgId)
            .Select(g => new
            {
                LgId = g.Key,
                TeamSeasons = g.Count(),
                Clubs = g.Select(t => t.TeamId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.LgId);

        ViewModel.Leagues = Services.NegroLeagues.All
            .Select(info => new NegroLeagueCard
            {
                Info = info,
                TeamSeasons = counts.TryGetValue(info.Id, out var c) ? c.TeamSeasons : 0,
                ClubCount = counts.TryGetValue(info.Id, out var c2) ? c2.Clubs : 0
            })
            .ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_HubContent", ViewModel);
        }

        return Page();
    }
}
