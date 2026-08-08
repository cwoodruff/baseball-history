using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Awards;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const int PageSize = 50;

    public AwardVotingViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string? award = null, int? year = null, string? league = null, [FromQuery] int page = 1)
    {
        ViewModel.SelectedAward = award;
        ViewModel.SelectedYear = year;
        ViewModel.SelectedLeague = league;
        ViewModel.CurrentPage = page;

        // Get available awards (cached with namespace prefix)
        ViewModel.AvailableAwards = (await cache.GetOrCreateAsync("awards_names", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.AwardsPlayers
                .Select(a => a.AwardId)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }))!;

        // Get available years (cached with namespace prefix)
        ViewModel.AvailableYears = (await cache.GetOrCreateAsync("awards_years", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.AwardsPlayers
                .Select(a => a.YearId)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }))!;

        // Get available leagues (cached with namespace prefix)
        ViewModel.AvailableLeagues = (await cache.GetOrCreateAsync("awards_leagues", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.AwardsPlayers
                .Select(a => a.LgId)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }))!;

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

        // Check which award/year/league combos have voting data
        var votingKeys = new HashSet<string>();
        if (!string.IsNullOrEmpty(award) || year.HasValue)
        {
            var votingQuery = context.AwardsSharePlayers.AsQueryable();
            if (!string.IsNullOrEmpty(award)) votingQuery = votingQuery.Where(a => a.AwardId == award);
            if (year.HasValue) votingQuery = votingQuery.Where(a => a.YearId == year.Value);
            if (!string.IsNullOrEmpty(league)) votingQuery = votingQuery.Where(a => a.LgId == league);

            votingKeys = (await votingQuery
                .Select(a => a.AwardId + "|" + a.YearId + "|" + a.LgId)
                .Distinct()
                .ToListAsync())
                .ToHashSet();
        }

        // Build winners query
        var query = context.AwardsPlayers.AsQueryable();
        if (!string.IsNullOrEmpty(award)) query = query.Where(a => a.AwardId == award);
        if (year.HasValue) query = query.Where(a => a.YearId == year.Value);
        if (!string.IsNullOrEmpty(league)) query = query.Where(a => a.LgId == league);

        ViewModel.TotalEntries = await query.CountAsync();
        ViewModel.TotalPages = (int)Math.Ceiling((double)ViewModel.TotalEntries / PageSize);
        ViewModel.CurrentPage = Math.Clamp(page, 1, Math.Max(1, ViewModel.TotalPages));

        var winnersData = await query
            .OrderByDescending(a => a.YearId)
            .ThenBy(a => a.AwardId)
            .ThenBy(a => a.LgId)
            .Skip((ViewModel.CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .Select(a => new
            {
                a.PlayerId,
                PlayerName = (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                a.AwardId,
                a.YearId,
                a.LgId,
                a.Notes
            })
            .ToListAsync();

        ViewModel.Winners = winnersData.Select(a => new AwardWinnerEntry
        {
            PlayerId = a.PlayerId,
            PlayerName = a.PlayerName,
            AwardId = a.AwardId,
            Year = a.YearId,
            LgId = a.LgId,
            Notes = a.Notes,
            IsInHallOfFame = hofPlayerIds.Contains(a.PlayerId),
            HasVotingData = votingKeys.Contains($"{a.AwardId}|{a.YearId}|{a.LgId}")
        }).ToList();

        // If a specific award+year+league is selected and has voting data, load the race
        if (!string.IsNullOrEmpty(award) && year.HasValue && !string.IsNullOrEmpty(league))
        {
            var votes = await context.AwardsSharePlayers
                .Where(a => a.AwardId == award && a.YearId == year.Value && a.LgId == league)
                .OrderByDescending(a => a.PointsWon)
                .Select(a => new
                {
                    a.PlayerId,
                    PlayerName = (a.Player.NameFirst ?? "") + " " + (a.Player.NameLast ?? ""),
                    a.PointsWon,
                    a.PointsMax,
                    a.VotesFirst
                })
                .ToListAsync();

            if (votes.Count > 0)
            {
                // Determine who won
                var winnerIds = winnersData
                    .Where(w => w.AwardId == award && w.YearId == year.Value && w.LgId == league)
                    .Select(w => w.PlayerId)
                    .ToHashSet();

                ViewModel.VotingDetail = new AwardRaceDetail
                {
                    AwardId = award,
                    Year = year.Value,
                    LgId = league,
                    Votes = votes.Select(v => new AwardVoteEntry
                    {
                        PlayerId = v.PlayerId,
                        PlayerName = v.PlayerName,
                        PointsWon = v.PointsWon ?? 0,
                        PointsMax = v.PointsMax ?? 0,
                        VotesFirst = v.VotesFirst,
                        VoteShare = (v.PointsMax ?? 0) > 0
                            ? Math.Round((double)(v.PointsWon ?? 0) / (v.PointsMax ?? 1) * 100, 1) : 0,
                        IsInHallOfFame = hofPlayerIds.Contains(v.PlayerId),
                        IsWinner = winnerIds.Contains(v.PlayerId)
                    }).ToList()
                };
            }
        }

        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_AwardsList", ViewModel);
        }

        return Page();
    }
}
