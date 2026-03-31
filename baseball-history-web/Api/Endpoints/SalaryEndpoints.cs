using baseball_history_web.Api.Dtos;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Api.Endpoints;

public static class SalaryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/players/{playerId}", GetPlayerSalaries).WithSummary("Player salary history");
        group.MapGet("/teams/{teamId}/{year:int}", GetTeamSalaries).WithSummary("Team payroll for a season");
        group.MapGet("/leaders", GetSalaryLeaders).WithSummary("Highest-paid players by year");
    }

    private static async Task<IResult> GetPlayerSalaries(string playerId, BaseballDbContext context)
    {
        var person = await context.People.FirstOrDefaultAsync(p => p.PlayerId == playerId);
        if (person == null) return Results.NotFound();

        var seasons = await context.Salaries
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.YearId)
            .Select(s => new SalarySeasonDto(s.YearId, s.TeamId, s.LgId, s.Salary))
            .ToListAsync();

        var careerTotal = seasons.Sum(s => s.Salary ?? 0);

        return Results.Ok(new PlayerSalaryHistoryDto(
            playerId, $"{person.NameFirst} {person.NameLast}".Trim(),
            seasons, careerTotal));
    }

    private static async Task<IResult> GetTeamSalaries(string teamId, int year, BaseballDbContext context)
    {
        var salaries = await context.Salaries
            .Where(s => s.TeamId == teamId && s.YearId == year)
            .OrderByDescending(s => s.Salary)
            .Select(s => new SalaryDto(
                s.YearId, s.TeamId, s.LgId, s.PlayerId,
                (s.Player.NameFirst ?? "") + " " + (s.Player.NameLast ?? ""),
                s.Salary))
            .ToListAsync();

        if (salaries.Count == 0) return Results.NotFound();

        var totalPayroll = salaries.Sum(s => s.Salary ?? 0);
        return Results.Ok(new TeamSalaryDto(
            (short)year, teamId, totalPayroll, salaries.Count, salaries));
    }

    private static async Task<IResult> GetSalaryLeaders(
        BaseballDbContext context, int? year = null, int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = context.Salaries.AsQueryable();
        if (year.HasValue) query = query.Where(s => s.YearId == year.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var data = await query
            .OrderByDescending(s => s.Salary)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SalaryDto(
                s.YearId, s.TeamId, s.LgId, s.PlayerId,
                (s.Player.NameFirst ?? "") + " " + (s.Player.NameLast ?? ""),
                s.Salary))
            .ToListAsync();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }
}
