using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using BaseballHistory.Data.Reference;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

/// <summary>
/// The seven Negro Leagues MLB recognizes as major leagues (1920-1948). Season
/// leaders come from the shared leaderboard query layer, so this service is
/// scoped like LeaderboardReadService.
/// </summary>
public sealed class NegroLeagueReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    ILeaderboardQueryService leaderboardQueryService) : INegroLeagueReadService
{
    private const int LeaderCount = 5;

    public async Task<IReadOnlyList<NegroLeagueSummaryReadModel>> ListLeaguesAsync(
        CancellationToken cancellationToken = default)
    {
        var leagueIds = NegroLeagues.All.Select(l => l.Id).ToList();

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var counts = await context.Teams
            .Where(t => leagueIds.Contains(t.LgId))
            .GroupBy(t => t.LgId)
            .Select(g => new
            {
                LgId = g.Key,
                TeamSeasons = g.Count(),
                Clubs = g.Select(t => t.TeamId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.LgId, cancellationToken);

        return NegroLeagues.All
            .Select(info => new NegroLeagueSummaryReadModel(
                info.Id,
                info.Name,
                info.FirstYear,
                info.LastYear,
                info.Summary,
                counts.TryGetValue(info.Id, out var c) ? c.TeamSeasons : 0,
                counts.TryGetValue(info.Id, out var c2) ? c2.Clubs : 0))
            .ToList();
    }

    public async Task<NegroLeagueReadModel?> GetLeagueAsync(string leagueId, CancellationToken cancellationToken = default)
    {
        var info = FindLeague(leagueId);
        if (info is null)
        {
            return null;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var seasons = (await context.Teams
                .Where(t => t.LgId == info.Id)
                .GroupBy(t => t.YearId)
                .Select(g => new
                {
                    Year = g.Key,
                    TeamCount = g.Count(),
                    ChampionTeamId = g.Where(t => t.LgWin == "Y").Select(t => t.TeamId).FirstOrDefault(),
                    ChampionName = g.Where(t => t.LgWin == "Y").Select(t => t.Name).FirstOrDefault()
                })
                .OrderByDescending(s => s.Year)
                .ToListAsync(cancellationToken))
            .Select(s => new NegroLeagueSeasonSummaryReadModel(s.Year, s.TeamCount, s.ChampionTeamId, s.ChampionName))
            .ToList();

        var clubs = await context.Teams
            .Where(t => t.LgId == info.Id)
            .GroupBy(t => t.TeamId)
            .Select(g => new
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
            .ToListAsync(cancellationToken);

        return new NegroLeagueReadModel(
            info.Id,
            info.Name,
            info.FirstYear,
            info.LastYear,
            info.Summary,
            seasons,
            clubs
                .OrderByDescending(c => c.Pennants)
                .ThenByDescending(c => c.Seasons)
                .ThenBy(c => c.Name)
                .Select(c => new NegroLeagueClubReadModel(
                    c.TeamId, c.Name, c.FirstYear, c.LastYear, c.Seasons, c.Wins, c.Losses,
                    WinningPercentage(c.Wins, c.Losses), c.Pennants))
                .ToList());
    }

    public async Task<NegroLeagueSeasonReadModel?> GetLeagueSeasonAsync(
        string leagueId,
        int year,
        CancellationToken cancellationToken = default)
    {
        var info = FindLeague(leagueId);
        var seasonYear = McpInputValidation.ValidateYear(year);
        if (info is null)
        {
            return null;
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var teams = await context.Teams
            .Where(t => t.LgId == info.Id && t.YearId == seasonYear)
            .Select(t => new
            {
                t.TeamId,
                Name = t.Name ?? t.TeamId,
                t.Rank,
                Wins = (int)(t.W ?? 0),
                Losses = (int)(t.L ?? 0),
                Games = (int?)t.G,
                WonPennant = t.LgWin == "Y"
            })
            .ToListAsync(cancellationToken);

        if (teams.Count == 0)
        {
            return null;
        }

        // Games behind the team leading by winning percentage
        var leader = teams
            .OrderByDescending(t => RawWinningPercentage(t.Wins, t.Losses))
            .ThenByDescending(t => t.Wins)
            .First();

        var standings = teams
            .OrderBy(t => t.Rank ?? byte.MaxValue)
            .ThenByDescending(t => RawWinningPercentage(t.Wins, t.Losses))
            .ThenByDescending(t => t.Wins)
            .Select(t => new NegroLeagueStandingsReadModel(
                t.TeamId, t.Name, t.Rank, t.Wins, t.Losses, t.Games,
                WinningPercentage(t.Wins, t.Losses),
                ((leader.Wins - t.Wins) + (t.Losses - leader.Losses)) / 2.0,
                t.WonPennant))
            .ToList();

        var availableYears = await context.Teams
            .Where(t => t.LgId == info.Id)
            .Select(t => t.YearId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new NegroLeagueSeasonReadModel(
            info.Id,
            info.Name,
            seasonYear,
            availableYears.Where(y => y < seasonYear).Select(y => (short?)y).Max(),
            availableYears.Where(y => y > seasonYear).Select(y => (short?)y).Min(),
            standings,
            await GetBattingLeadersAsync("avg", seasonYear, info.Id, qualified: true, r => r.AVG ?? 0),
            await GetBattingLeadersAsync("hr", seasonYear, info.Id, qualified: false, r => r.HR),
            await GetPitchingLeadersAsync("era", seasonYear, info.Id, qualified: true, r => r.ERA ?? 0),
            await GetPitchingLeadersAsync("w", seasonYear, info.Id, qualified: false, r => r.W));
    }

    private static NegroLeagueInfo? FindLeague(string leagueId) =>
        NegroLeagues.Find(McpInputValidation.NormalizeRequiredCode(leagueId, "leagueId"));

    private async Task<IReadOnlyList<NegroLeagueLeaderReadModel>> GetBattingLeadersAsync(
        string stat, short year, string league, bool qualified, Func<BattingLeaderRow, decimal> selectValue)
    {
        var result = await leaderboardQueryService.GetBattingLeadersAsync(new LeaderboardRequest(
            stat, FromYear: year, ToYear: year, League: league, SingleSeason: true,
            Qualified: qualified, PageSize: LeaderCount));

        return result.Rows
            .Select(r => new NegroLeagueLeaderReadModel(
                r.Rank, r.PlayerId, r.PlayerName, r.TeamName, (double)selectValue(r), r.IsHallOfFamer))
            .ToList();
    }

    private async Task<IReadOnlyList<NegroLeagueLeaderReadModel>> GetPitchingLeadersAsync(
        string stat, short year, string league, bool qualified, Func<PitchingLeaderRow, decimal> selectValue)
    {
        var result = await leaderboardQueryService.GetPitchingLeadersAsync(new LeaderboardRequest(
            stat, FromYear: year, ToYear: year, League: league, SingleSeason: true,
            Qualified: qualified, PageSize: LeaderCount));

        return result.Rows
            .Select(r => new NegroLeagueLeaderReadModel(
                r.Rank, r.PlayerId, r.PlayerName, r.TeamName, (double)selectValue(r), r.IsHallOfFamer))
            .ToList();
    }

    private static double RawWinningPercentage(int wins, int losses) =>
        wins + losses > 0 ? (double)wins / (wins + losses) : 0;

    private static double WinningPercentage(int wins, int losses) =>
        Math.Round(RawWinningPercentage(wins, losses), 3);
}
