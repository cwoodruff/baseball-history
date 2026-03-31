using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Postseason;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 50;

    public PostseasonViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int? year = null, string? round = null, [FromQuery] int page = 1)
    {
        ViewModel.SelectedYear = year;
        ViewModel.SelectedRound = round;
        ViewModel.CurrentPage = page;

        // Get available years (cached)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("postseason_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.SeriesPost
                .Select(s => (int)s.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Build query
        var query = context.SeriesPost.AsQueryable();

        if (year.HasValue) query = query.Where(s => s.YearId == year.Value);
        if (!string.IsNullOrEmpty(round)) query = query.Where(s => s.Round == round);

        ViewModel.TotalSeries = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalSeries / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

        var seriesData = await query
            .OrderByDescending(s => s.YearId)
            .ThenBy(s => s.Round)
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(s => new
            {
                s.YearId,
                s.Round,
                s.TeamIdwinner,
                WinnerName = s.TeamWinner.Name,
                s.LgIdwinner,
                s.TeamIdloser,
                LoserName = s.TeamLoser != null ? s.TeamLoser.Name : null,
                s.LgIdloser,
                s.Wins,
                s.Losses,
                s.Ties
            })
            .ToListAsync();

        ViewModel.Series = seriesData.Select(s => new PostseasonSeriesEntry
        {
            Year = s.YearId,
            Round = s.Round,
            WinnerTeamId = s.TeamIdwinner,
            WinnerTeamName = s.WinnerName,
            WinnerLgId = s.LgIdwinner,
            LoserTeamId = s.TeamIdloser,
            LoserTeamName = s.LoserName,
            LoserLgId = s.LgIdloser,
            Wins = s.Wins ?? 0,
            Losses = s.Losses ?? 0,
            Ties = s.Ties ?? 0
        }).ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_PostseasonList", ViewModel);
        }

        return Page();
    }
}
