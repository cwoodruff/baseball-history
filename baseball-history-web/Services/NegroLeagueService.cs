using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using BaseballHistory.Data.Reference;

namespace baseball_history_web.Services;

/// <summary>
/// Negro Leagues hub, league and season queries, shared by the /NegroLeagues
/// pages and the /api/negro-leagues endpoints.
/// </summary>
public class NegroLeagueService(BaseballDbContext context, ILeaderboardQueryService leaderboardService)
{
    private const int LeaderCount = 5;

    /// <summary>The seven recognized leagues with team-season and club counts</summary>
    public async Task<List<NegroLeagueCard>> GetLeagueCardsAsync()
    {
        var leagueIds = NegroLeagues.All.Select(l => l.Id).ToList();

        var counts = await context.Teams
            .Where(t => leagueIds.Contains(t.LgId))
            .GroupBy(t => t.LgId)
            .Select(g => new
            {
                LgId = g.Key,
                TeamSeasons = g.Count(),
                Clubs = g.Select(t => t.TeamId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.LgId);

        return NegroLeagues.All
            .Select(info => new NegroLeagueCard
            {
                Info = info,
                TeamSeasons = counts.TryGetValue(info.Id, out var c) ? c.TeamSeasons : 0,
                ClubCount = counts.TryGetValue(info.Id, out var c2) ? c2.Clubs : 0
            })
            .ToList();
    }

    /// <summary>A league's seasons and clubs, or null for an unrecognized league ID</summary>
    public async Task<NegroLeagueDetailViewModel?> GetLeagueAsync(string? lgId)
    {
        var info = NegroLeagues.Find(lgId);
        if (info == null)
        {
            return null;
        }

        var seasons = await context.Teams
            .Where(t => t.LgId == info.Id)
            .GroupBy(t => t.YearId)
            .Select(g => new NegroLeagueSeasonSummary
            {
                Year = g.Key,
                TeamCount = g.Count(),
                ChampionTeamId = g.Where(t => t.LgWin == "Y").Select(t => t.TeamId).FirstOrDefault(),
                ChampionName = g.Where(t => t.LgWin == "Y").Select(t => t.Name).FirstOrDefault()
            })
            .OrderByDescending(s => s.Year)
            .ToListAsync();

        var clubs = await context.Teams
            .Where(t => t.LgId == info.Id)
            .GroupBy(t => t.TeamId)
            .Select(g => new NegroLeagueClub
            {
                TeamId = g.Key,
                Name = g.OrderByDescending(t => t.YearId).Select(t => t.Name).First() ?? g.Key,
                FirstYear = g.Min(t => t.YearId),
                LastYear = g.Max(t => t.YearId),
                Seasons = g.Count(),
                Wins = g.Sum(t => (int)(t.W ?? 0)),
                Losses = g.Sum(t => (int)(t.L ?? 0)),
                Pennants = g.Count(t => t.LgWin == "Y")
            })
            .ToListAsync();

        return new NegroLeagueDetailViewModel
        {
            Info = info,
            Seasons = seasons,
            Clubs = clubs
                .OrderByDescending(c => c.Pennants)
                .ThenByDescending(c => c.Seasons)
                .ThenBy(c => c.Name)
                .ToList()
        };
    }

    /// <summary>Standings and leaders for one league season, or null when the league or season doesn't exist</summary>
    public async Task<NegroLeagueSeasonViewModel?> GetSeasonAsync(string? lgId, short year)
    {
        var info = NegroLeagues.Find(lgId);
        if (info == null)
        {
            return null;
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
            return null;
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

        return new NegroLeagueSeasonViewModel
        {
            Info = info,
            Year = year,
            Standings = standings,
            AvailableYears = availableYears,
            BattingAverageLeaders = await GetBattingLeadersAsync("avg", year, info.Id, qualified: true,
                r => r.AVG ?? 0, v => v.ToString(".000").TrimStart('0')),
            HomeRunLeaders = await GetBattingLeadersAsync("hr", year, info.Id, qualified: false,
                r => r.HR, v => v.ToString("0")),
            EraLeaders = await GetPitchingLeadersAsync("era", year, info.Id, qualified: true,
                r => r.ERA ?? 0, v => v.ToString("0.00")),
            WinLeaders = await GetPitchingLeadersAsync("w", year, info.Id, qualified: false,
                r => r.W, v => v.ToString("0"))
        };
    }

    private async Task<List<LeaderLine>> GetBattingLeadersAsync(string stat, short year, string league,
        bool qualified, Func<BattingLeaderRow, decimal> selectValue, Func<decimal, string> formatValue)
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
            RawValue = selectValue(r),
            Value = formatValue(selectValue(r)),
            IsInHallOfFame = r.IsHallOfFamer
        }).ToList();
    }

    private async Task<List<LeaderLine>> GetPitchingLeadersAsync(string stat, short year, string league,
        bool qualified, Func<PitchingLeaderRow, decimal> selectValue, Func<decimal, string> formatValue)
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
            RawValue = selectValue(r),
            Value = formatValue(selectValue(r)),
            IsInHallOfFame = r.IsHallOfFamer
        }).ToList();
    }
}
