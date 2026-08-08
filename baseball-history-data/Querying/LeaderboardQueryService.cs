using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballHistory.Data.Querying;

public sealed class LeaderboardQueryService : ILeaderboardQueryService
{
    private readonly BaseballDbContext _context;

    public LeaderboardQueryService(BaseballDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BattingLeaderRow>> GetBattingLeadersAsync(
        LeaderboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var statDef = LeaderboardStatCatalog.GetBattingStat(request.Stat)
            ?? throw new ArgumentException($"Unknown batting stat: {request.Stat}");

        var query = _context.Batting.AsQueryable();

        // Apply filters
        if (request.FromYear.HasValue)
            query = query.Where(b => b.YearId >= request.FromYear.Value);
        if (request.ToYear.HasValue)
            query = query.Where(b => b.YearId <= request.ToYear.Value);
        if (!string.IsNullOrEmpty(request.League))
            query = query.Where(b => b.LgId == request.League);

        var hofPlayerIds = await _context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (request.SingleSeason)
        {
            return await GetSingleSeasonBattingLeadersAsync(query, request, statDef, hofPlayerIds, cancellationToken);
        }
        else
        {
            return await GetCareerBattingLeadersAsync(query, request, statDef, hofPlayerIds, cancellationToken);
        }
    }

    private async Task<PagedResult<BattingLeaderRow>> GetSingleSeasonBattingLeadersAsync(
        IQueryable<Batting> query,
        LeaderboardRequest request,
        LeaderboardStatDefinition statDef,
        List<string> hofPlayerIds,
        CancellationToken cancellationToken)
    {
        var dataQuery = query
            .Select(b => new
            {
                b.PlayerId,
                PlayerName = (b.Player.NameFirst ?? "") + " " + (b.Player.NameLast ?? ""),
                b.YearId,
                b.TeamId,
                TeamName = b.Team.Name,
                G = (int)(b.G ?? 0),
                AB = (int)(b.Ab ?? 0),
                R = (int)(b.R ?? 0),
                H = (int)(b.H ?? 0),
                Doubles = (int)(b._2b ?? 0),
                Triples = (int)(b._3b ?? 0),
                HR = (int)(b.Hr ?? 0),
                RBI = (int)(b.Rbi ?? 0),
                SB = (int)(b.Sb ?? 0),
                BB = (int)(b.Bb ?? 0),
                HBP = (int?)(b.Hbp),
                SH = (int?)(b.Sh),
                SF = (int?)(b.Sf),
                TeamGames = (int)(b.Team.G ?? 0)
            });

        // Apply qualification
        if (statDef.IsRateStat)
        {
            if (request.MinAtBats.HasValue)
            {
                // Explicit override
                dataQuery = dataQuery.Where(x => x.AB >= request.MinAtBats.Value);
            }
            else if (request.Qualified)
            {
                // Season-relative: PA >= 3.1 × TeamGames
                // PA = AB + BB + COALESCE(HBP,0) + COALESCE(SH,0) + COALESCE(SF,0)
                dataQuery = dataQuery.Where(x =>
                    x.AB + x.BB + (x.HBP ?? 0) + (x.SH ?? 0) + (x.SF ?? 0) >=
                    QualificationRules.BattingPlateAppearancesPerGame * x.TeamGames);
            }
        }

        // Filter out zero AB for rate stats
        if (statDef.IsRateStat)
        {
            dataQuery = dataQuery.Where(x => x.AB > 0);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);

        // Materialize data
        var data = await dataQuery.ToListAsync(cancellationToken);

        // Compute rates
        var rows = data.Select(x => new
        {
            x.PlayerId,
            x.PlayerName,
            IsHallOfFamer = hofPlayerIds.Contains(x.PlayerId),
            x.YearId,
            x.TeamId,
            x.TeamName,
            x.G,
            x.AB,
            x.R,
            x.H,
            x.Doubles,
            x.Triples,
            x.HR,
            x.RBI,
            x.SB,
            x.BB,
            AVG = x.AB > 0 ? (decimal)x.H / x.AB : (decimal?)null,
            OBP = ComputeOBP(x.H, x.BB, x.HBP, x.AB, x.SF),
            SLG = ComputeSLG(x.H, x.Doubles, x.Triples, x.HR, x.AB),
            OPS = ComputeOBP(x.H, x.BB, x.HBP, x.AB, x.SF) + ComputeSLG(x.H, x.Doubles, x.Triples, x.HR, x.AB),
            SortKey = GetBattingSortKey(statDef.Key, x.H, x.BB, x.HBP, x.AB, x.SF, x.Doubles, x.Triples, x.HR, x.R, x.RBI, x.SB, x.G)
        }).ToList();

        // Sort
        var sorted = statDef.SortDirection == "ascending"
            ? rows.OrderBy(x => x.SortKey).ThenBy(x => x.PlayerId).ToList()
            : rows.OrderByDescending(x => x.SortKey).ThenBy(x => x.PlayerId).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var page = Math.Clamp(request.Page, 1, Math.Max(1, totalPages));

        var pagedRows = sorted
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select((x, index) => new BattingLeaderRow(
                Rank: (page - 1) * request.PageSize + index + 1,
                x.PlayerId,
                x.PlayerName,
                x.IsHallOfFamer,
                x.YearId,
                x.TeamId,
                x.TeamName,
                x.G,
                x.AB,
                x.R,
                x.H,
                x.Doubles,
                x.Triples,
                x.HR,
                x.RBI,
                x.SB,
                x.BB,
                x.AVG,
                x.OBP,
                x.SLG,
                x.OPS))
            .ToList();

        return new PagedResult<BattingLeaderRow>(pagedRows, page, request.PageSize, totalCount, totalPages);
    }

    private async Task<PagedResult<BattingLeaderRow>> GetCareerBattingLeadersAsync(
        IQueryable<Batting> query,
        LeaderboardRequest request,
        LeaderboardStatDefinition statDef,
        List<string> hofPlayerIds,
        CancellationToken cancellationToken)
    {
        // Group by player, summing across stints
        var grouped = query
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                PlayerName = (g.First().Player.NameFirst ?? "") + " " + (g.First().Player.NameLast ?? ""),
                G = g.Sum(b => (int?)(b.G)) ?? 0,
                AB = g.Sum(b => (int?)(b.Ab)) ?? 0,
                R = g.Sum(b => (int?)(b.R)) ?? 0,
                H = g.Sum(b => (int?)(b.H)) ?? 0,
                Doubles = g.Sum(b => (int?)(b._2b)) ?? 0,
                Triples = g.Sum(b => (int?)(b._3b)) ?? 0,
                HR = g.Sum(b => (int?)(b.Hr)) ?? 0,
                RBI = g.Sum(b => (int?)(b.Rbi)) ?? 0,
                SB = g.Sum(b => (int?)(b.Sb)) ?? 0,
                BB = g.Sum(b => (int?)(b.Bb)) ?? 0,
                HBP = g.Sum(b => (int?)(b.Hbp)),
                SH = g.Sum(b => (int?)(b.Sh)),
                SF = g.Sum(b => (int?)(b.Sf)),
                // Season-relative threshold: SUM(3.1 × Teams.G) for stints with valid Team.G
                // Use conditional to skip null/zero values (EF Core translates to SQL CASE)
                Threshold = g.Sum(b => 
                    b.Team != null && b.Team.G.HasValue && b.Team.G.Value > 0
                        ? (decimal?)(QualificationRules.BattingPlateAppearancesPerGame * b.Team.G.Value)
                        : (decimal?)null
                )
            });

