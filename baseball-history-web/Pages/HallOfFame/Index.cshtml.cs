using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.HallOfFame;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 50;

    public HallOfFameViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? year = null, string? category = null, [FromQuery] int page = 1)
    {
        ViewModel.SelectedYear = year;
        ViewModel.SelectedCategory = category;
        ViewModel.CurrentPage = page;

        // Get available years (cached with namespace prefix)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("halloffame_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => (int)h.Yearid)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Build query — projection-first (no Include)
        var query = context.HallOfFame
            .Where(h => h.Inducted == "Y");

        // Apply filters
        if (year.HasValue)
            query = query.Where(h => h.Yearid == year.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(h => h.Category == category);

        // Get total count
        ViewModel.TotalInductees = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalInductees / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

        // Get inductees — project only needed fields (no lazy-load entities)
        var inductees = await query
            .OrderByDescending(h => h.Yearid)
            .ThenBy(h => h.Player.NameLast)
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(h => new
            {
                h.PlayerId,
                FirstName = h.Player.NameFirst,
                LastName = h.Player.NameLast,
                h.Yearid,
                h.Category,
                h.VotedBy,
                h.Votes,
                h.Ballots,
                DebutYear = h.Player.Debut,
                FinalYear = h.Player.FinalGame
            })
            .ToListAsync();

        ViewModel.Inductees = inductees.Select(h =>
        {
            double? votePct = null;
            if (int.TryParse(h.Votes, out var votes) && int.TryParse(h.Ballots, out var ballots) && ballots > 0)
            {
                votePct = (double)votes / ballots * 100;
            }

            return new HallOfFameInductee
            {
                PlayerId = h.PlayerId,
                FirstName = h.FirstName,
                LastName = h.LastName,
                FullName = $"{h.FirstName} {h.LastName}".Trim(),
                InductionYear = h.Yearid,
                Category = h.Category ?? "Player",
                VotedBy = h.VotedBy,
                VotePercentage = votePct,
                DebutYear = h.DebutYear?.Year.ToString(),
                FinalYear = h.FinalYear?.Year.ToString()
            };
        }).ToList();

        // Get category counts (cached with namespace prefix)
        var categoryCounts = (await cache.GetOrCreateAsync("halloffame_category_counts", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .GroupBy(h => h.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Category ?? "", g => g.Count);
        }))!;

        ViewModel.TotalPlayers = categoryCounts.GetValueOrDefault("Player");
        ViewModel.TotalManagers = categoryCounts.GetValueOrDefault("Manager");
        ViewModel.TotalPioneers = categoryCounts.GetValueOrDefault("Pioneer/Executive");
        ViewModel.TotalUmpires = categoryCounts.GetValueOrDefault("Umpire");

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_InducteeList", ViewModel);
        }

        return Page();
    }
}