using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Teams;

public class FranchiseModel : PageModel
{
    private readonly BaseballDbContext _context;

    public FranchiseModel(BaseballDbContext context)
    {
        _context = context;
    }

    public FranchiseSummary? Franchise { get; set; }
    public List<TeamSeasonSummary> Seasons { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var franchise = await _context.TeamsFranchises
            .Include(f => f.Teams)
            .FirstOrDefaultAsync(f => f.FranchId == id);

        if (franchise == null)
        {
            return NotFound();
        }

        Franchise = FranchiseSummary.FromFranchise(franchise, franchise.Teams);

        // Get all seasons for this franchise
        Seasons = franchise.Teams
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
            .ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_FranchiseSeasons", this);
        }

        return Page();
    }
}

public class TeamSeasonSummary
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? DivId { get; set; }
    public short Wins { get; set; }
    public short Losses { get; set; }
    public byte? Rank { get; set; }
    public bool WonDivision { get; set; }
    public bool WonPennant { get; set; }
    public bool WonWorldSeries { get; set; }

    public double WinningPercentage => (Wins + Losses) > 0
        ? (double)Wins / (Wins + Losses)
        : 0;

    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
    public string Record => $"{Wins}-{Losses}";
}