        // Apply qualification
        if (statDef.IsRateStat)
        {
            if (request.MinAtBats.HasValue)
            {
                // Explicit override takes precedence over automatic qualification
                grouped = grouped.Where(x => x.AB >= request.MinAtBats.Value);
            }
            else if (request.Qualified)
            {
                // Season-relative qualification: apply the computed Threshold (SUM of 3.1 PA
                // per team game across stints) as an enhancement OVER a flat 100 PA sanity
                // floor, never below it. This matters because some (mostly 19th-century or
                // partial-season) teams have anomalously low Teams.G values in the source
                // data, which would otherwise let degenerate small-sample careers (e.g. a
                // handful of AB) qualify with a near-zero computed threshold. Falls back to
                // the flat floor alone when Threshold is null due to missing Teams.G data.
                grouped = grouped.Where(x =>
                    x.Threshold.HasValue
                        ? x.AB + x.BB + (x.HBP ?? 0) + (x.SH ?? 0) + (x.SF ?? 0) >= (x.Threshold.Value > 100 ? x.Threshold.Value : 100)
                        : x.AB + x.BB + (x.HBP ?? 0) + (x.SH ?? 0) + (x.SF ?? 0) >= 100);
            }
            // When request.Qualified is false and no explicit override, no threshold filter is applied.
        }

