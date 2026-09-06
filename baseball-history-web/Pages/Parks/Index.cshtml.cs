using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Parks;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context) : PageModel
{
    public ParkListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync([FromQuery] string? q = null, [FromQuery] string? state = null,
        [FromQuery] int page = 1)
    {
        ViewModel.SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        ViewModel.SelectedState = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
        ViewModel.CurrentPage = Math.Max(1, page);

        var query = context.Parks.AsQueryable();

        if (ViewModel.SelectedState != null)
        {
            query = query.Where(p => p.State == ViewModel.SelectedState);
        }

        if (ViewModel.SearchQuery != null)
        {
            var term = ViewModel.SearchQuery.ToLower();
            query = query.Where(p =>
                (p.Parkname != null && p.Parkname.ToLower().Contains(term)) ||
                (p.Parkalias != null && p.Parkalias.ToLower().Contains(term)) ||
                (p.City != null && p.City.ToLower().Contains(term)));
        }

        ViewModel.TotalParks = await query.CountAsync();

        if (ViewModel.TotalPages > 0 && ViewModel.CurrentPage > ViewModel.TotalPages)
        {
            ViewModel.CurrentPage = ViewModel.TotalPages;
        }

        ViewModel.Parks = await query
            .OrderBy(p => p.Parkname)
            .ThenBy(p => p.Parkkey)
            .Skip((ViewModel.CurrentPage - 1) * ViewModel.PageSize)
            .Take(ViewModel.PageSize)
            .Select(p => new ParkSummary
            {
                ParkKey = p.Parkkey,
                ParkName = p.Parkname,
                Alias = p.Parkalias,
                City = p.City,
                State = p.State,
                Country = p.Country,
                FirstYear = p.HomeGames.Min(h => (short?)h.Yearkey),
                LastYear = p.HomeGames.Max(h => (short?)h.Yearkey),
                TeamCount = p.HomeGames.Select(h => h.Teamkey).Distinct().Count()
            })
            .ToListAsync();

        ViewModel.StateOptions = await context.Parks
            .Where(p => p.State != null && p.State != "")
            .Select(p => p.State!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_ParksContent", ViewModel);
        }

        return Page();
    }
}
