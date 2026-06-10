using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class TeamSeasonReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService) : ITeamSeasonReadService
{
    public async Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        string teamId,
        string league,
        int year,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var team = await context.Teams
            .Include(t => t.Franchise)
            .FirstOrDefaultAsync(
                t => t.TeamId == teamId && t.LgId == league && t.YearId == year,
                cancellationToken);

        if (team is null)
        {
            return null;
        }

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var batters = await context.Batting
            .Where(b => b.TeamId == teamId && b.LgId == league && b.YearId == year)
            .OrderByDescending(b => b.Ab ?? 0)
            .ThenBy(b => b.Player.NameLast)
            .ThenBy(b => b.Player.NameFirst)
            .ThenBy(b => b.PlayerId)
            .Select(b => new
            {
                b.PlayerId,
                b.Player.NameFirst,
                b.Player.NameLast,
                Games = b.G ?? 0,
                AtBats = b.Ab ?? 0,
                Hits = b.H ?? 0,
                HomeRuns = b.Hr ?? 0,
                Rbi = b.Rbi ?? 0
            })
            .ToListAsync(cancellationToken);

        var batterModels = batters.Select(b => new TeamSeasonBatterReadModel(
            b.PlayerId,
            FormatName(b.NameFirst, b.NameLast, b.PlayerId),
            b.Games,
            b.AtBats,
            b.Hits,
            b.HomeRuns,
            b.Rbi,
            b.AtBats > 0 ? Math.Round((double)b.Hits / b.AtBats, 3) : 0,
            hallOfFamers.Contains(b.PlayerId)))
            .ToList();

        var pitchers = await context.Pitching
            .Where(p => p.TeamId == teamId && p.LgId == league && p.YearId == year)
            .OrderByDescending(p => p.Ipouts ?? 0)
            .ThenBy(p => p.Player.NameLast)
            .ThenBy(p => p.Player.NameFirst)
            .ThenBy(p => p.PlayerId)
            .Select(p => new
            {
                p.PlayerId,
                p.Player.NameFirst,
                p.Player.NameLast,
                Games = p.G ?? 0,
                Wins = p.W ?? 0,
                Losses = p.L ?? 0,
                Saves = p.Sv ?? 0,
                Strikeouts = p.So ?? 0,
                InningsPitchedOuts = p.Ipouts ?? 0,
                EarnedRuns = p.Er ?? 0
            })
            .ToListAsync(cancellationToken);

        var pitcherModels = pitchers.Select(p => new TeamSeasonPitcherReadModel(
            p.PlayerId,
            FormatName(p.NameFirst, p.NameLast, p.PlayerId),
            p.Games,
            p.Wins,
            p.Losses,
            p.Saves,
            p.Strikeouts,
            p.InningsPitchedOuts > 0 ? Math.Round((p.EarnedRuns * 27.0) / p.InningsPitchedOuts, 2) : 0,
            hallOfFamers.Contains(p.PlayerId)))
            .ToList();

        var managers = await context.Managers
            .Where(m => m.TeamId == teamId && m.LgId == league && m.YearId == year)
            .OrderBy(m => m.Inseason)
            .ThenBy(m => m.Player.NameLast)
            .ThenBy(m => m.Player.NameFirst)
            .ThenBy(m => m.PlayerId)
            .Select(m => new
            {
                m.PlayerId,
                m.Player.NameFirst,
                m.Player.NameLast,
                Games = m.G ?? 0,
                Wins = m.W ?? 0,
                Losses = m.L ?? 0,
                Order = m.Inseason
            })
            .ToListAsync(cancellationToken);

        var managerModels = managers.Select(m => new TeamSeasonManagerReadModel(
            m.PlayerId,
            FormatName(m.NameFirst, m.NameLast, m.PlayerId),
            m.Games,
            m.Wins,
            m.Losses,
            m.Order,
            hallOfFamers.Contains(m.PlayerId)))
            .ToList();

        var wins = team.W ?? 0;
        var losses = team.L ?? 0;
        var games = wins + losses;
        int.TryParse(team.Attendance, out var attendance);

        TeamSeasonBattingReadModel? batting = null;
        if (int.TryParse(team.R, out var runs) && int.TryParse(team.Ab, out var atBats) && int.TryParse(team.H, out var hits))
        {
            batting = new TeamSeasonBattingReadModel(
                runs,
                atBats,
                hits,
                int.TryParse(team._2b, out var doubles) ? doubles : 0,
                int.TryParse(team._3b, out var triples) ? triples : 0,
                int.TryParse(team.Hr, out var homeRuns) ? homeRuns : 0,
                int.TryParse(team.Bb, out var walks) ? walks : 0,
                int.TryParse(team.So, out var strikeouts) ? strikeouts : 0,
                int.TryParse(team.Sb, out var stolenBases) ? stolenBases : 0,
                atBats > 0 ? Math.Round((double)hits / atBats, 3) : 0);
        }

        TeamSeasonPitchingReadModel? pitching = null;
        if (int.TryParse(team.Ra, out var runsAllowed) && int.TryParse(team.Er, out var earnedRuns))
        {
            pitching = new TeamSeasonPitchingReadModel(
                runsAllowed,
                earnedRuns,
                double.TryParse(team.Era, out var era) ? Math.Round(era, 2) : 0,
                int.TryParse(team.Cg, out var completeGames) ? completeGames : 0,
                int.TryParse(team.Sho, out var shutouts) ? shutouts : 0,
                int.TryParse(team.Sv, out var saves) ? saves : 0,
                int.TryParse(team.Ha, out var hitsAllowed) ? hitsAllowed : 0,
                int.TryParse(team.Hra, out var homeRunsAllowed) ? homeRunsAllowed : 0,
                int.TryParse(team.Bba, out var walksAllowed) ? walksAllowed : 0,
                int.TryParse(team.Soa, out var strikeoutsThrown) ? strikeoutsThrown : 0);
        }

        return new TeamSeasonReadModel(
            team.TeamId,
            team.Name ?? team.TeamId,
            team.YearId,
            team.LgId,
            team.DivId,
            team.FranchId,
            team.Franchise?.FranchName,
            wins,
            (short)losses,
            games > 0 ? Math.Round((double)wins / games, 3) : 0,
            team.Rank,
            team.DivWin == "Y",
            team.Wcwin == "Y",
            team.LgWin == "Y",
            team.Wswin == "Y",
            team.Park,
            attendance > 0 ? attendance : null,
            batting,
            pitching,
            batterModels,
            pitcherModels,
            managerModels);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
