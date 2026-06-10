using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class FranchiseReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    BaseballMcpRequestPolicy requestPolicy) : IFranchiseReadService
{
    public async Task<PagedReadResult<FranchiseLookupItem>> ListFranchisesAsync(
        FranchiseLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        McpInputValidation.ValidatePage(request.Page);
        McpInputValidation.ValidatePageSize(request.PageSize);

        var normalizedRequest = requestPolicy.Normalize(request);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.TeamsFranchises
            .Where(f => f.Teams.Any())
            .Select(f => new
            {
                f.FranchId,
                DisplayName = f.FranchName ?? f.FranchId,
                IsActive = f.Active == "Y",
                FirstYear = f.Teams.Min(t => t.YearId),
                LastYear = f.Teams.Max(t => t.YearId),
                TotalSeasons = f.Teams.Count(),
                TotalWins = f.Teams.Sum(t => (int)(t.W ?? 0)),
                TotalLosses = f.Teams.Sum(t => (int)(t.L ?? 0)),
                WorldSeriesWins = f.Teams.Count(t => t.Wswin == "Y"),
                PennantWins = f.Teams.Count(t => t.LgWin == "Y"),
                CurrentTeamId = f.Teams.OrderByDescending(t => t.YearId).Select(t => t.TeamId).FirstOrDefault(),
                CurrentLeague = f.Teams.OrderByDescending(t => t.YearId).Select(t => t.LgId).FirstOrDefault(),
                CurrentDivision = f.Teams.OrderByDescending(t => t.YearId).Select(t => t.DivId).FirstOrDefault()
            });

        if (!string.IsNullOrWhiteSpace(normalizedRequest.League))
        {
            query = query.Where(f => f.CurrentLeague != null && EF.Functions.ILike(f.CurrentLeague, normalizedRequest.League));
        }

        if (normalizedRequest.ActiveOnly)
        {
            query = query.Where(f => f.IsActive);
        }

        query = query
            .OrderBy(f => f.DisplayName)
            .ThenBy(f => f.FranchId);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageWindow = requestPolicy.CreateFranchiseLookupWindow(normalizedRequest, totalCount);

        var rows = await query
            .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
            .Take(pageWindow.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(f =>
            {
                var totalGames = f.TotalWins + f.TotalLosses;
                return new FranchiseLookupItem(
                    f.FranchId,
                    f.DisplayName,
                    f.IsActive,
                    f.FirstYear,
                    f.LastYear,
                    f.TotalSeasons,
                    f.TotalWins,
                    f.TotalLosses,
                    totalGames > 0 ? Math.Round((double)f.TotalWins / totalGames, 3) : 0,
                    f.WorldSeriesWins,
                    f.PennantWins,
                    f.CurrentTeamId,
                    f.CurrentLeague,
                    f.CurrentDivision);
            })
            .ToList();

        return pageWindow.CreateResult(items, totalCount);
    }

    public async Task<FranchiseReadModel?> GetFranchiseAsync(string franchiseId, CancellationToken cancellationToken = default)
    {
        var normalizedFranchiseId = McpInputValidation.NormalizeRequiredCode(franchiseId, "franchiseId");

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var franchise = await context.TeamsFranchises
            .Include(f => f.Teams)
            .FirstOrDefaultAsync(f => f.FranchId == normalizedFranchiseId, cancellationToken);

        if (franchise is null)
        {
            return null;
        }

        var teams = franchise.Teams
            .OrderByDescending(t => t.YearId)
            .ThenBy(t => t.TeamId)
            .ThenBy(t => t.LgId)
            .ToList();

        var totalWins = teams.Sum(t => t.W ?? 0);
        var totalLosses = teams.Sum(t => t.L ?? 0);
        var totalGames = totalWins + totalLosses;

        return new FranchiseReadModel(
            franchise.FranchId,
            franchise.FranchName ?? franchise.FranchId,
            franchise.Active == "Y",
            teams.Min(t => t.YearId),
            teams.Max(t => t.YearId),
            teams.Count,
            totalWins,
            totalLosses,
            totalGames > 0 ? Math.Round((double)totalWins / totalGames, 3) : 0,
            teams.Count(t => t.Wswin == "Y"),
            teams.Count(t => t.LgWin == "Y"),
            teams.Select(t =>
            {
                var wins = t.W ?? 0;
                var losses = t.L ?? 0;
                var games = wins + losses;
                return new FranchiseSeasonReadModel(
                    t.YearId,
                    t.TeamId,
                    t.Name,
                    t.LgId,
                    t.DivId,
                    wins,
                    (short)losses,
                    games > 0 ? Math.Round((double)wins / games, 3) : 0,
                    t.Rank,
                    t.DivWin == "Y",
                    t.LgWin == "Y",
                    t.Wswin == "Y");
            }).ToList());
    }
}
