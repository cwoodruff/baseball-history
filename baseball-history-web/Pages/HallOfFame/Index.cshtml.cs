using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.HallOfFame;

public class IndexModel(BaseballDbContext context) : PageModel
{
    private const int PageSize = 50;

    public HallOfFameViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? year = null, string? category = null, int page = 1)
    {
        ViewModel.SelectedYear = year;
        ViewModel.SelectedCategory = category;
        ViewModel.CurrentPage = page;

        // Get available years
        ViewModel.AvailableYears = await context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => (int)h.Yearid)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        // Build query
        var query = context.HallOfFame
            .Include(h => h.Player)
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

        // Get inductees
        var inductees = await query
            .OrderByDescending(h => h.Yearid)
            .ThenBy(h => h.Player.NameLast)
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(h => new
            {
                h.PlayerId,
                h.Player,
                h.Yearid,
                h.Category,
                h.VotedBy,
                h.Votes,
                h.Ballots
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
                FirstName = h.Player?.NameFirst,
                LastName = h.Player?.NameLast,
                FullName = $"{h.Player?.NameFirst} {h.Player?.NameLast}".Trim(),
                InductionYear = h.Yearid,
                Category = h.Category ?? "Player",
                VotedBy = h.VotedBy,
                VotePercentage = votePct,
                DebutYear = h.Player?.Debut?.Year.ToString(),
                FinalYear = h.Player?.FinalGame?.Year.ToString()
            };
        }).ToList();

        // Get category counts
        var categoryCounts = await context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .GroupBy(h => h.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewModel.TotalPlayers = categoryCounts.FirstOrDefault(c => c.Category == "Player")?.Count ?? 0;
        ViewModel.TotalManagers = categoryCounts.FirstOrDefault(c => c.Category == "Manager")?.Count ?? 0;
        ViewModel.TotalPioneers = categoryCounts.FirstOrDefault(c => c.Category == "Pioneer/Executive")?.Count ?? 0;
        ViewModel.TotalUmpires = categoryCounts.FirstOrDefault(c => c.Category == "Umpire")?.Count ?? 0;

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_InducteeList", ViewModel);
        }

        return Page();
    }
}