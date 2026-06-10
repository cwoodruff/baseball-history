using System.Linq.Expressions;
using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
namespace baseball_history_mcp.Querying;

public sealed class LeaderboardReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService,
    BaseballMcpRequestPolicy requestPolicy) : ILeaderboardReadService
{
    public async Task<PagedReadResult<BattingLeaderboardEntry>> GetBattingLeadersAsync(
        BattingLeaderboardQuery request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var query = context.Batting.AsQueryable();
        if (normalizedRequest.FromYear.HasValue) query = query.Where(b => b.YearId >= normalizedRequest.FromYear.Value);
        if (normalizedRequest.ToYear.HasValue) query = query.Where(b => b.YearId <= normalizedRequest.ToYear.Value);
        if (!string.IsNullOrWhiteSpace(normalizedRequest.League)) query = query.Where(b => b.LgId == normalizedRequest.League);

        if (normalizedRequest.SingleSeason)
        {
            var seasonQuery = query
                .Where(b => b.Ab >= normalizedRequest.MinAtBats)
                .Select(b => new
                {
                    b.PlayerId,
                    PlayerName = (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                    b.YearId,
                    b.TeamId,
                    TeamName = b.Team.Name,
                    Games = b.G ?? 0,
                    AtBats = b.Ab ?? 0,
                    Runs = b.R ?? 0,
                    Hits = b.H ?? 0,
                    Doubles = b._2b ?? 0,
                    Triples = b._3b ?? 0,
                    HomeRuns = b.Hr ?? 0,
                    Rbi = b.Rbi ?? 0,
                    StolenBases = b.Sb ?? 0,
                    Walks = b.Bb ?? 0
                });
            var totalCount = await seasonQuery.CountAsync(cancellationToken);
            var pageWindow = requestPolicy.CreateLeaderboardWindow(normalizedRequest.Page, normalizedRequest.PageSize, totalCount);

            var data = await ApplyBattingOrder(seasonQuery, normalizedRequest.Stat)
                .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
                .Take(pageWindow.PageSize)
                .ToListAsync(cancellationToken);

            return pageWindow.CreateResult(
                data.Select((entry, index) =>
                {
                    var avg = entry.AtBats > 0 ? (double)entry.Hits / entry.AtBats : 0;
                    var obp = (entry.AtBats + entry.Walks) > 0
                        ? (double)(entry.Hits + entry.Walks) / (entry.AtBats + entry.Walks)
                        : 0;
                    var totalBases = entry.Hits + entry.Doubles + (2 * entry.Triples) + (3 * entry.HomeRuns);
                    var slg = entry.AtBats > 0 ? (double)totalBases / entry.AtBats : 0;

                    return new BattingLeaderboardEntry(
                        ((pageWindow.Page - 1) * pageWindow.PageSize) + index + 1,
                        entry.PlayerId,
                        entry.PlayerName.Trim(),
                        entry.YearId,
                        entry.TeamId,
                        entry.TeamName,
                        hallOfFamers.Contains(entry.PlayerId),
                        entry.Games,
                        entry.AtBats,
                        entry.Runs,
                        entry.Hits,
                        entry.Doubles,
                        entry.Triples,
                        entry.HomeRuns,
                        entry.Rbi,
                        entry.StolenBases,
                        entry.Walks,
                        Math.Round(avg, 3),
                        Math.Round(obp, 3),
                        Math.Round(slg, 3),
                        Math.Round(obp + slg, 3));
                }).ToList(),
                totalCount);
        }

        var careerQuery = query
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                Games = g.Sum(b => b.G ?? 0),
                AtBats = g.Sum(b => b.Ab ?? 0),
                Runs = g.Sum(b => b.R ?? 0),
                Hits = g.Sum(b => b.H ?? 0),
                Doubles = g.Sum(b => b._2b ?? 0),
                Triples = g.Sum(b => b._3b ?? 0),
                HomeRuns = g.Sum(b => b.Hr ?? 0),
                Rbi = g.Sum(b => b.Rbi ?? 0),
                StolenBases = g.Sum(b => b.Sb ?? 0),
                Walks = g.Sum(b => b.Bb ?? 0)
            })
            .Where(x => x.AtBats >= normalizedRequest.MinAtBats);

        {
            var totalCount = await careerQuery.CountAsync(cancellationToken);
            var pageWindow = requestPolicy.CreateLeaderboardWindow(normalizedRequest.Page, normalizedRequest.PageSize, totalCount);

            var data = await ApplyBattingOrder(careerQuery, normalizedRequest.Stat)
                .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
                .Take(pageWindow.PageSize)
                .ToListAsync(cancellationToken);

            var playerIds = data.Select(entry => entry.PlayerId).ToList();
            var playerNames = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(
                    p => p.PlayerId,
                    p => string.Join(' ', new[] { p.NameFirst, p.NameLast }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim(),
                    cancellationToken);

            return pageWindow.CreateResult(
                data.Select((entry, index) =>
                {
                    var avg = entry.AtBats > 0 ? (double)entry.Hits / entry.AtBats : 0;
                    var obp = (entry.AtBats + entry.Walks) > 0
                        ? (double)(entry.Hits + entry.Walks) / (entry.AtBats + entry.Walks)
                        : 0;
                    var totalBases = entry.Hits + entry.Doubles + (2 * entry.Triples) + (3 * entry.HomeRuns);
                    var slg = entry.AtBats > 0 ? (double)totalBases / entry.AtBats : 0;

                    return new BattingLeaderboardEntry(
                        ((pageWindow.Page - 1) * pageWindow.PageSize) + index + 1,
                        entry.PlayerId,
                        playerNames.GetValueOrDefault(entry.PlayerId, entry.PlayerId),
                        null,
                        null,
                        null,
                        hallOfFamers.Contains(entry.PlayerId),
                        entry.Games,
                        entry.AtBats,
                        entry.Runs,
                        entry.Hits,
                        entry.Doubles,
                        entry.Triples,
                        entry.HomeRuns,
                        entry.Rbi,
                        entry.StolenBases,
                        entry.Walks,
                        Math.Round(avg, 3),
                        Math.Round(obp, 3),
                        Math.Round(slg, 3),
                        Math.Round(obp + slg, 3));
                }).ToList(),
                totalCount);
        }
    }

    public async Task<PagedReadResult<PitchingLeaderboardEntry>> GetPitchingLeadersAsync(
        PitchingLeaderboardQuery request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);
        var minimumOuts = normalizedRequest.MinInningsPitched * 3;
        var ascending = normalizedRequest.Stat.Equals("era", StringComparison.OrdinalIgnoreCase)
            || normalizedRequest.Stat.Equals("whip", StringComparison.OrdinalIgnoreCase)
            || normalizedRequest.Stat.Equals("bb9", StringComparison.OrdinalIgnoreCase);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var query = context.Pitching.AsQueryable();
        if (normalizedRequest.FromYear.HasValue) query = query.Where(p => p.YearId >= normalizedRequest.FromYear.Value);
        if (normalizedRequest.ToYear.HasValue) query = query.Where(p => p.YearId <= normalizedRequest.ToYear.Value);
        if (!string.IsNullOrWhiteSpace(normalizedRequest.League)) query = query.Where(p => p.LgId == normalizedRequest.League);

        if (normalizedRequest.SingleSeason)
        {
            var seasonQuery = query
                .Where(p => (p.Ipouts ?? 0) >= minimumOuts)
                .Select(p => new
                {
                    p.PlayerId,
                    PlayerName = (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                    p.YearId,
                    p.TeamId,
                    TeamName = p.Team.Name,
                    Games = p.G ?? 0,
                    GamesStarted = p.Gs ?? 0,
                    Wins = p.W ?? 0,
                    Losses = p.L ?? 0,
                    Saves = p.Sv ?? 0,
                    CompleteGames = p.Cg ?? 0,
                    Shutouts = p.Sho ?? 0,
                    InningsPitchedOuts = p.Ipouts ?? 0,
                    Hits = p.H ?? 0,
                    EarnedRuns = p.Er ?? 0,
                    HomeRuns = p.Hr ?? 0,
                    Walks = p.Bb ?? 0,
                    Strikeouts = p.So ?? 0
                });

            var totalCount = await seasonQuery.CountAsync(cancellationToken);
            var pageWindow = requestPolicy.CreateLeaderboardWindow(normalizedRequest.Page, normalizedRequest.PageSize, totalCount);

            var data = await ApplyPitchingOrder(seasonQuery, normalizedRequest.Stat, ascending)
                .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
                .Take(pageWindow.PageSize)
                .ToListAsync(cancellationToken);

            return pageWindow.CreateResult(
                data.Select((entry, index) =>
                {
                    var inningsPitched = entry.InningsPitchedOuts / 3.0;
                    var era = inningsPitched > 0 ? entry.EarnedRuns * 9.0 / inningsPitched : 0;
                    var whip = inningsPitched > 0 ? (entry.Walks + entry.Hits) / inningsPitched : 0;

                    return new PitchingLeaderboardEntry(
                        ((pageWindow.Page - 1) * pageWindow.PageSize) + index + 1,
                        entry.PlayerId,
                        entry.PlayerName.Trim(),
                        entry.YearId,
                        entry.TeamId,
                        entry.TeamName,
                        hallOfFamers.Contains(entry.PlayerId),
                        entry.Games,
                        entry.GamesStarted,
                        entry.Wins,
                        entry.Losses,
                        entry.Saves,
                        entry.CompleteGames,
                        entry.Shutouts,
                        Math.Round(inningsPitched, 1),
                        entry.Hits,
                        entry.EarnedRuns,
                        entry.HomeRuns,
                        entry.Walks,
                        entry.Strikeouts,
                        Math.Round(era, 2),
                        Math.Round(whip, 3));
                }).ToList(),
                totalCount);
        }

        var careerQuery = query
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                Games = g.Sum(p => p.G ?? 0),
                GamesStarted = g.Sum(p => p.Gs ?? 0),
                Wins = g.Sum(p => p.W ?? 0),
                Losses = g.Sum(p => p.L ?? 0),
                Saves = g.Sum(p => p.Sv ?? 0),
                CompleteGames = g.Sum(p => p.Cg ?? 0),
                Shutouts = g.Sum(p => p.Sho ?? 0),
                InningsPitchedOuts = g.Sum(p => p.Ipouts ?? 0),
                Hits = g.Sum(p => p.H ?? 0),
                EarnedRuns = g.Sum(p => p.Er ?? 0),
                HomeRuns = g.Sum(p => p.Hr ?? 0),
                Walks = g.Sum(p => p.Bb ?? 0),
                Strikeouts = g.Sum(p => p.So ?? 0)
            })
            .Where(x => x.InningsPitchedOuts >= minimumOuts);

        {
            var totalCount = await careerQuery.CountAsync(cancellationToken);
            var pageWindow = requestPolicy.CreateLeaderboardWindow(normalizedRequest.Page, normalizedRequest.PageSize, totalCount);

            var data = await ApplyPitchingOrder(careerQuery, normalizedRequest.Stat, ascending)
                .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
                .Take(pageWindow.PageSize)
                .ToListAsync(cancellationToken);

            var playerIds = data.Select(entry => entry.PlayerId).ToList();
            var playerNames = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(
                    p => p.PlayerId,
                    p => string.Join(' ', new[] { p.NameFirst, p.NameLast }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim(),
                    cancellationToken);

            return pageWindow.CreateResult(
                data.Select((entry, index) =>
                {
                    var inningsPitched = entry.InningsPitchedOuts / 3.0;
                    var era = inningsPitched > 0 ? entry.EarnedRuns * 9.0 / inningsPitched : 0;
                    var whip = inningsPitched > 0 ? (entry.Walks + entry.Hits) / inningsPitched : 0;

                    return new PitchingLeaderboardEntry(
                        ((pageWindow.Page - 1) * pageWindow.PageSize) + index + 1,
                        entry.PlayerId,
                        playerNames.GetValueOrDefault(entry.PlayerId, entry.PlayerId),
                        null,
                        null,
                        null,
                        hallOfFamers.Contains(entry.PlayerId),
                        entry.Games,
                        entry.GamesStarted,
                        entry.Wins,
                        entry.Losses,
                        entry.Saves,
                        entry.CompleteGames,
                        entry.Shutouts,
                        Math.Round(inningsPitched, 1),
                        entry.Hits,
                        entry.EarnedRuns,
                        entry.HomeRuns,
                        entry.Walks,
                        entry.Strikeouts,
                        Math.Round(era, 2),
                        Math.Round(whip, 3));
                }).ToList(),
                totalCount);
        }
    }

    private static IOrderedQueryable<T> ApplyBattingOrder<T>(IQueryable<T> query, string stat)
        where T : class
    {
        return stat.ToLowerInvariant() switch
        {
            "hr" or "homeruns" => query.OrderByDescending(DynExpr<T>("HomeRuns")),
            "h" or "hits" => query.OrderByDescending(DynExpr<T>("Hits")),
            "r" or "runs" => query.OrderByDescending(DynExpr<T>("Runs")),
            "rbi" => query.OrderByDescending(DynExpr<T>("Rbi")),
            "sb" or "stolenbases" => query.OrderByDescending(DynExpr<T>("StolenBases")),
            "2b" or "doubles" => query.OrderByDescending(DynExpr<T>("Doubles")),
            "3b" or "triples" => query.OrderByDescending(DynExpr<T>("Triples")),
            "bb" or "walks" => query.OrderByDescending(DynExpr<T>("Walks")),
            "g" or "games" => query.OrderByDescending(DynExpr<T>("Games")),
            "ab" or "atbats" => query.OrderByDescending(DynExpr<T>("AtBats")),
            "avg" or "battingaverage" => query.OrderByDescending(DynComputedExpr<T>("Hits", "AtBats")),
            "obp" => query.OrderByDescending(DynComputedExpr<T>("Hits", "Walks", "AtBats", "Walks")),
            "slg" => query.OrderByDescending(DynSluggingExpr<T>()),
            "ops" => query.OrderByDescending(DynOpsExpr<T>()),
            _ => throw new BaseballMcpUsageException($"Unsupported batting stat '{stat}'.")
        };
    }

    private static IOrderedQueryable<T> ApplyPitchingOrder<T>(IQueryable<T> query, string stat, bool ascending)
        where T : class
    {
        if (ascending)
        {
            return stat.ToLowerInvariant() switch
            {
                "era" => query.OrderBy(DynEraExpr<T>()),
                "whip" => query.OrderBy(DynWhipExpr<T>()),
                "bb9" => query.OrderBy(DynBb9Expr<T>()),
                _ => throw new BaseballMcpUsageException($"Unsupported pitching stat '{stat}'.")
            };
        }

        return stat.ToLowerInvariant() switch
        {
            "w" or "wins" => query.OrderByDescending(DynExpr<T>("Wins")),
            "l" or "losses" => query.OrderByDescending(DynExpr<T>("Losses")),
            "so" or "strikeouts" => query.OrderByDescending(DynExpr<T>("Strikeouts")),
            "sv" or "saves" => query.OrderByDescending(DynExpr<T>("Saves")),
            "cg" => query.OrderByDescending(DynExpr<T>("CompleteGames")),
            "sho" => query.OrderByDescending(DynExpr<T>("Shutouts")),
            "ip" => query.OrderByDescending(DynExpr<T>("InningsPitchedOuts")),
            "g" or "games" => query.OrderByDescending(DynExpr<T>("Games")),
            "gs" => query.OrderByDescending(DynExpr<T>("GamesStarted")),
            "hr" => query.OrderByDescending(DynExpr<T>("HomeRuns")),
            "k9" => query.OrderByDescending(DynK9Expr<T>()),
            "wpct" => query.OrderByDescending(DynWinningPctExpr<T>()),
            _ => throw new BaseballMcpUsageException($"Unsupported pitching stat '{stat}'.")
        };
    }

    private static Expression<Func<T, int>> DynExpr<T>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var converted = Expression.Convert(property, typeof(int));
        return Expression.Lambda<Func<T, int>>(converted, parameter);
    }

    private static Expression<Func<T, double>> DynComputedExpr<T>(string numeratorProperty, string denominatorProperty)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var numerator = Expression.Convert(Expression.Property(parameter, numeratorProperty), typeof(double));
        var denominator = Expression.Convert(Expression.Property(parameter, denominatorProperty), typeof(double));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denominator, zero), zero, Expression.Divide(numerator, denominator));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynComputedExpr<T>(
        string numeratorPropertyOne,
        string numeratorPropertyTwo,
        string denominatorPropertyOne,
        string denominatorPropertyTwo)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var numeratorOne = Expression.Convert(Expression.Property(parameter, numeratorPropertyOne), typeof(double));
        var numeratorTwo = Expression.Convert(Expression.Property(parameter, numeratorPropertyTwo), typeof(double));
        var denominatorOne = Expression.Convert(Expression.Property(parameter, denominatorPropertyOne), typeof(double));
        var denominatorTwo = Expression.Convert(Expression.Property(parameter, denominatorPropertyTwo), typeof(double));
        var numerator = Expression.Add(numeratorOne, numeratorTwo);
        var denominator = Expression.Add(denominatorOne, denominatorTwo);
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denominator, zero), zero, Expression.Divide(numerator, denominator));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynSluggingExpr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var hits = Expression.Convert(Expression.Property(parameter, "Hits"), typeof(double));
        var doubles = Expression.Convert(Expression.Property(parameter, "Doubles"), typeof(double));
        var triples = Expression.Convert(Expression.Property(parameter, "Triples"), typeof(double));
        var homeRuns = Expression.Convert(Expression.Property(parameter, "HomeRuns"), typeof(double));
        var atBats = Expression.Convert(Expression.Property(parameter, "AtBats"), typeof(double));
        var zero = Expression.Constant(0.0);
        var totalBases = Expression.Add(hits, Expression.Add(doubles, Expression.Add(Expression.Multiply(Expression.Constant(2.0), triples), Expression.Multiply(Expression.Constant(3.0), homeRuns))));
        var body = Expression.Condition(Expression.Equal(atBats, zero), zero, Expression.Divide(totalBases, atBats));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynOpsExpr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var obp = Expression.Invoke(DynComputedExpr<T>("Hits", "Walks", "AtBats", "Walks"), parameter);
        var slg = Expression.Invoke(DynSluggingExpr<T>(), parameter);
        return Expression.Lambda<Func<T, double>>(Expression.Add(obp, slg), parameter);
    }

    private static Expression<Func<T, double>> DynEraExpr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var earnedRuns = Expression.Convert(Expression.Property(parameter, "EarnedRuns"), typeof(double));
        var inningsPitchedOuts = Expression.Convert(Expression.Property(parameter, "InningsPitchedOuts"), typeof(double));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(
            Expression.Equal(inningsPitchedOuts, zero),
            Expression.Constant(double.MaxValue),
            Expression.Divide(Expression.Multiply(earnedRuns, Expression.Constant(27.0)), inningsPitchedOuts));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynWhipExpr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var hits = Expression.Convert(Expression.Property(parameter, "Hits"), typeof(double));
        var walks = Expression.Convert(Expression.Property(parameter, "Walks"), typeof(double));
        var inningsPitchedOuts = Expression.Convert(Expression.Property(parameter, "InningsPitchedOuts"), typeof(double));
        var body = Expression.Condition(
            Expression.Equal(inningsPitchedOuts, Expression.Constant(0.0)),
            Expression.Constant(double.MaxValue),
            Expression.Divide(Expression.Multiply(Expression.Add(hits, walks), Expression.Constant(3.0)), inningsPitchedOuts));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynK9Expr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var strikeouts = Expression.Convert(Expression.Property(parameter, "Strikeouts"), typeof(double));
        var inningsPitchedOuts = Expression.Convert(Expression.Property(parameter, "InningsPitchedOuts"), typeof(double));
        var body = Expression.Condition(
            Expression.Equal(inningsPitchedOuts, Expression.Constant(0.0)),
            Expression.Constant(0.0),
            Expression.Divide(Expression.Multiply(strikeouts, Expression.Constant(27.0)), inningsPitchedOuts));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynBb9Expr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var walks = Expression.Convert(Expression.Property(parameter, "Walks"), typeof(double));
        var inningsPitchedOuts = Expression.Convert(Expression.Property(parameter, "InningsPitchedOuts"), typeof(double));
        var body = Expression.Condition(
            Expression.Equal(inningsPitchedOuts, Expression.Constant(0.0)),
            Expression.Constant(double.MaxValue),
            Expression.Divide(Expression.Multiply(walks, Expression.Constant(27.0)), inningsPitchedOuts));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }

    private static Expression<Func<T, double>> DynWinningPctExpr<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var wins = Expression.Convert(Expression.Property(parameter, "Wins"), typeof(double));
        var losses = Expression.Convert(Expression.Property(parameter, "Losses"), typeof(double));
        var decisions = Expression.Add(wins, losses);
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(decisions, zero), zero, Expression.Divide(wins, decisions));
        return Expression.Lambda<Func<T, double>>(body, parameter);
    }
}