        if (statDef.IsRateStat)
        {
            grouped = grouped.Where(x => x.AB > 0);
        }

        var totalCount = await grouped.CountAsync(cancellationToken);

        // Materialize
        var data = await grouped.ToListAsync(cancellationToken);

        // Compute rates
        var rows = data.Select(x => new
        {
            x.PlayerId,
            x.PlayerName,
            IsHallOfFamer = hofPlayerIds.Contains(x.PlayerId),
            x.G,
            x.AB,
            x.R,
            x.H,
            x.Doubles,
            x.Triples,
            x.HR,
            x.RBI,
            x.SB,
            x.BB,
            AVG = x.AB > 0 ? (decimal)x.H / x.AB : (decimal?)null,
            OBP = ComputeOBP(x.H, x.BB, x.HBP, x.AB, x.SF),
            SLG = ComputeSLG(x.H, x.Doubles, x.Triples, x.HR, x.AB),
            OPS = ComputeOBP(x.H, x.BB, x.HBP, x.AB, x.SF) + ComputeSLG(x.H, x.Doubles, x.Triples, x.HR, x.AB),
            SortKey = GetBattingSortKey(statDef.Key, x.H, x.BB, x.HBP, x.AB, x.SF, x.Doubles, x.Triples, x.HR, x.R, x.RBI, x.SB, x.G)
        }).ToList();

        // Sort with tie-breaker
        var sorted = statDef.SortDirection == "ascending"
            ? rows.OrderBy(x => x.SortKey).ThenBy(x => x.PlayerId).ToList()
            : rows.OrderByDescending(x => x.SortKey).ThenBy(x => x.PlayerId).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var page = Math.Clamp(request.Page, 1, Math.Max(1, totalPages));

        var pagedRows = sorted
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select((x, index) => new BattingLeaderRow(
                Rank: (page - 1) * request.PageSize + index + 1,
                x.PlayerId,
                x.PlayerName,
                x.IsHallOfFamer,
                YearId: null,
                TeamId: null,
                TeamName: null,
                x.G,
                x.AB,
                x.R,
                x.H,
                x.Doubles,
                x.Triples,
                x.HR,
                x.RBI,
                x.SB,
                x.BB,
                x.AVG,
                x.OBP,
                x.SLG,
                x.OPS))
            .ToList();

