using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.AllStar;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context) : PageModel
{
    public AllStarIndexViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Game identity mixes Retrosheet game IDs (AL/NL) and squad codes
        // (East-West, North-South), so classify in memory
        var rows = await context.AllstarFull
            .Select(a => new { a.YearId, a.GameId, a.LgId, a.PlayerId })
            .ToListAsync();

        ViewModel.Years = rows
            .GroupBy(a => a.YearId)
            .Select(g => new AllStarYearSummary
            {
                Year = g.Key,
                GameCount = g.Select(a => AllStarGames.GroupKey(a.LgId, a.GameId)).Distinct().Count(),
                Selections = g.Select(a => a.PlayerId).Distinct().Count(),
                HasEastWestGame = g.Any(a => a.LgId is "EAS" or "WES")
            })
            .OrderByDescending(y => y.Year)
            .ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_AllStarYearList", ViewModel);
        }

        return Page();
    }
}
