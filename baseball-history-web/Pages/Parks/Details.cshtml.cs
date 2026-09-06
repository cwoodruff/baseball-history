using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Parks;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class DetailsModel(BaseballDbContext context) : PageModel
{
    public ParkDetailViewModel Park { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string parkKey)
    {
        if (string.IsNullOrEmpty(parkKey))
        {
            return NotFound();
        }

        var park = await context.Parks
            .Where(p => p.Parkkey == parkKey)
            .Select(p => new ParkDetailViewModel
            {
                ParkKey = p.Parkkey!,
                ParkName = p.Parkname,
                Alias = p.Parkalias,
                City = p.City,
                State = p.State,
                Country = p.Country
            })
            .FirstOrDefaultAsync();

        if (park == null)
        {
            return NotFound();
        }

        park.Seasons = await context.HomeGames
            .Where(h => h.Parkkey == parkKey)
            .OrderByDescending(h => h.Yearkey)
            .ThenBy(h => h.Teamkey)
            .Select(h => new ParkSeasonRow
            {
                Year = h.Yearkey,
                TeamId = h.Teamkey,
                LgId = h.Leaguekey,
                TeamName = h.Team.Name,
                Games = h.Games,
                Openings = h.Openings,
                Attendance = h.Attendance,
                SpanFirst = h.Spanfirst,
                SpanLast = h.Spanlast
            })
            .ToListAsync();

        park.Tenants = park.Seasons
            .GroupBy(s => (s.TeamId, s.LgId))
            .Select(g => new ParkTenant
            {
                TeamId = g.Key.TeamId,
                LgId = g.Key.LgId,
                TeamName = g.OrderByDescending(s => s.Year).First().TeamName ?? g.Key.TeamId,
                FirstYear = g.Min(s => s.Year),
                LastYear = g.Max(s => s.Year),
                SeasonCount = g.Select(s => s.Year).Distinct().Count(),
                TotalGames = g.Sum(s => s.Games ?? 0)
            })
            .OrderByDescending(t => t.LastYear)
            .ThenBy(t => t.TeamName)
            .ToList();

        Park = park;

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_ParkDetail", Park);
        }

        return Page();
    }
}
