using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context) : PageModel
{
    public TeamListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? league, [FromQuery] string? q = null,
        [FromQuery] int? era = null)
    {
        ViewModel.SelectedLeague = league;
        ViewModel.SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        ViewModel.Era = era is >= 1870 and <= 2030 ? era - era % 10 : null;

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

            // Name filter
            if (ViewModel.SearchQuery != null &&
                !summary.FranchiseName.Contains(ViewModel.SearchQuery, StringComparison.OrdinalIgnoreCase))
                continue;

            // Era filter: franchise fielded a team at some point during the decade
            if (ViewModel.Era.HasValue &&
                !(summary.FirstYear <= ViewModel.Era.Value + 9 && summary.LastYear >= ViewModel.Era.Value))
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
            return Partial("_TeamsContent", ViewModel);
        }

        return Page();
    }
}