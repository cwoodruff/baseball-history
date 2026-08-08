using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class FranchiseModel(BaseballDbContext context) : PageModel
{
    public FranchiseDetailViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var franchise = await context.TeamsFranchises
            .Where(f => f.FranchId == id)
            .Select(f => new FranchiseSummary
            {
                FranchiseId = f.FranchId,
                FranchiseName = f.FranchName ?? f.FranchId,
                IsActive = f.Active == "Y",
                FirstYear = f.Teams.Min(t => (short?)t.YearId),
                LastYear = f.Teams.Max(t => (short?)t.YearId),
                TotalSeasons = f.Teams.Count(),
                TotalWins = f.Teams.Sum(t => (int)(t.W ?? 0)),
                TotalLosses = f.Teams.Sum(t => (int)(t.L ?? 0)),
                WorldSeriesWins = f.Teams.Count(t => t.Wswin == "Y"),
                PennantWins = f.Teams.Count(t => t.LgWin == "Y"),
                CurrentTeamId = f.Teams
                    .OrderByDescending(t => t.YearId)
                    .Select(t => t.TeamId)
                    .FirstOrDefault(),
                CurrentLeague = f.Teams
                    .OrderByDescending(t => t.YearId)
                    .Select(t => t.LgId)
                    .FirstOrDefault(),
                CurrentDivision = f.Teams
                    .OrderByDescending(t => t.YearId)
                    .Select(t => t.DivId)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (franchise == null)
        {
            return NotFound();
        }

        var seasons = await context.Teams
            .Where(t => t.FranchId == id)
            .OrderByDescending(t => t.YearId)
            .Select(t => new TeamSeasonSummary
            {
                Year = t.YearId,
                TeamId = t.TeamId,
                TeamName = t.Name ?? t.TeamId,
                LgId = t.LgId,
                DivId = t.DivId,
                Wins = t.W ?? 0,
                Losses = t.L ?? 0,
                Rank = t.Rank,
                WonDivision = t.DivWin == "Y",
                WonPennant = t.LgWin == "Y",
                WonWorldSeries = t.Wswin == "Y"
            })
            .ToListAsync();

        ViewModel = new FranchiseDetailViewModel
        {
            Franchise = franchise,
            Seasons = seasons
        };

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_FranchiseSeasons", ViewModel);
        }

        return Page();
    }
}
