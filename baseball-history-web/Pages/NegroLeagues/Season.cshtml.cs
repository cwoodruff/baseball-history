using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.NegroLeagues;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class SeasonModel(BaseballDbContext context, ILeaderboardQueryService leaderboardService) : PageModel
{
    private const int LeaderCount = 5;

    public NegroLeagueSeasonViewModel ViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string lgId, short year)
    {
        var info = Services.NegroLeagues.Find(lgId);
        if (info == null)
        {
            return NotFound();
        }

        var standings = await context.Teams
            .Where(t => t.LgId == info.Id && t.YearId == year)
            .Select(t => new NegroLeagueStandingsRow
            {
                TeamId = t.TeamId,
                Name = t.Name ?? t.TeamId,
                Rank = t.Rank,
                Wins = t.W ?? 0,
                Losses = t.L ?? 0,
                Games = t.G,
                WonPennant = t.LgWin == "Y"
            })
            .ToListAsync();

        if (standings.Count == 0)
        {
            return NotFound();
        }

        standings = standings
            .OrderBy(s => s.Rank ?? byte.MaxValue)
            .ThenByDescending(s => s.WinningPercentage)
            .ThenByDescending(s => s.Wins)
            .ToList();
        NegroLeagueStandingsRow.ComputeGamesBehind(standings);

        var availableYears = await context.Teams
            .Where(t => t.LgId == info.Id)
            .Select(t => t.YearId)
            .Distinct()
            .OrderBy(y => y)
            .ToListAsync();

        ViewModel = new NegroLeagueSeasonViewModel
        {
            Info = info,
            Year = year,
            Standings = standings,
            AvailableYears = availableYears,
            BattingAverageLeaders = await GetBattingLeadersAsync("avg", year, info.Id, qualified: true,
                r => (r.AVG ?? 0).ToString(".000").TrimStart('0')),
            HomeRunLeaders = await GetBattingLeadersAsync("hr", year, info.Id, qualified: false,
                r => r.HR.ToString()),
            EraLeaders = await GetPitchingLeadersAsync("era", year, info.Id, qualified: true,
                r => (r.ERA ?? 0).ToString("0.00")),
            WinLeaders = await GetPitchingLeadersAsync("w", year, info.Id, qualified: false,
                r => r.W.ToString())
        };

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_LeagueSeason", ViewModel);
        }

        return Page();
    }

    private async Task<List<LeaderLine>> GetBattingLeadersAsync(string stat, short year, string league,
        bool qualified, Func<BattingLeaderRow, string> formatValue)
    {
        var result = await leaderboardService.GetBattingLeadersAsync(new LeaderboardRequest(
            stat, FromYear: year, ToYear: year, League: league, SingleSeason: true,
            Qualified: qualified, PageSize: LeaderCount));

        return result.Rows.Select(r => new LeaderLine
        {
            Rank = r.Rank,
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            TeamName = r.TeamName,
            Value = formatValue(r),
            IsInHallOfFame = r.IsHallOfFamer
        }).ToList();
    }

    private async Task<List<LeaderLine>> GetPitchingLeadersAsync(string stat, short year, string league,
        bool qualified, Func<PitchingLeaderRow, string> formatValue)
    {
        var result = await leaderboardService.GetPitchingLeadersAsync(new LeaderboardRequest(
            stat, FromYear: year, ToYear: year, League: league, SingleSeason: true,
            Qualified: qualified, PageSize: LeaderCount));

        return result.Rows.Select(r => new LeaderLine
        {
            Rank = r.Rank,
            PlayerId = r.PlayerId,
            PlayerName = r.PlayerName,
            TeamName = r.TeamName,
            Value = formatValue(r),
            IsInHallOfFame = r.IsHallOfFamer
        }).ToList();
    }
}
