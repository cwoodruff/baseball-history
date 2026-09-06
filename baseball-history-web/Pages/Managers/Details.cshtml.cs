using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages.Managers;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class DetailsModel(BaseballDbContext context) : PageModel
{
    public ManagerDetailViewModel Manager { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            return NotFound();
        }

        var seasons = await context.Managers
            .Where(m => m.PlayerId == playerId)
            .OrderByDescending(m => m.YearId)
            .ThenBy(m => m.Inseason)
            .Select(m => new ManagerSeasonRow
            {
                Year = m.YearId,
                TeamId = m.TeamId,
                LgId = m.LgId,
                TeamName = m.Team.Name,
                Inseason = m.Inseason,
                Games = m.G,
                Wins = m.W,
                Losses = m.L,
                Rank = m.Rank,
                IsPlayerManager = m.PlyrMgr == "Y",
                WonPennant = m.Team.LgWin == "Y",
                WonWorldSeries = m.Team.Wswin == "Y"
            })
            .ToListAsync();

        if (seasons.Count == 0)
        {
            return NotFound();
        }

        var person = await context.People
            .Where(p => p.PlayerId == playerId)
            .Select(p => new
            {
                FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
                WasPlayer = p.Battings.Any() || p.Pitchings.Any()
            })
            .FirstOrDefaultAsync();

        var isInHallOfFame = await context.HallOfFame
            .AnyAsync(h => h.PlayerId == playerId && h.Inducted == "Y");

        var halves = await context.ManagersHalf
            .Where(m => m.PlayerId == playerId)
            .OrderByDescending(m => m.YearId)
            .ThenBy(m => m.Half)
            .Select(m => new ManagerHalfRow
            {
                Year = m.YearId,
                TeamId = m.TeamId,
                LgId = m.LgId,
                TeamName = m.Team.Name,
                Half = m.Half,
                Games = m.G,
                Wins = m.W,
                Losses = m.L,
                Rank = m.Rank
            })
            .ToListAsync();

        var awards = await context.AwardsManagers
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.YearId)
            .Select(a => new ManagerAwardRow
            {
                Year = a.YearId,
                AwardId = a.AwardId,
                LgId = a.LgId
            })
            .ToListAsync();

        if (awards.Count > 0)
        {
            var votingKeys = (await context.AwardsShareManagers
                    .Where(v => v.PlayerId == playerId)
                    .Select(v => v.AwardId + "|" + v.YearId + "|" + v.LgId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();

            foreach (var award in awards)
            {
                award.HasVotingData = votingKeys.Contains($"{award.AwardId}|{award.Year}|{award.LgId}");
            }
        }

        Manager = new ManagerDetailViewModel
        {
            PlayerId = playerId,
            FullName = person?.FullName.Trim() ?? playerId,
            IsInHallOfFame = isInHallOfFame,
            WasPlayer = person?.WasPlayer ?? false,
            Seasons = seasons,
            Halves = halves,
            Awards = awards
        };

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_ManagerDetail", Manager);
        }

        return Page();
    }
}
