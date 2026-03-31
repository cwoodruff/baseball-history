using baseball_history_web.Api.Dtos;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class TeamEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/franchises", GetFranchises).WithSummary("List all franchises");
        group.MapGet("/franchises/{franchiseId}", GetFranchiseDetail).WithSummary("Franchise detail with all seasons");
        group.MapGet("/seasons/{teamId}/{lgId}/{year:int}", GetTeamSeason).WithSummary("Team season detail with roster");
    }

    private static async Task<IResult> GetFranchises(
        BaseballDbContext context, string? league = null, bool? activeOnly = null)
    {
        var summaries = await context.TeamsFranchises
            .Where(f => f.Teams.Any())
            .Select(f => new
            {
                f.FranchId,
                f.FranchName,
                IsActive = f.Active == "Y",
                FirstYear = f.Teams.Min(t => t.YearId),
                LastYear = f.Teams.Max(t => t.YearId),
                TotalSeasons = f.Teams.Count(),
                TotalWins = f.Teams.Sum(t => (int)(t.W ?? 0)),
                TotalLosses = f.Teams.Sum(t => (int)(t.L ?? 0)),
                WorldSeriesWins = f.Teams.Count(t => t.Wswin == "Y"),
                PennantWins = f.Teams.Count(t => t.LgWin == "Y"),
                CurrentTeamId = f.Teams.OrderByDescending(t => t.YearId).First().TeamId,
                CurrentLeague = f.Teams.OrderByDescending(t => t.YearId).First().LgId,
                CurrentDivision = f.Teams.OrderByDescending(t => t.YearId).First().DivId
            })
            .ToListAsync();

        var filtered = summaries.AsEnumerable();
        if (!string.IsNullOrEmpty(league))
            filtered = filtered.Where(f => f.CurrentLeague == league);
        if (activeOnly == true)
            filtered = filtered.Where(f => f.IsActive);

        var data = filtered
            .OrderBy(f => f.FranchName)
            .Select(f =>
            {
                var totalGames = f.TotalWins + f.TotalLosses;
                return new FranchiseListItem(
                    f.FranchId, f.FranchName ?? f.FranchId, f.IsActive,
                    f.FirstYear, f.LastYear, f.TotalSeasons,
                    f.TotalWins, f.TotalLosses,
                    totalGames > 0 ? Math.Round((double)f.TotalWins / totalGames, 3) : 0,
                    f.WorldSeriesWins, f.PennantWins,
                    f.CurrentTeamId, f.CurrentLeague, f.CurrentDivision);
            })
            .ToList();

        return Results.Ok(data);
    }

    private static async Task<IResult> GetFranchiseDetail(string franchiseId, BaseballDbContext context)
    {
        var franchise = await context.TeamsFranchises
            .Include(f => f.Teams)
            .FirstOrDefaultAsync(f => f.FranchId == franchiseId);

        if (franchise == null) return Results.NotFound();

        var teams = franchise.Teams.OrderByDescending(t => t.YearId).ToList();
        var totalWins = teams.Sum(t => t.W ?? 0);
        var totalLosses = teams.Sum(t => t.L ?? 0);
        var totalGames = totalWins + totalLosses;

        var seasons = teams.Select(t =>
        {
            var w = t.W ?? 0;
            var l = t.L ?? 0;
            var g = w + l;
            return new FranchiseSeasonItem(
                t.YearId, t.TeamId, t.Name, t.LgId, t.DivId,
                w, l, g > 0 ? Math.Round((double)w / g, 3) : 0,
                t.Rank,
                t.DivWin == "Y", t.LgWin == "Y", t.Wswin == "Y");
        }).ToList();

        return Results.Ok(new FranchiseDetail(
            franchise.FranchId, franchise.FranchName ?? franchise.FranchId,
            franchise.Active == "Y",
            teams.Min(t => t.YearId), teams.Max(t => t.YearId),
            teams.Count, totalWins, totalLosses,
            totalGames > 0 ? Math.Round((double)totalWins / totalGames, 3) : 0,
            teams.Count(t => t.Wswin == "Y"), teams.Count(t => t.LgWin == "Y"),
            seasons));
    }

    private static async Task<IResult> GetTeamSeason(
        string teamId, string lgId, int year, BaseballDbContext context, IMemoryCache cache)
    {
        var team = await context.Teams
            .Include(t => t.Franchise)
            .FirstOrDefaultAsync(t => t.TeamId == teamId && t.LgId == lgId && t.YearId == year);

        if (team == null) return Results.NotFound();

        var hofPlayerIds = (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;

        var batters = await context.Batting
            .Where(b => b.TeamId == teamId && b.LgId == lgId && b.YearId == year)
            .OrderByDescending(b => b.Ab ?? 0)
            .Select(b => new RosterBatterDto(
                b.PlayerId,
                (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                b.G ?? 0, b.Ab ?? 0, b.H ?? 0, b.Hr ?? 0, b.Rbi ?? 0,
                (b.Ab ?? 0) > 0 ? (double)(b.H ?? 0) / (b.Ab ?? 0) : 0,
                hofPlayerIds.Contains(b.PlayerId)))
            .ToListAsync();

        var pitchers = await context.Pitching
            .Where(p => p.TeamId == teamId && p.LgId == lgId && p.YearId == year)
            .OrderByDescending(p => p.Ipouts ?? 0)
            .Select(p => new RosterPitcherDto(
                p.PlayerId,
                (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                p.G ?? 0, p.W ?? 0, p.L ?? 0, p.Sv ?? 0, p.So ?? 0,
                (p.Ipouts ?? 0) > 0 ? ((p.Er ?? 0) * 27.0) / (p.Ipouts ?? 0) : 0,
                hofPlayerIds.Contains(p.PlayerId)))
            .ToListAsync();

        var managers = await context.Managers
            .Where(m => m.TeamId == teamId && m.LgId == lgId && m.YearId == year)
            .OrderBy(m => m.Inseason)
            .Select(m => new ApiManagerDto(
                m.PlayerId,
                (m.Player.NameFirst ?? "") + " " + (m.Player.NameLast ?? ""),
                m.G ?? 0, m.W ?? 0, m.L ?? 0, m.Inseason,
                hofPlayerIds.Contains(m.PlayerId)))
            .ToListAsync();

        var w = team.W ?? 0;
        var l = team.L ?? 0;
        var g = w + l;
        int.TryParse(team.Attendance, out var attendance);

        ApiTeamBattingDto? batting = null;
        if (int.TryParse(team.R, out var runs) && int.TryParse(team.Ab, out var ab) && int.TryParse(team.H, out var hits))
        {
            batting = new ApiTeamBattingDto(
                runs, ab, hits,
                int.TryParse(team._2b, out var d) ? d : 0,
                int.TryParse(team._3b, out var t3) ? t3 : 0,
                int.TryParse(team.Hr, out var hr) ? hr : 0,
                int.TryParse(team.Bb, out var bb) ? bb : 0,
                int.TryParse(team.So, out var so) ? so : 0,
                int.TryParse(team.Sb, out var sb) ? sb : 0,
                ab > 0 ? Math.Round((double)hits / ab, 3) : 0);
        }

        ApiTeamPitchingDto? pitching = null;
        if (int.TryParse(team.Ra, out var ra) && int.TryParse(team.Er, out var er))
        {
            pitching = new ApiTeamPitchingDto(
                ra, er,
                double.TryParse(team.Era, out var era) ? era : 0,
                int.TryParse(team.Cg, out var cg) ? cg : 0,
                int.TryParse(team.Sho, out var sho) ? sho : 0,
                int.TryParse(team.Sv, out var sv) ? sv : 0,
                int.TryParse(team.Ha, out var ha) ? ha : 0,
                int.TryParse(team.Hra, out var hra) ? hra : 0,
                int.TryParse(team.Bba, out var bba) ? bba : 0,
                int.TryParse(team.Soa, out var soa) ? soa : 0);
        }

        return Results.Ok(new TeamSeasonDetail(
            team.TeamId, team.Name, team.YearId, team.LgId, team.DivId,
            team.FranchId, team.Franchise?.FranchName,
            w, (short)l, g > 0 ? Math.Round((double)w / g, 3) : 0, team.Rank,
            team.DivWin == "Y", team.Wcwin == "Y", team.LgWin == "Y", team.Wswin == "Y",
            team.Park, attendance > 0 ? attendance : null,
            batting, pitching, batters, pitchers, managers));
    }
}