        return new PagedResult<BattingLeaderRow>(pagedRows, page, request.PageSize, totalCount, totalPages);
    }

    public async Task<PagedResult<PitchingLeaderRow>> GetPitchingLeadersAsync(
        LeaderboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var statDef = LeaderboardStatCatalog.GetPitchingStat(request.Stat)
            ?? throw new ArgumentException($"Unknown pitching stat: {request.Stat}");

        var query = _context.Pitching.AsQueryable();

        // Apply filters
        if (request.FromYear.HasValue)
            query = query.Where(p => p.YearId >= request.FromYear.Value);
        if (request.ToYear.HasValue)
            query = query.Where(p => p.YearId <= request.ToYear.Value);
        if (!string.IsNullOrEmpty(request.League))
            query = query.Where(p => p.LgId == request.League);

        var hofPlayerIds = await _context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (request.SingleSeason)
        {
            return await GetSingleSeasonPitchingLeadersAsync(query, request, statDef, hofPlayerIds, cancellationToken);
        }
        else
        {
            return await GetCareerPitchingLeadersAsync(query, request, statDef, hofPlayerIds, cancellationToken);
        }
    }

    private async Task<PagedResult<PitchingLeaderRow>> GetSingleSeasonPitchingLeadersAsync(
        IQueryable<Pitching> query,
        LeaderboardRequest request,
        LeaderboardStatDefinition statDef,
        List<string> hofPlayerIds,
        CancellationToken cancellationToken)
    {
        var dataQuery = query
            .Select(p => new
            {
                p.PlayerId,
                PlayerName = (p.Player.NameFirst ?? "") + " " + (p.Player.NameLast ?? ""),
                p.YearId,
                p.TeamId,
                TeamName = p.Team!.Name,
                G = (int)(p.G ?? 0),
                GS = (int)(p.Gs ?? 0),
                W = (int)(p.W ?? 0),
                L = (int)(p.L ?? 0),
                SV = (int)(p.Sv ?? 0),
                CG = (int)(p.Cg ?? 0),
                SHO = (int)(p.Sho ?? 0),
                IPouts = (int)(p.Ipouts ?? 0),
                H = (int)(p.H ?? 0),
                HR = (int)(p.Hr ?? 0),
                BB = (int)(p.Bb ?? 0),
                SO = (int)(p.So ?? 0),
                ER = (int?)(p.Er),
                TeamGames = (int)(p.Team!.G ?? 0)
            });

        // Apply qualification
        if (statDef.IsRateStat)
        {
            if (request.MinInningsPitched.HasValue)
            {
                dataQuery = dataQuery.Where(x => x.IPouts >= request.MinInningsPitched.Value * 3);
            }
            else if (request.Qualified)
            {
                // Season-relative: IPouts >= 3 × TeamGames
                dataQuery = dataQuery.Where(x => x.IPouts >= QualificationRules.PitchingOutsPerGame * x.TeamGames);
            }
        }

        if (statDef.IsRateStat)
        {
            dataQuery = dataQuery.Where(x => x.IPouts > 0);
        }

        var totalCount = await dataQuery.CountAsync(cancellationToken);

        var data = await dataQuery.ToListAsync(cancellationToken);

        var rows = data.Select(x => new
        {
            x.PlayerId,
            x.PlayerName,
            IsHallOfFamer = hofPlayerIds.Contains(x.PlayerId),
            x.YearId,
            x.TeamId,
            x.TeamName,
            x.G,
            x.GS,
            x.W,
            x.L,
            x.SV,
            x.CG,
            x.SHO,
            IP = x.IPouts / 3.0m,
            x.H,
            x.HR,
            x.BB,
            x.SO,
            ERA = ComputeERA(x.ER, x.IPouts),
            WHIP = ComputeWHIP(x.BB, x.H, x.IPouts),
            K9 = ComputeK9(x.SO, x.IPouts),
            BB9 = ComputeBB9(x.BB, x.IPouts),
            WPCT = ComputeWPCT(x.W, x.L),
            SortKey = GetPitchingSortKey(statDef.Key, x.W, x.L, x.SO, x.SV, x.CG, x.SHO, x.IPouts, x.G, x.GS, x.HR, x.ER, x.BB, x.H)
        }).ToList();

        var sorted = statDef.SortDirection == "ascending"
            ? rows.OrderBy(x => x.SortKey).ThenBy(x => x.PlayerId).ToList()
            : rows.OrderByDescending(x => x.SortKey).ThenBy(x => x.PlayerId).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var page = Math.Clamp(request.Page, 1, Math.Max(1, totalPages));

        var pagedRows = sorted
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select((x, index) => new PitchingLeaderRow(
                Rank: (page - 1) * request.PageSize + index + 1,
                x.PlayerId,
                x.PlayerName,
                x.IsHallOfFamer,
                x.YearId,
                x.TeamId,
                x.TeamName,
                x.G,
                x.GS,
                x.W,
                x.L,
                x.SV,
                x.CG,
                x.SHO,
                x.IP,
                x.H,
                x.HR,
                x.BB,
                x.SO,
                x.ERA,
                x.WHIP,
                x.K9,
                x.BB9,
                x.WPCT))
            .ToList();

        return new PagedResult<PitchingLeaderRow>(pagedRows, page, request.PageSize, totalCount, totalPages);
    }

    private async Task<PagedResult<PitchingLeaderRow>> GetCareerPitchingLeadersAsync(
        IQueryable<Pitching> query,
        LeaderboardRequest request,
        LeaderboardStatDefinition statDef,
        List<string> hofPlayerIds,
        CancellationToken cancellationToken)
    {
        var grouped = query
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                PlayerName = (g.First().Player.NameFirst ?? "") + " " + (g.First().Player.NameLast ?? ""),
                G = g.Sum(p => (int?)(p.G)) ?? 0,
                GS = g.Sum(p => (int?)(p.Gs)) ?? 0,
                W = g.Sum(p => (int?)(p.W)) ?? 0,
                L = g.Sum(p => (int?)(p.L)) ?? 0,
                SV = g.Sum(p => (int?)(p.Sv)) ?? 0,
                CG = g.Sum(p => (int?)(p.Cg)) ?? 0,
                SHO = g.Sum(p => (int?)(p.Sho)) ?? 0,
                IPouts = g.Sum(p => (int?)(p.Ipouts)) ?? 0,
                H = g.Sum(p => (int?)(p.H)) ?? 0,
                HR = g.Sum(p => (int?)(p.Hr)) ?? 0,
                BB = g.Sum(p => (int?)(p.Bb)) ?? 0,
                SO = g.Sum(p => (int?)(p.So)) ?? 0,
                ER = g.Sum(p => (int?)(p.Er)),
                // Season-relative threshold: SUM(3 outs × Teams.G) for stints with valid Team.G
                // Use conditional to skip null/zero values (EF Core translates to SQL CASE)
                Threshold = g.Sum(p => 
                    p.Team != null && p.Team.G.HasValue && p.Team.G.Value > 0
                        ? (decimal?)(QualificationRules.PitchingOutsPerGame * p.Team.G.Value)
                        : (decimal?)null
                )
            });

        if (statDef.IsRateStat)
        {
            if (request.MinInningsPitched.HasValue)
            {
                // Explicit override takes precedence over automatic qualification
                grouped = grouped.Where(x => x.IPouts >= request.MinInningsPitched.Value * 3);
            }
            else if (request.Qualified)
            {
                // Season-relative qualification: apply the computed Threshold (SUM of 3 outs
                // per team game across stints) as an enhancement OVER a flat 90-out (30 IP)
                // sanity floor, never below it - see the equivalent batting comment above for
                // rationale (anomalously low Teams.G values in some source data). Falls back
                // to the flat floor alone when Threshold is null due to missing Teams.G data.
                grouped = grouped.Where(x =>
                    x.Threshold.HasValue
                        ? x.IPouts >= (x.Threshold.Value > 90 ? x.Threshold.Value : 90)
                        : x.IPouts >= 90);
            }
            // When request.Qualified is false and no explicit override, no threshold filter is applied.
        }

        if (statDef.IsRateStat)
        {
            grouped = grouped.Where(x => x.IPouts > 0);
        }

        var totalCount = await grouped.CountAsync(cancellationToken);

        var data = await grouped.ToListAsync(cancellationToken);

        var rows = data.Select(x => new
        {
            x.PlayerId,
            x.PlayerName,
            IsHallOfFamer = hofPlayerIds.Contains(x.PlayerId),
            x.G,
            x.GS,
            x.W,
            x.L,
            x.SV,
            x.CG,
            x.SHO,
            IP = x.IPouts / 3.0m,
            x.H,
            x.HR,
            x.BB,
            x.SO,
            ERA = ComputeERA(x.ER, x.IPouts),
            WHIP = ComputeWHIP(x.BB, x.H, x.IPouts),
            K9 = ComputeK9(x.SO, x.IPouts),
            BB9 = ComputeBB9(x.BB, x.IPouts),
            WPCT = ComputeWPCT(x.W, x.L),
            SortKey = GetPitchingSortKey(statDef.Key, x.W, x.L, x.SO, x.SV, x.CG, x.SHO, x.IPouts, x.G, x.GS, x.HR, x.ER, x.BB, x.H)
        }).ToList();

        var sorted = statDef.SortDirection == "ascending"
            ? rows.OrderBy(x => x.SortKey).ThenBy(x => x.PlayerId).ToList()
            : rows.OrderByDescending(x => x.SortKey).ThenBy(x => x.PlayerId).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var page = Math.Clamp(request.Page, 1, Math.Max(1, totalPages));

        var pagedRows = sorted
            .Skip((page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select((x, index) => new PitchingLeaderRow(
                Rank: (page - 1) * request.PageSize + index + 1,
                x.PlayerId,
                x.PlayerName,
                x.IsHallOfFamer,
                YearId: null,
                TeamId: null,
                TeamName: null,
                x.G,
                x.GS,
                x.W,
                x.L,
                x.SV,
                x.CG,
                x.SHO,
                x.IP,
                x.H,
                x.HR,
                x.BB,
                x.SO,
                x.ERA,
                x.WHIP,
                x.K9,
                x.BB9,
                x.WPCT))
            .ToList();

        return new PagedResult<PitchingLeaderRow>(pagedRows, page, request.PageSize, totalCount, totalPages);
    }

    // Batting formulas
    private static decimal? ComputeOBP(int h, int bb, int? hbp, int ab, int? sf)
    {
        var denominator = ab + bb + (hbp ?? 0) + (sf ?? 0);
        if (denominator == 0) return null;
        return (decimal)(h + bb + (hbp ?? 0)) / denominator;
    }

    private static decimal? ComputeSLG(int h, int doubles, int triples, int hr, int ab)
    {
        if (ab == 0) return null;
        var singles = h - doubles - triples - hr;
        var totalBases = singles + (2 * doubles) + (3 * triples) + (4 * hr);
        return (decimal)totalBases / ab;
    }

    // Pitching formulas
    private static decimal? ComputeERA(int? er, int ipouts)
    {
        if (ipouts == 0 || !er.HasValue) return null;
        return (decimal)er.Value * 27 / ipouts;
    }

    private static decimal? ComputeWHIP(int bb, int h, int ipouts)
    {
        if (ipouts == 0) return null;
        return (decimal)(bb + h) * 3 / ipouts;
    }

    private static decimal? ComputeK9(int so, int ipouts)
    {
        if (ipouts == 0) return null;
        return (decimal)so * 27 / ipouts;
    }

    private static decimal? ComputeBB9(int bb, int ipouts)
    {
        if (ipouts == 0) return null;
        return (decimal)bb * 27 / ipouts;
    }

    private static decimal? ComputeWPCT(int w, int l)
    {
        var decisions = w + l;
        if (decisions == 0) return null;
        return (decimal)w / decisions;
    }

    private static decimal GetBattingSortKey(string stat, int h, int bb, int? hbp, int ab, int? sf, int doubles, int triples, int hr, int r, int rbi, int sb, int g)
    {
        return stat switch
        {
            "hr" => hr,
            "h" => h,
            "r" => r,
            "rbi" => rbi,
            "sb" => sb,
            "2b" => doubles,
            "3b" => triples,
            "bb" => bb,
            "g" => g,
            "ab" => ab,
            "avg" => ab > 0 ? (decimal)h / ab : 0,
            "obp" => ComputeOBP(h, bb, hbp, ab, sf) ?? 0,
            "slg" => ComputeSLG(h, doubles, triples, hr, ab) ?? 0,
            "ops" => (ComputeOBP(h, bb, hbp, ab, sf) ?? 0) + (ComputeSLG(h, doubles, triples, hr, ab) ?? 0),
            _ => 0
        };
    }

    private static decimal GetPitchingSortKey(string stat, int w, int l, int so, int sv, int cg, int sho, int ipouts, int g, int gs, int hr, int? er, int bb, int h)
    {
        return stat switch
        {
            "w" => w,
            "l" => l,
            "so" => so,
            "bb" => bb,
            "sv" => sv,
            "cg" => cg,
            "sho" => sho,
            "ip" => ipouts / 3.0m,
            "g" => g,
            "gs" => gs,
            "hr" => hr,
            "era" => ComputeERA(er, ipouts) ?? 0,
            "whip" => ComputeWHIP(bb, h, ipouts) ?? 0,
            "k9" => ComputeK9(so, ipouts) ?? 0,
            "bb9" => ComputeBB9(bb, ipouts) ?? 0,
            "wpct" => ComputeWPCT(w, l) ?? 0,
            _ => 0
        };
    }
}
