using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.AllStar;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class YearModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public AllStarYearViewModel ViewModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(short year)
    {
        var rows = await context.AllstarFull
            .Where(a => a.YearId == year)
            .Select(a => new
            {
                a.PlayerId,
                FullName = (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                a.GameId,
                a.GameNum,
                a.TeamId,
                a.LgId,
                a.Gp,
                a.StartingPos
            })
            .ToListAsync();

        if (rows.Count == 0)
        {
            return NotFound();
        }

        // The squad codes (EAS/WES/NOS/SAS) aren't season leagues, so club names
        // and links resolve by teamID alone against that year's Teams rows
        var teamsThisYear = await context.Teams
            .Where(t => t.YearId == year)
            .Select(t => new { t.TeamId, t.LgId, t.Name })
            .ToListAsync();
        var teamByIdAndLeague = teamsThisYear.ToDictionary(t => (t.TeamId, t.LgId), t => t);
        var teamById = teamsThisYear
            .GroupBy(t => t.TeamId)
            .ToDictionary(g => g.Key, g => g.First());

        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        var games = rows
            .GroupBy(r => AllStarGames.GroupKey(r.LgId, r.GameId))
            .Select(g =>
            {
                var leagues = g.Select(r => r.LgId).Distinct().OrderBy(l => l).ToList();
                return new AllStarGameViewModel
                {
                    GroupKey = g.Key,
                    Title = AllStarGames.TitleFor(g.Key, leagues),
                    GameNum = g.Max(r => int.TryParse(r.GameNum, out var num) ? num : 0),
                    GameDate = AllStarGameViewModel.ParseGameDate(
                        g.Select(r => r.GameId).FirstOrDefault(id => !string.IsNullOrEmpty(id))),
                    Rosters = g
                        .GroupBy(r => r.LgId)
                        .OrderBy(lg => lg.Key)
                        .Select(lg => new AllStarRosterGroup
                        {
                            LgId = lg.Key,
                            Players = lg
                                .Select(r =>
                                {
                                    var isSeasonLeague = r.LgId is "AL" or "NL";
                                    var team = isSeasonLeague
                                        ? teamByIdAndLeague.GetValueOrDefault((r.TeamId, r.LgId))
                                        : teamById.GetValueOrDefault(r.TeamId);
                                    return new AllStarRosterRow
                                    {
                                        PlayerId = r.PlayerId,
                                        FullName = r.FullName.Trim(),
                                        TeamId = r.TeamId,
                                        TeamName = team?.Name,
                                        LinkLgId = team?.LgId,
                                        StartingPos = int.TryParse(r.StartingPos, out var pos) ? pos : null,
                                        Played = r.Gp == 1,
                                        IsInHallOfFame = hofPlayerIds.Contains(r.PlayerId)
                                    };
                                })
                                .OrderByDescending(p => p.IsStarter)
                                .ThenBy(p => p.StartingPos ?? int.MaxValue)
                                .ThenBy(p => p.FullName)
                                .ToList()
                        })
                        .ToList()
                };
            })
            .OrderBy(g => AllStarGames.TypeOrder(g.GroupKey))
            .ThenBy(g => g.GameNum)
            .ThenBy(g => g.GameDate)
            .ToList();

        ViewModel = new AllStarYearViewModel
        {
            Year = year,
            Games = games,
            AvailableYears = await context.AllstarFull
                .Select(a => a.YearId)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync()
        };

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_AllStarYear", ViewModel);
        }

        return Page();
    }
}
