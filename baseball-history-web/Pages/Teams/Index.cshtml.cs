using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

public class IndexModel : PageModel
{
    private readonly BaseballDbContext _context;

    public IndexModel(BaseballDbContext context)
    {
        _context = context;
    }

    public TeamListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? league)
    {
        ViewModel.SelectedLeague = league;

        // Get all franchises with their teams
        var franchises = await _context.TeamsFranchises
            .Include(f => f.Teams)
            .ToListAsync();

        foreach (var franchise in franchises)
        {
            if (!franchise.Teams.Any()) continue;

            var summary = FranchiseSummary.FromFranchise(franchise, franchise.Teams);

            // Apply league filter if specified
            if (!string.IsNullOrEmpty(league) && summary.CurrentLeague != league)
                continue;

            if (summary.IsActive)
                ViewModel.ActiveFranchises.Add(summary);
            else
                ViewModel.InactiveFranchises.Add(summary);
        }

        // Sort by franchise name
        ViewModel.ActiveFranchises = ViewModel.ActiveFranchises.OrderBy(f => f.FranchiseName).ToList();
        ViewModel.InactiveFranchises = ViewModel.InactiveFranchises.OrderBy(f => f.FranchiseName).ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_TeamList", ViewModel);
        }

        return Page();
    }
}
