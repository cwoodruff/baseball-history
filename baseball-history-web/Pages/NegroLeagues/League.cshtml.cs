using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.NegroLeagues;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class LeagueModel(BaseballDbContext context) : PageModel
{
    public NegroLeagueDetailViewModel ViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string lgId)
    {
        var info = Services.NegroLeagues.Find(lgId);
        if (info == null)
        {
            return NotFound();
        }

        var seasons = await context.Teams
            .Where(t => t.LgId == info.Id)
            .GroupBy(t => t.YearId)
            .Select(g => new NegroLeagueSeasonSummary
            {
                Year = g.Key,
                TeamCount = g.Count(),
                ChampionTeamId = g.Where(t => t.LgWin == "Y").Select(t => t.TeamId).FirstOrDefault(),
                ChampionName = g.Where(t => t.LgWin == "Y").Select(t => t.Name).FirstOrDefault()
            })
            .OrderByDescending(s => s.Year)
            .ToListAsync();

        var clubs = await context.Teams
            .Where(t => t.LgId == info.Id)
            .GroupBy(t => t.TeamId)
            .Select(g => new NegroLeagueClub
            {
                TeamId = g.Key,
                Name = g.OrderByDescending(t => t.YearId).Select(t => t.Name).First() ?? g.Key,
                FirstYear = g.Min(t => t.YearId),
                LastYear = g.Max(t => t.YearId),
                Seasons = g.Count(),
                Wins = g.Sum(t => (int)(t.W ?? 0)),
                Losses = g.Sum(t => (int)(t.L ?? 0)),
                Pennants = g.Count(t => t.LgWin == "Y")
            })
            .ToListAsync();

        ViewModel = new NegroLeagueDetailViewModel
        {
            Info = info,
            Seasons = seasons,
            Clubs = clubs
                .OrderByDescending(c => c.Pennants)
                .ThenByDescending(c => c.Seasons)
                .ThenBy(c => c.Name)
                .ToList()
        };

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_LeagueDetail", ViewModel);
        }

        return Page();
    }
}
