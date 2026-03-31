using System.Linq.Expressions;
using baseball_history_web.Api.Dtos;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class LeaderEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/batting", GetBattingLeaders).WithSummary("Batting leaderboard (career or single-season)");
        group.MapGet("/pitching", GetPitchingLeaders).WithSummary("Pitching leaderboard (career or single-season)");
    }

    private static async Task<IResult> GetBattingLeaders(
        BaseballDbContext context, IMemoryCache cache,
        string stat = "hr", int? fromYear = null, int? toYear = null,
        string? league = null, int minAb = 0, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var hofPlayerIds = await GetHofPlayerIds(context, cache);

        var query = context.Batting.AsQueryable();
        if (fromYear.HasValue) query = query.Where(b => b.YearId >= fromYear.Value);
        if (toYear.HasValue) query = query.Where(b => b.YearId <= toYear.Value);
        if (!string.IsNullOrEmpty(league)) query = query.Where(b => b.LgId == league);

        if (singleSeason)
        {
            var seasonQuery = query
                .Where(b => b.Ab >= minAb)
                .Select(b => new
                {
                    b.PlayerId,
                    PlayerName = (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                    b.YearId,
                    b.TeamId,
                    TeamName = b.Team.Name,
                    G = b.G ?? 0, AB = b.Ab ?? 0, R = b.R ?? 0, H = b.H ?? 0,
                    Doubles = b._2b ?? 0, Triples = b._3b ?? 0, HR = b.Hr ?? 0,
                    RBI = b.Rbi ?? 0, SB = b.Sb ?? 0, BB = b.Bb ?? 0
                });

            var totalCount = await seasonQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var orderedQuery = ApplyBattingOrder(seasonQuery, stat);
            var data = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var leaders = data.Select((b, i) =>
            {
                var avg = b.AB > 0 ? (double)b.H / b.AB : 0;
                var obp = (b.AB + b.BB) > 0 ? (double)(b.H + b.BB) / (b.AB + b.BB) : 0;
                var tb = b.H + b.Doubles + 2 * b.Triples + 3 * b.HR;
                var slg = b.AB > 0 ? (double)tb / b.AB : 0;
                return new BattingLeaderDto(
                    (page - 1) * pageSize + i + 1, b.PlayerId, b.PlayerName, b.YearId,
                    b.TeamId, b.TeamName, hofPlayerIds.Contains(b.PlayerId),
                    b.G, b.AB, b.R, b.H, b.Doubles, b.Triples, b.HR, b.RBI, b.SB, b.BB,
                    Math.Round(avg, 3), Math.Round(obp, 3), Math.Round(slg, 3), Math.Round(obp + slg, 3));
            }).ToList();

            return Results.Ok(PagedResponse.Create(leaders, page, pageSize, totalCount));
        }
        else
        {
            var careerQuery = query
                .GroupBy(b => b.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    G = g.Sum(b => b.G ?? 0), AB = g.Sum(b => b.Ab ?? 0), R = g.Sum(b => b.R ?? 0),
                    H = g.Sum(b => b.H ?? 0), Doubles = g.Sum(b => b._2b ?? 0), Triples = g.Sum(b => b._3b ?? 0),
                    HR = g.Sum(b => b.Hr ?? 0), RBI = g.Sum(b => b.Rbi ?? 0),
                    SB = g.Sum(b => b.Sb ?? 0), BB = g.Sum(b => b.Bb ?? 0)
                })
                .Where(x => x.AB >= minAb);

            var totalCount = await careerQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var orderedQuery = ApplyBattingOrder(careerQuery, stat);
            var data = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var playerIds = data.Select(c => c.PlayerId).ToList();
            var players = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => (p.NameFirst ?? "") + " " + (p.NameLast ?? ""));

            var leaders = data.Select((c, i) =>
            {
                var avg = c.AB > 0 ? (double)c.H / c.AB : 0;
                var obp = (c.AB + c.BB) > 0 ? (double)(c.H + c.BB) / (c.AB + c.BB) : 0;
                var tb = c.H + c.Doubles + 2 * c.Triples + 3 * c.HR;
                var slg = c.AB > 0 ? (double)tb / c.AB : 0;
                return new BattingLeaderDto(
                    (page - 1) * pageSize + i + 1, c.PlayerId,
                    players.GetValueOrDefault(c.PlayerId, c.PlayerId), null,
                    null, null, hofPlayerIds.Contains(c.PlayerId),
                    c.G, c.AB, c.R, c.H, c.Doubles, c.Triples, c.HR, c.RBI, c.SB, c.BB,
                    Math.Round(avg, 3), Math.Round(obp, 3), Math.Round(slg, 3), Math.Round(obp + slg, 3));
            }).ToList();

            return Results.Ok(PagedResponse.Create(leaders, page, pageSize, totalCount));
        }
    }

    private static async Task<IResult> GetPitchingLeaders(
        BaseballDbContext context, IMemoryCache cache,
        string stat = "w", int? fromYear = null, int? toYear = null,
        string? league = null, int minIp = 0, bool singleSeason = false,
        int page = 1, int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var hofPlayerIds = await GetHofPlayerIds(context, cache);
        var minOuts = minIp * 3;
        var isAscending = stat.ToLower() is "era" or "whip";

        var query = context.Pitching.AsQueryable();
        if (fromYear.HasValue) query = query.Where(p => p.YearId >= fromYear.Value);
        if (toYear.HasValue) query = query.Where(p => p.YearId <= toYear.Value);
        if (!string.IsNullOrEmpty(league)) query = query.Where(p => p.LgId == league);

        if (singleSeason)
        {
            var seasonQuery = query
                .Where(p => (p.Ipouts ?? 0) >= minOuts)
                .Select(p => new
                {
                    p.PlayerId,
                    PlayerName = (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                    p.YearId, p.TeamId,
                    TeamName = p.Team.Name,
                    G = p.G ?? 0, GS = p.Gs ?? 0, W = p.W ?? 0, L = p.L ?? 0,
                    SV = p.Sv ?? 0, CG = p.Cg ?? 0, SHO = p.Sho ?? 0,
                    IPOuts = p.Ipouts ?? 0, H = p.H ?? 0, ER = p.Er ?? 0,
                    HR = p.Hr ?? 0, BB = p.Bb ?? 0, SO = p.So ?? 0
                });

            var totalCount = await seasonQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var orderedQuery = ApplyPitchingOrder(seasonQuery, stat, isAscending);
            var data = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var leaders = data.Select((p, i) =>
            {
                var ip = p.IPOuts / 3.0;
                var era = ip > 0 ? p.ER * 9.0 / ip : 0;
                var whip = ip > 0 ? (p.BB + p.H) / ip : 0;
                return new PitchingLeaderDto(
                    (page - 1) * pageSize + i + 1, p.PlayerId, p.PlayerName, p.YearId,
                    p.TeamId, p.TeamName, hofPlayerIds.Contains(p.PlayerId),
                    p.G, p.GS, p.W, p.L, p.SV, p.CG, p.SHO, Math.Round(ip, 1),
                    p.H, p.ER, p.HR, p.BB, p.SO,
                    Math.Round(era, 2), Math.Round(whip, 3));
            }).ToList();

            return Results.Ok(PagedResponse.Create(leaders, page, pageSize, totalCount));
        }
        else
        {
            var careerQuery = query
                .GroupBy(p => p.PlayerId)
                .Select(g => new
                {
                    PlayerId = g.Key,
                    G = g.Sum(p => p.G ?? 0), GS = g.Sum(p => p.Gs ?? 0),
                    W = g.Sum(p => p.W ?? 0), L = g.Sum(p => p.L ?? 0),
                    SV = g.Sum(p => p.Sv ?? 0), CG = g.Sum(p => p.Cg ?? 0),
                    SHO = g.Sum(p => p.Sho ?? 0), IPOuts = g.Sum(p => p.Ipouts ?? 0),
                    H = g.Sum(p => p.H ?? 0), ER = g.Sum(p => p.Er ?? 0),
                    HR = g.Sum(p => p.Hr ?? 0), BB = g.Sum(p => p.Bb ?? 0),
                    SO = g.Sum(p => p.So ?? 0)
                })
                .Where(x => x.IPOuts >= minOuts);

            var totalCount = await careerQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Clamp(page, 1, Math.Max(1, totalPages));

            var orderedQuery = ApplyPitchingOrder(careerQuery, stat, isAscending);
            var data = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var playerIds = data.Select(c => c.PlayerId).ToList();
            var players = await context.People
                .Where(p => playerIds.Contains(p.PlayerId))
                .ToDictionaryAsync(p => p.PlayerId, p => (p.NameFirst ?? "") + " " + (p.NameLast ?? ""));

            var leaders = data.Select((p, i) =>
            {
                var ip = p.IPOuts / 3.0;
                var era = ip > 0 ? p.ER * 9.0 / ip : 0;
                var whip = ip > 0 ? (p.BB + p.H) / ip : 0;
                return new PitchingLeaderDto(
                    (page - 1) * pageSize + i + 1, p.PlayerId,
                    players.GetValueOrDefault(p.PlayerId, p.PlayerId), null,
                    null, null, hofPlayerIds.Contains(p.PlayerId),
                    p.G, p.GS, p.W, p.L, p.SV, p.CG, p.SHO, Math.Round(ip, 1),
                    p.H, p.ER, p.HR, p.BB, p.SO,
                    Math.Round(era, 2), Math.Round(whip, 3));
            }).ToList();

            return Results.Ok(PagedResponse.Create(leaders, page, pageSize, totalCount));
        }
    }

    // Expression tree helpers — same approach as the Razor Page models
    private static IOrderedQueryable<T> ApplyBattingOrder<T>(IQueryable<T> query, string stat) where T : class
    {
        return (query, stat.ToLower()) switch
        {
            (var q, "hr" or "homeruns") => q.OrderByDescending(DynExpr<T>("HR")),
            (var q, "h" or "hits") => q.OrderByDescending(DynExpr<T>("H")),
            (var q, "r" or "runs") => q.OrderByDescending(DynExpr<T>("R")),
            (var q, "rbi") => q.OrderByDescending(DynExpr<T>("RBI")),
            (var q, "sb" or "stolenbases") => q.OrderByDescending(DynExpr<T>("SB")),
            (var q, "2b" or "doubles") => q.OrderByDescending(DynExpr<T>("Doubles")),
            (var q, "3b" or "triples") => q.OrderByDescending(DynExpr<T>("Triples")),
            (var q, "bb" or "walks") => q.OrderByDescending(DynExpr<T>("BB")),
            (var q, "g" or "games") => q.OrderByDescending(DynExpr<T>("G")),
            (var q, "ab" or "atbats") => q.OrderByDescending(DynExpr<T>("AB")),
            (var q, "avg" or "battingaverage") => q.OrderByDescending(DynComputedExpr<T>("H", "AB")),
            (var q, "obp") => q.OrderByDescending(DynComputedExpr<T>("H", "BB", "AB", "BB")),
            (var q, "slg") => q.OrderByDescending(DynSlgExpr<T>()),
            (var q, "ops") => q.OrderByDescending(DynOpsExpr<T>()),
            (var q, _) => q.OrderByDescending(DynExpr<T>("H"))
        };
    }

    private static IOrderedQueryable<T> ApplyPitchingOrder<T>(IQueryable<T> query, string stat, bool ascending)
        where T : class
    {
        if (ascending)
        {
            return (query, stat.ToLower()) switch
            {
                (var q, "era") => q.OrderBy(DynEraExpr<T>()),
                (var q, "whip") => q.OrderBy(DynWhipExpr<T>()),
                (var q, _) => q.OrderBy(DynExpr<T>("W"))
            };
        }

        return (query, stat.ToLower()) switch
        {
            (var q, "w" or "wins") => q.OrderByDescending(DynExpr<T>("W")),
            (var q, "l" or "losses") => q.OrderByDescending(DynExpr<T>("L")),
            (var q, "so" or "strikeouts") => q.OrderByDescending(DynExpr<T>("SO")),
            (var q, "sv" or "saves") => q.OrderByDescending(DynExpr<T>("SV")),
            (var q, "cg") => q.OrderByDescending(DynExpr<T>("CG")),
            (var q, "sho") => q.OrderByDescending(DynExpr<T>("SHO")),
            (var q, "ip") => q.OrderByDescending(DynExpr<T>("IPOuts")),
            (var q, "g" or "games") => q.OrderByDescending(DynExpr<T>("G")),
            (var q, "gs") => q.OrderByDescending(DynExpr<T>("GS")),
            (var q, "hr") => q.OrderByDescending(DynExpr<T>("HR")),
            (var q, "k9") => q.OrderByDescending(DynK9Expr<T>()),
            (var q, "bb9") => q.OrderBy(DynBb9Expr<T>()),
            (var q, "wpct") => q.OrderByDescending(DynWpctExpr<T>()),
            (var q, _) => q.OrderByDescending(DynExpr<T>("W"))
        };
    }

    private static async Task<HashSet<string>> GetHofPlayerIds(BaseballDbContext context, IMemoryCache cache)
    {
        return (await cache.GetOrCreateAsync("hof_player_ids", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync();
        }))!;
    }

    #region Expression tree helpers

    private static Expression<Func<T, int>> DynExpr<T>(string propName)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var prop = Expression.Property(param, propName);
        return Expression.Lambda<Func<T, int>>(prop, param);
    }

    private static Expression<Func<T, double>> DynComputedExpr<T>(string numProp, string denomProp)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var num = Expression.Convert(Expression.Property(param, numProp), typeof(double));
        var denom = Expression.Convert(Expression.Property(param, denomProp), typeof(double));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denom, zero), zero, Expression.Divide(num, denom));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynComputedExpr<T>(
        string numProp1, string numProp2, string denomProp1, string denomProp2)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var n1 = Expression.Convert(Expression.Property(param, numProp1), typeof(double));
        var n2 = Expression.Convert(Expression.Property(param, numProp2), typeof(double));
        var d1 = Expression.Convert(Expression.Property(param, denomProp1), typeof(double));
        var d2 = Expression.Convert(Expression.Property(param, denomProp2), typeof(double));
        var num = Expression.Add(n1, n2);
        var denom = Expression.Add(d1, d2);
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denom, zero), zero, Expression.Divide(num, denom));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynSlgExpr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var h = Expression.Property(param, "H");
        var doubles = Expression.Property(param, "Doubles");
        var triples = Expression.Property(param, "Triples");
        var hr = Expression.Property(param, "HR");
        var ab = Expression.Property(param, "AB");
        var tb = Expression.Add(
            Expression.Add(h, doubles),
            Expression.Add(
                Expression.Multiply(Expression.Constant(2), triples),
                Expression.Multiply(Expression.Constant(3), hr)));
        var num = Expression.Convert(tb, typeof(double));
        var denom = Expression.Convert(ab, typeof(double));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denom, zero), zero, Expression.Divide(num, denom));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynOpsExpr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var h = Expression.Convert(Expression.Property(param, "H"), typeof(double));
        var bb = Expression.Convert(Expression.Property(param, "BB"), typeof(double));
        var ab = Expression.Convert(Expression.Property(param, "AB"), typeof(double));
        var doubles = Expression.Convert(Expression.Property(param, "Doubles"), typeof(double));
        var triples = Expression.Convert(Expression.Property(param, "Triples"), typeof(double));
        var hr = Expression.Convert(Expression.Property(param, "HR"), typeof(double));
        var zero = Expression.Constant(0.0);
        var two = Expression.Constant(2.0);
        var three = Expression.Constant(3.0);

        var obpNum = Expression.Add(h, bb);
        var obpDenom = Expression.Add(ab, bb);
        var obp = Expression.Condition(Expression.Equal(obpDenom, zero), zero, Expression.Divide(obpNum, obpDenom));

        var tb = Expression.Add(Expression.Add(h, doubles),
            Expression.Add(Expression.Multiply(two, triples), Expression.Multiply(three, hr)));
        var slg = Expression.Condition(Expression.Equal(ab, zero), zero, Expression.Divide(tb, ab));

        return Expression.Lambda<Func<T, double>>(Expression.Add(obp, slg), param);
    }

    private static Expression<Func<T, double>> DynEraExpr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var er = Expression.Convert(Expression.Property(param, "ER"), typeof(double));
        var ipouts = Expression.Convert(Expression.Property(param, "IPOuts"), typeof(double));
        var num = Expression.Multiply(er, Expression.Constant(27.0));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(
            Expression.Equal(ipouts, zero), Expression.Constant(double.MaxValue), Expression.Divide(num, ipouts));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynWhipExpr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var bb = Expression.Convert(Expression.Property(param, "BB"), typeof(double));
        var h = Expression.Convert(Expression.Property(param, "H"), typeof(double));
        var ipouts = Expression.Convert(Expression.Property(param, "IPOuts"), typeof(double));
        var num = Expression.Multiply(Expression.Add(bb, h), Expression.Constant(3.0));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(
            Expression.Equal(ipouts, zero), Expression.Constant(double.MaxValue), Expression.Divide(num, ipouts));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynK9Expr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var so = Expression.Convert(Expression.Property(param, "SO"), typeof(double));
        var ipouts = Expression.Convert(Expression.Property(param, "IPOuts"), typeof(double));
        var num = Expression.Multiply(so, Expression.Constant(27.0));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(ipouts, zero), zero, Expression.Divide(num, ipouts));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynBb9Expr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var bb = Expression.Convert(Expression.Property(param, "BB"), typeof(double));
        var ipouts = Expression.Convert(Expression.Property(param, "IPOuts"), typeof(double));
        var num = Expression.Multiply(bb, Expression.Constant(27.0));
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(ipouts, zero), zero, Expression.Divide(num, ipouts));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    private static Expression<Func<T, double>> DynWpctExpr<T>()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var w = Expression.Convert(Expression.Property(param, "W"), typeof(double));
        var l = Expression.Convert(Expression.Property(param, "L"), typeof(double));
        var denom = Expression.Add(w, l);
        var zero = Expression.Constant(0.0);
        var body = Expression.Condition(Expression.Equal(denom, zero), zero, Expression.Divide(w, denom));
        return Expression.Lambda<Func<T, double>>(body, param);
    }

    #endregion
}
