using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context) : PageModel
{
    public TeamListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? league)
    {
        ViewModel.SelectedLeague = league;

        // Aggregate team stats in the database instead of loading all 2700+ team records
        var summaries = await context.TeamsFranchises
            .Where(f => f.Teams.Any())
            .Select(f => new FranchiseSummary
            {
                FranchiseId = f.FranchId,
                FranchiseName = f.FranchName ?? f.FranchId,
                IsActive = f.Active == "Y",
                FirstYear = f.Teams.Min(t => t.YearId),
                LastYear = f.Teams.Max(t => t.YearId),
                TotalSeasons = f.Teams.Count(),
                TotalWins = f.Teams.Sum(t => (int)(t.W ?? 0)),
                TotalLosses = f.Teams.Sum(t => (int)(t.L ?? 0)),
                WorldSeriesWins = f.Teams.Count(t => t.Wswin == "Y"),
                PennantWins = f.Teams.Count(t => t.LgWin == "Y"),
                CurrentTeamId = f.Teams.OrderByDescending(t => t.YearId).First().TeamId,
                CurrentLeague = f.Teams.OrderByDescending(t => t.YearId).First().LgId,
                CurrentDivision = f.Teams.OrderByDescending(t => t.YearId).First().DivId
            })
            .ToListAsync();

        foreach (var summary in summaries)
        {
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