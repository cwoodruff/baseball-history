using baseball_history_mcp.Configuration;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Querying;

public sealed class ParkReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IOptions<BaseballMcpOptions> options) : IParkReadService
{
    public async Task<PagedReadResult<ParkSummaryReadModel>> SearchParksAsync(
        ParkSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        McpInputValidation.ValidatePage(request.Page);
        McpInputValidation.ValidatePageSize(request.PageSize);
        var query = McpInputValidation.NormalizeOptionalText(request.Query);
        var state = McpInputValidation.NormalizeOptionalText(request.State)?.ToUpperInvariant();

        var maxPageSize = options.Value.Limits.ParkSearchPageSizeMax;
        var pageSize = Math.Clamp(request.PageSize, 1, maxPageSize);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var parksQuery = context.Parks.Where(p => p.Parkkey != null && p.Parkkey != "");

        if (state != null)
        {
            parksQuery = parksQuery.Where(p => p.State == state);
        }

        if (query != null)
        {
            var pattern = $"%{query}%";
            parksQuery = parksQuery.Where(p =>
                (p.Parkname != null && EF.Functions.ILike(p.Parkname, pattern)) ||
                (p.Parkalias != null && EF.Functions.ILike(p.Parkalias, pattern)) ||
                (p.City != null && EF.Functions.ILike(p.City, pattern)));
        }

        var orderedQuery = parksQuery
            .OrderBy(p => p.Parkname)
            .ThenBy(p => p.Parkkey);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        var page = Math.Clamp(request.Page, 1, totalPages);

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ParkSummaryReadModel(
                p.Parkkey!,
                p.Parkname,
                p.Parkalias,
                p.City,
                p.State,
                p.Country,
                p.HomeGames.Min(h => (short?)h.Yearkey),
                p.HomeGames.Max(h => (short?)h.Yearkey)))
            .ToListAsync(cancellationToken);

        return new PagedReadResult<ParkSummaryReadModel>(
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

    public async Task<ParkReadModel?> GetParkAsync(
        string parkKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedParkKey = McpInputValidation.NormalizeRequiredCode(parkKey, "parkKey");
        var maxSeasonRows = options.Value.Limits.ParkSeasonRowsMax;

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var park = await context.Parks
            .Where(p => p.Parkkey == normalizedParkKey)
            .Select(p => new { p.Parkkey, p.Parkname, p.Parkalias, p.City, p.State, p.Country })
            .FirstOrDefaultAsync(cancellationToken);

        if (park is null)
        {
            return null;
        }

        var seasonsQuery = context.HomeGames
            .Where(h => h.Parkkey == normalizedParkKey)
            .OrderByDescending(h => h.Yearkey)
            .ThenBy(h => h.Teamkey);

        var totalSeasonRows = await seasonsQuery.CountAsync(cancellationToken);

        var allSeasons = await context.HomeGames
            .Where(h => h.Parkkey == normalizedParkKey)
            .Select(h => new
            {
                h.Yearkey,
                h.Teamkey,
                h.Leaguekey,
                TeamName = h.Team.Name,
                h.Games,
                h.Openings,
                h.Attendance
            })
            .ToListAsync(cancellationToken);

        var tenants = allSeasons
            .GroupBy(s => (s.Teamkey, s.Leaguekey))
            .Select(g => new ParkTenantReadModel(
                g.Key.Teamkey,
                g.Key.Leaguekey,
                g.OrderByDescending(s => s.Yearkey).First().TeamName,
                g.Min(s => s.Yearkey),
                g.Max(s => s.Yearkey),
                g.Select(s => s.Yearkey).Distinct().Count(),
                g.Sum(s => s.Games ?? 0)))
            .OrderByDescending(t => t.LastYear)
            .ThenBy(t => t.TeamId)
            .ToList();

        var seasons = allSeasons
            .OrderByDescending(s => s.Yearkey)
            .ThenBy(s => s.Teamkey)
            .Take(maxSeasonRows)
            .Select(s => new ParkSeasonReadModel(
                s.Yearkey,
                s.Teamkey,
                s.Leaguekey,
                s.TeamName,
                s.Games,
                s.Openings,
                s.Attendance))
            .ToList();

        var aliases = string.IsNullOrWhiteSpace(park.Parkalias)
            ? Array.Empty<string>()
            : park.Parkalias.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ParkReadModel(
            park.Parkkey!,
            park.Parkname,
            aliases,
            park.City,
            park.State,
            park.Country,
            allSeasons.Count > 0 ? allSeasons.Min(s => s.Yearkey) : null,
            allSeasons.Count > 0 ? allSeasons.Max(s => s.Yearkey) : null,
            allSeasons.Sum(s => s.Games ?? 0),
            allSeasons.Sum(s => (long)(s.Attendance ?? 0)),
            totalSeasonRows,
            maxSeasonRows,
            totalSeasonRows > maxSeasonRows,
            tenants,
            seasons);
    }
}
