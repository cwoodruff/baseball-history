using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class SalaryReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    BaseballMcpRequestPolicy requestPolicy) : ISalaryReadService
{
    public async Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(
        string playerId,
        int? itemCount = null,
        CancellationToken cancellationToken = default)
    {
        playerId = requestPolicy.NormalizeRequiredId(playerId, "playerId");
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .FirstOrDefaultAsync(p => p.PlayerId == playerId, cancellationToken);

        if (person is null)
        {
            return null;
        }

        var seasons = await context.Salaries
            .Where(s => s.PlayerId == playerId)
            .OrderByDescending(s => s.YearId)
            .ThenBy(s => s.TeamId)
            .Select(s => new SalarySeasonReadModel(s.YearId, s.TeamId, s.LgId, s.Salary))
            .ToListAsync(cancellationToken);

        if (seasons.Count == 0)
        {
            return null;
        }

        var itemWindow = requestPolicy.CreateSalaryHistoryWindow(itemCount);

        return new PlayerSalaryHistoryReadModel(
            playerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            seasons.Take(itemWindow.ItemCount).ToList(),
            seasons.Sum(s => s.Salary ?? 0))
        {
            RequestedItemCount = itemWindow.RequestedItemCount,
            MaxItemCount = itemWindow.MaxItemCount,
            WasItemCountClamped = itemWindow.WasItemCountClamped
        };
    }

    public async Task<TeamPayrollReadModel?> GetTeamPayrollAsync(
        string teamId,
        int year,
        int? itemCount = null,
        CancellationToken cancellationToken = default)
    {
        teamId = requestPolicy.NormalizeRequiredId(teamId, "teamId");
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var players = await context.Salaries
            .Where(s => s.TeamId == teamId && s.YearId == year)
            .OrderByDescending(s => s.Salary)
            .ThenBy(s => s.Player.NameLast)
            .ThenBy(s => s.Player.NameFirst)
            .ThenBy(s => s.PlayerId)
            .Select(s => new SalaryEntryReadModel(
                s.YearId,
                s.TeamId,
                s.LgId,
                s.PlayerId,
                FormatName(s.Player.NameFirst, s.Player.NameLast, s.PlayerId),
                s.Salary))
            .ToListAsync(cancellationToken);

        if (players.Count == 0)
        {
            return null;
        }

        var itemWindow = requestPolicy.CreateTeamPayrollWindow(itemCount);

        return new TeamPayrollReadModel(
            (short)year,
            teamId,
            players.Sum(p => p.Salary ?? 0),
            players.Count,
            players.Take(itemWindow.ItemCount).ToList())
        {
            RequestedItemCount = itemWindow.RequestedItemCount,
            MaxItemCount = itemWindow.MaxItemCount,
            WasItemCountClamped = itemWindow.WasItemCountClamped
        };
    }

    public async Task<PagedReadResult<SalaryEntryReadModel>> GetSalaryLeadersAsync(
        SalaryLeaderQuery request,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Salaries.AsQueryable();
        if (request.Year.HasValue)
        {
            query = query.Where(s => s.YearId == request.Year.Value);
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var pageWindow = requestPolicy.CreateLeaderboardWindow(request.Page, request.PageSize, totalCount);

        var items = await query
            .OrderByDescending(s => s.Salary)
            .ThenByDescending(s => s.YearId)
            .ThenBy(s => s.Player.NameLast)
            .ThenBy(s => s.Player.NameFirst)
            .ThenBy(s => s.PlayerId)
            .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
            .Take(pageWindow.PageSize)
            .Select(s => new SalaryEntryReadModel(
                s.YearId,
                s.TeamId,
                s.LgId,
                s.PlayerId,
                FormatName(s.Player.NameFirst, s.Player.NameLast, s.PlayerId),
                s.Salary))
            .ToListAsync(cancellationToken);

        return pageWindow.CreateResult(items, totalCount);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
