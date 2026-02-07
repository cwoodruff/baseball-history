using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Players;

public class IndexModel : PageModel
{
    private readonly BaseballDbContext _context;
    private const int PageSize = 48;

    public IndexModel(BaseballDbContext context)
    {
        _context = context;
    }

    public PlayerListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? letter, int page = 1)
    {
        // Get all available first letters
        var availableLetters = await _context.People
            .Where(p => p.NameLast != null && p.NameLast.Length > 0)
            .Select(p => p.NameLast!.Substring(0, 1).ToUpper())
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();

        ViewModel.AvailableLetters = availableLetters
            .Where(l => l.Length == 1 && char.IsLetter(l[0]))
            .Select(l => l[0])
            .ToList();

        // Default to 'A' if no letter specified
        var currentLetter = letter?.ToUpper().FirstOrDefault() ?? 'A';
        ViewModel.CurrentLetter = currentLetter.ToString();

        // Get Hall of Fame player IDs for highlighting
        var hofPlayerIds = await _context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSetAsync();

        // Query players by last name starting letter
        var query = _context.People
            .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith(currentLetter.ToString()))
            .OrderBy(p => p.NameLast)
            .ThenBy(p => p.NameFirst);

        // Get total count for pagination
        ViewModel.TotalPlayers = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalPlayers / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));
        ViewModel.PageSize = PageSize;

        // Get players for current page with batting stats
        var players = await query
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(p => new
            {
                Person = p,
                TotalGames = p.Battings.Sum(b => (int?)b.G) ?? 0 + p.Pitchings.Sum(pi => (int?)pi.G) ?? 0,
                TotalHits = p.Battings.Sum(b => (int?)b.H) ?? 0,
                TotalHR = p.Battings.Sum(b => (int?)b.Hr) ?? 0,
                LastTeam = p.Battings.OrderByDescending(b => b.YearId).Select(b => b.TeamId).FirstOrDefault()
                           ?? p.Pitchings.OrderByDescending(pi => pi.YearId).Select(pi => pi.TeamId).FirstOrDefault()
            })
            .ToListAsync();

        ViewModel.Players = players.Select(p => new PlayerSummary
        {
            PlayerId = p.Person.PlayerId,
            FirstName = p.Person.NameFirst,
            LastName = p.Person.NameLast,
            FullName = $"{p.Person.NameFirst} {p.Person.NameLast}".Trim(),
            BirthYear = p.Person.BirthYear,
            DebutYear = p.Person.Debut?.Year.ToString(),
            FinalYear = p.Person.FinalGame?.Year.ToString(),
            IsInHallOfFame = hofPlayerIds.Contains(p.Person.PlayerId),
            TotalGames = p.TotalGames > 0 ? p.TotalGames : null,
            TotalHits = p.TotalHits > 0 ? p.TotalHits : null,
            TotalHomeRuns = p.TotalHR > 0 ? p.TotalHR : null,
            LastTeamId = p.LastTeam
        }).ToList();

        // Return partial view for HTMX requests
        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_PlayerList", ViewModel);
        }

        return Page();
    }
}