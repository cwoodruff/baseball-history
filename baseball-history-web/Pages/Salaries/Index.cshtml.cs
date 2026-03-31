using baseball_history_web.Extensions;
using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Salaries;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 50;

    public SalaryViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int? year = null, string? team = null, [FromQuery] int page = 1)
    {
        ViewModel.SelectedYear = year;
        ViewModel.SelectedTeam = team;
        ViewModel.CurrentPage = page;

        // Get available years (cached)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("salary_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.Salaries
                .Select(s => (int)s.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Get available teams for the selected year (or all teams)
        var teamQuery = context.Salaries.AsQueryable();
        if (year.HasValue) teamQuery = teamQuery.Where(s => s.YearId == year.Value);

        ViewModel.AvailableTeams = await teamQuery
            .Select(s => new { s.TeamId, s.Team.Name })
            .Distinct()
            .OrderBy(t => t.Name)
            .Select(t => new TeamOption { TeamId = t.TeamId, TeamName = t.Name })
            .ToListAsync();

        // Get Hall of Fame player IDs (cached)
        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        // Build query
        var query = context.Salaries.AsQueryable();
        if (year.HasValue) query = query.Where(s => s.YearId == year.Value);
        if (!string.IsNullOrEmpty(team)) query = query.Where(s => s.TeamId == team);

        ViewModel.TotalEntries = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalEntries / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

        // Team payroll summary
        if (year.HasValue && !string.IsNullOrEmpty(team))
        {
            ViewModel.TeamPayroll = await query.SumAsync(s => s.Salary ?? 0);
            ViewModel.TeamName = await context.Teams
                .Where(t => t.TeamId == team && t.YearId == year.Value)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }

        var salaryData = await query
            .OrderByDescending(s => s.Salary)
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(s => new
            {
                s.YearId,
                s.PlayerId,
                PlayerName = (s.Player.NameFirst ?? "") + " " + (s.Player.NameLast ?? ""),
                s.TeamId,
                TeamName = s.Team.Name,
                s.LgId,
                s.Salary
            })
            .ToListAsync();

        ViewModel.Salaries = salaryData.Select(s => new SalaryEntry
        {
            Year = s.YearId,
            PlayerId = s.PlayerId,
            PlayerName = s.PlayerName,
            TeamId = s.TeamId,
            TeamName = s.TeamName,
            LgId = s.LgId,
            Salary = s.Salary ?? 0,
            IsInHallOfFame = hofPlayerIds.Contains(s.PlayerId)
        }).ToList();

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_SalaryList", ViewModel);
        }

        return Page();
    }
}
