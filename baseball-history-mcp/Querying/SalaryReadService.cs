using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Querying;

public sealed class SalaryReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IOptions<BaseballMcpOptions> options) : ISalaryReadService
{
    public async Task<PlayerSalaryHistoryReadModel?> GetPlayerSalaryHistoryAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedPlayerId = McpInputValidation.NormalizeRequiredPlayerId(playerId);
        var maxSeasonCount = options.Value.Limits.SalaryHistorySeasonsMax;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .Where(p => p.PlayerId == normalizedPlayerId)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            return null;
        }

        var salaryQuery = context.Salaries
            .Where(s => s.PlayerId == normalizedPlayerId)
            .OrderByDescending(s => s.YearId)
            .ThenBy(s => s.TeamId)
            .ThenBy(s => s.LgId);

        var totalSeasonCount = await salaryQuery.CountAsync(cancellationToken);
        var careerTotal = await context.Salaries
            .Where(s => s.PlayerId == normalizedPlayerId)
            .SumAsync(s => s.Salary ?? 0, cancellationToken);
        var seasons = await salaryQuery
            .Take(maxSeasonCount)
            .Select(s => new SalarySeasonReadModel(s.YearId, s.TeamId, s.LgId, s.Salary))
            .ToListAsync(cancellationToken);

        return new PlayerSalaryHistoryReadModel(
            person.PlayerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            careerTotal,
            totalSeasonCount,
            seasons.Count,
            maxSeasonCount,
            totalSeasonCount > maxSeasonCount,
            seasons);
    }

    public async Task<PagedReadResult<SalaryLeaderEntry>> GetSalaryLeadersAsync(
        SalaryLeaderRequest request,
        CancellationToken cancellationToken = default)
    {
        McpInputValidation.ValidateOptionalYear(request.Year, "year");
        McpInputValidation.ValidatePage(request.Page);
        McpInputValidation.ValidatePageSize(request.PageSize);

        var maxPageSize = options.Value.Limits.SalaryLeaderboardPageSizeMax;
        var pageSize = Math.Clamp(request.PageSize, 1, maxPageSize);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Salaries.AsQueryable();
        if (request.Year.HasValue)
        {
            var normalizedYear = McpInputValidation.ValidateYear(request.Year.Value, "year");
            query = query.Where(s => s.YearId == normalizedYear);
        }

        query = query
            .OrderByDescending(s => s.Salary ?? 0)
            .ThenByDescending(s => s.YearId)
            .ThenBy(s => s.PlayerId)
            .ThenBy(s => s.TeamId)
            .ThenBy(s => s.LgId);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        var page = Math.Clamp(request.Page, 1, totalPages);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SalaryLeaderEntry(
                s.YearId,
                s.TeamId,
                s.LgId,
                s.PlayerId,
                FormatName(s.Player.NameFirst, s.Player.NameLast, s.PlayerId),
                s.Salary))
            .ToListAsync(cancellationToken);

        return new PagedReadResult<SalaryLeaderEntry>(
            items,
            page,
            pageSize,
            totalCount,
            totalPages)
        {
            RequestedPage = request.Page,
            RequestedPageSize = request.PageSize,
            MaxPageSize = maxPageSize,
            WasPageAdjusted = page != request.Page,
            WasPageSizeClamped = pageSize != request.PageSize
        };
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
