using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class TeamReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService) : ITeamReadService
{
    public async Task<TeamSeasonReadModel?> GetTeamSeasonAsync(
        string teamId,
        string league,
        int year,
        CancellationToken cancellationToken = default)
    {
        var normalizedTeamId = McpInputValidation.NormalizeRequiredCode(teamId, "teamId");
        var normalizedLeague = McpInputValidation.NormalizeRequiredCode(league, "league");
        var normalizedYear = McpInputValidation.ValidateYear(year);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var team = await context.Teams
            .Where(t => t.TeamId == normalizedTeamId && t.LgId == normalizedLeague && t.YearId == normalizedYear)
            .Select(t => new
            {
                t.TeamId,
                t.Name,
                t.YearId,
                t.LgId,
                t.DivId,
                t.FranchId,
                FranchiseName = t.Franchise != null ? t.Franchise.FranchName : null,
                Wins = t.W ?? 0,
                Losses = t.L ?? 0,
                t.Rank,
                WonDivision = t.DivWin == "Y",
                WonWildCard = t.Wcwin == "Y",
                WonPennant = t.LgWin == "Y",
                WonWorldSeries = t.Wswin == "Y",
                t.Park,
                t.Attendance,
                t.R,
                t.Ab,
                t.H,
                t._2b,
                t._3b,
                t.Hr,
                t.Bb,
                t.So,
                t.Sb,
                t.Ra,
                t.Er,
                t.Era,
                t.Cg,
                t.Sho,
                t.Sv,
                t.Ha,
                t.Hra,
                t.Bba,
                t.Soa
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (team is null)
        {
            return null;
        }

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var batters = await context.Batting
            .Where(b => b.TeamId == normalizedTeamId && b.LgId == normalizedLeague && b.YearId == normalizedYear)
            .OrderByDescending(b => b.Ab ?? 0)
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

        var batterResults = batters
            .Select(b => new TeamBatterReadModel(
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
            .Where(p => p.TeamId == normalizedTeamId && p.LgId == normalizedLeague && p.YearId == normalizedYear)
            .OrderByDescending(p => p.Ipouts ?? 0)
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
                EarnedRuns = p.Er ?? 0,
                InningsPitchedOuts = p.Ipouts ?? 0
            })
            .ToListAsync(cancellationToken);

        var pitcherResults = pitchers
            .Select(p => new TeamPitcherReadModel(
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
            .Where(m => m.TeamId == normalizedTeamId && m.LgId == normalizedLeague && m.YearId == normalizedYear)
            .OrderBy(m => m.Inseason)
            .ThenBy(m => m.PlayerId)
            .Select(m => new TeamManagerReadModel(
                m.PlayerId,
                FormatName(m.Player.NameFirst, m.Player.NameLast, m.PlayerId),
                m.G ?? 0,
                m.W ?? 0,
                m.L ?? 0,
                m.Inseason,
                hallOfFamers.Contains(m.PlayerId)))
            .ToListAsync(cancellationToken);

        var totalGames = team.Wins + team.Losses;
        int.TryParse(team.Attendance, out var attendance);

        TeamBattingSummaryReadModel? batting = null;
        if (int.TryParse(team.R, out var runs) && int.TryParse(team.Ab, out var atBats) && int.TryParse(team.H, out var hits))
        {
            batting = new TeamBattingSummaryReadModel(
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

        TeamPitchingSummaryReadModel? pitching = null;
        if (int.TryParse(team.Ra, out var runsAllowed) && int.TryParse(team.Er, out var earnedRuns))
        {
            pitching = new TeamPitchingSummaryReadModel(
                runsAllowed,
                earnedRuns,
                double.TryParse(team.Era, out var era) ? era : 0,
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
            team.Name,
            team.YearId,
            team.LgId,
            team.DivId,
            team.FranchId,
            team.FranchiseName,
            team.Wins,
            (short)team.Losses,
            totalGames > 0 ? Math.Round((double)team.Wins / totalGames, 3) : 0,
            team.Rank,
            team.WonDivision,
            team.WonWildCard,
            team.WonPennant,
            team.WonWorldSeries,
            team.Park,
            attendance > 0 ? attendance : null,
            batting,
            pitching,
            batterResults,
            pitcherResults,
            managers);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
