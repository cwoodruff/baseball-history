using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages;

public class SearchModel : PageModel
{
    private readonly BaseballDbContext _context;

    public SearchModel(BaseballDbContext context)
    {
        _context = context;
    }

    public SearchViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Partial("_SearchResults", ViewModel);
        }

        ViewModel.Query = q;
        var searchTerm = q.Trim();

        // Get Hall of Fame player IDs
        var hofPlayerIds = await _context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSetAsync();

        // Search players (by first name, last name, or full name)
        var players = await _context.People
            .Where(p =>
                (p.NameFirst != null && p.NameFirst.Contains(searchTerm)) ||
                (p.NameLast != null && p.NameLast.Contains(searchTerm)) ||
                (p.NameFirst != null && p.NameLast != null &&
                 (p.NameFirst + " " + p.NameLast).Contains(searchTerm)))
            .Take(10)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, p.Debut, p.FinalGame })
            .ToListAsync();

        ViewModel.Players = players.Select(p => new SearchResult
        {
            Id = p.PlayerId,
            Title = $"{p.NameFirst} {p.NameLast}".Trim(),
            Subtitle = p.Debut.HasValue
                ? $"{p.Debut.Value.Year} - {p.FinalGame?.Year.ToString() ?? "Present"}"
                : null,
            Type = SearchResultType.Player,
            Initials = GetInitials(p.NameFirst, p.NameLast),
            IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId)
        }).ToList();

        // Search franchises
        var franchises = await _context.TeamsFranchises
            .Where(f => f.FranchName != null && f.FranchName.Contains(searchTerm))
            .Take(5)
            .ToListAsync();

        // Get latest team ID for each franchise
        var franchiseIds = franchises.Select(f => f.FranchId).ToList();
        var latestTeams = await _context.Teams
            .Where(t => t.FranchId != null && franchiseIds.Contains(t.FranchId))
            .GroupBy(t => t.FranchId)
            .Select(g => new { FranchId = g.Key, TeamId = g.OrderByDescending(t => t.YearId).First().TeamId })
            .ToDictionaryAsync(x => x.FranchId!, x => x.TeamId);

        ViewModel.Teams = franchises.Select(f => new SearchResult
        {
            Id = f.FranchId,
            Title = f.FranchName ?? f.FranchId,
            Subtitle = f.Active == "Y" ? "Active Franchise" : "Historical Franchise",
            Type = SearchResultType.Franchise,
            Initials = GetInitials(f.FranchName, null),
            TeamId = latestTeams.GetValueOrDefault(f.FranchId)
        }).ToList();

        return Partial("_SearchResults", ViewModel);
    }

    private static string GetInitials(string? firstName, string? lastName)
    {
        var initials = "";
        if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
            initials += firstName[0];
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            initials += lastName[0];
        if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(firstName))
            initials = firstName.Length >= 2 ? firstName.Substring(0, 2) : firstName;
        return initials.ToUpper();
    }
}
