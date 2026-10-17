using System.Linq.Expressions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Services;

/// <summary>
/// Manager browser and career queries, shared by the /Managers pages and the
/// /api/managers endpoints.
/// </summary>
public class ManagerQueryService(BaseballDbContext context, IMemoryCache cache)
{
    /// <summary>One managerial stint; also used by the player Managing tab</summary>
    public static readonly Expression<Func<BaseballHistory.Data.Models.Managers, ManagerSeasonRow>> SeasonRowProjection =
        m => new ManagerSeasonRow
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
        };

    /// <summary>
    /// Fills the list view model's Managers and TotalManagers for its
    /// SearchQuery, Sort, CurrentPage and PageSize; CurrentPage is clamped
    /// to the last page.
    /// </summary>
    public async Task LoadManagerListAsync(ManagerListViewModel viewModel)
    {
        var query = context.People.Where(p => p.Managers.Any());

        if (viewModel.SearchQuery != null)
        {
            var term = viewModel.SearchQuery.ToLower();
            query = query.Where(p =>
                (p.NameFirst != null && p.NameFirst.ToLower().Contains(term)) ||
                (p.NameLast != null && p.NameLast.ToLower().Contains(term)));
        }

        viewModel.TotalManagers = await query.CountAsync();

        if (viewModel.TotalPages > 0 && viewModel.CurrentPage > viewModel.TotalPages)
        {
            viewModel.CurrentPage = viewModel.TotalPages;
        }

        var summaries = query.Select(p => new ManagerSummary
        {
            PlayerId = p.PlayerId,
            FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
            FirstYear = p.Managers.Min(m => m.YearId),
            LastYear = p.Managers.Max(m => m.YearId),
            Seasons = p.Managers.Select(m => m.YearId).Distinct().Count(),
            Games = p.Managers.Sum(m => (int)(m.G ?? 0)),
            Wins = p.Managers.Sum(m => (int)(m.W ?? 0)),
            Losses = p.Managers.Sum(m => (int)(m.L ?? 0))
        });

        summaries = viewModel.Sort == "name"
            ? summaries.OrderBy(s => s.FullName)
            : summaries.OrderByDescending(s => s.Wins);

        viewModel.Managers = await summaries
            .Skip((viewModel.CurrentPage - 1) * viewModel.PageSize)
            .Take(viewModel.PageSize)
            .ToListAsync();

        var hofPlayerIds = await HallOfFameIds.GetAsync(context, cache);
        foreach (var manager in viewModel.Managers)
        {
            manager.IsInHallOfFame = hofPlayerIds.Contains(manager.PlayerId);
        }
    }

    /// <summary>Full managerial career, or null when the person never managed</summary>
    public async Task<ManagerDetailViewModel?> GetManagerDetailAsync(string playerId)
    {
        var seasons = await GetManagerialSeasonsAsync(playerId);
        if (seasons.Count == 0)
        {
            return null;
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

        return new ManagerDetailViewModel
        {
            PlayerId = playerId,
            FullName = person?.FullName.Trim() ?? playerId,
            IsInHallOfFame = isInHallOfFame,
            WasPlayer = person?.WasPlayer ?? false,
            Seasons = seasons,
            Halves = halves,
            Awards = awards
        };
    }

    /// <summary>Managerial stints, most recent first; empty when the person never managed</summary>
    public Task<List<ManagerSeasonRow>> GetManagerialSeasonsAsync(string playerId) =>
        context.Managers
            .Where(m => m.PlayerId == playerId)
            .OrderByDescending(m => m.YearId)
            .ThenBy(m => m.Inseason)
            .Select(SeasonRowProjection)
            .ToListAsync();
}
