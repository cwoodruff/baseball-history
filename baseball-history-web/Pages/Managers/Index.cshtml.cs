using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Managers;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public ManagerListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync([FromQuery] string? q = null, [FromQuery] string? sort = null,
        [FromQuery] int page = 1)
    {
        ViewModel.SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        ViewModel.Sort = sort == "name" ? "name" : "wins";
        ViewModel.CurrentPage = Math.Max(1, page);

        var query = context.People.Where(p => p.Managers.Any());

        if (ViewModel.SearchQuery != null)
        {
            var term = ViewModel.SearchQuery.ToLower();
            query = query.Where(p =>
                (p.NameFirst != null && p.NameFirst.ToLower().Contains(term)) ||
                (p.NameLast != null && p.NameLast.ToLower().Contains(term)));
        }

        ViewModel.TotalManagers = await query.CountAsync();

        if (ViewModel.TotalPages > 0 && ViewModel.CurrentPage > ViewModel.TotalPages)
        {
            ViewModel.CurrentPage = ViewModel.TotalPages;
        }

        var summaries = query.Select(p => new ManagerSummary
        {
            PlayerId = p.PlayerId,
            FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
            FirstYear = p.Managers.Min(m => m.YearId),
            LastYear = p.Managers.Max(m => m.YearId),
            Seasons = p.Managers.Select(m => m.YearId).Distinct().Count(),
            Games = p.Managers.Sum(m => (int)(m.G ?? 0)),
            Wins = p.Managers.Sum(m => (int)(m.W ?? 0)),
            Losses = p.Managers.Sum(m => (int)(m.L ?? 0))
        });

        summaries = ViewModel.Sort == "name"
            ? summaries.OrderBy(s => s.FullName)
            : summaries.OrderByDescending(s => s.Wins);

        ViewModel.Managers = await summaries
            .Skip((ViewModel.CurrentPage - 1) * ViewModel.PageSize)
            .Take(ViewModel.PageSize)
            .ToListAsync();

        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        foreach (var manager in ViewModel.Managers)
        {
            manager.IsInHallOfFame = hofPlayerIds.Contains(manager.PlayerId);
        }

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_ManagersContent", ViewModel);
        }

        return Page();
    }
}
