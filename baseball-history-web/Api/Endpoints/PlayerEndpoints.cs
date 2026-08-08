using baseball_history_web.Api.Dtos;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Api.Endpoints;

public static class PlayerEndpoints
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", GetPlayers).WithSummary("List players by last name letter");
        group.MapGet("/{playerId}", GetPlayerDetail).WithSummary("Get player detail with career stats");
        group.MapGet("/{playerId}/batting", GetPlayerBatting).WithSummary("Season-by-season batting stats");
        group.MapGet("/{playerId}/pitching", GetPlayerPitching).WithSummary("Season-by-season pitching stats");
        group.MapGet("/{playerId}/fielding", GetPlayerFielding).WithSummary("Season-by-season fielding stats");
        group.MapGet("/{playerId}/awards", GetPlayerAwards).WithSummary("Player awards");
        group.MapGet("/{playerId}/postseason/batting", GetPlayerPostseasonBatting).WithSummary("Postseason batting stats");
        group.MapGet("/{playerId}/postseason/pitching", GetPlayerPostseasonPitching).WithSummary("Postseason pitching stats");
    }

    private static async Task<IResult> GetPlayers(
        BaseballDbContext context, IMemoryCache cache,
        string? letter = null, int page = 1, int pageSize = 25)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var currentLetter = letter?.ToUpper().FirstOrDefault() ?? 'A';

        var hofPlayerIds = await GetHofPlayerIds(context, cache);

        var query = context.People
            .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith(currentLetter.ToString()))
            .OrderBy(p => p.NameLast)
            .ThenBy(p => p.NameFirst);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var players = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst,
                p.NameLast,
                DebutYear = p.Debut,
                FinalYear = p.FinalGame,
                TotalGames = (p.Battings.Sum(b => (int?)b.G) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0),
                TotalHits = p.Battings.Sum(b => (int?)b.H) ?? 0,
                TotalHR = p.Battings.Sum(b => (int?)b.Hr) ?? 0,
                LastTeam = p.Battings.OrderByDescending(b => b.YearId).Select(b => b.TeamId).FirstOrDefault()
                           ?? p.Pitchings.OrderByDescending(pi => pi.YearId).Select(pi => pi.TeamId).FirstOrDefault()
            })
            .ToListAsync();

        var data = players.Select(p => new PlayerListItem(
            p.PlayerId,
            p.NameFirst,
            p.NameLast,
            p.DebutYear?.Year,
            p.FinalYear?.Year,
            hofPlayerIds.Contains(p.PlayerId),
            p.TotalGames > 0 ? p.TotalGames : null,
            p.TotalHits > 0 ? p.TotalHits : null,
            p.TotalHR > 0 ? p.TotalHR : null,
            p.LastTeam
        )).ToList();

        return Results.Ok(PagedResponse.Create(data, page, pageSize, totalCount));
    }

    private static async Task<IResult> GetPlayerDetail(
        string playerId, BaseballDbContext context, IMemoryCache cache)
    {
        var person = await context.People.FirstOrDefaultAsync(p => p.PlayerId == playerId);
        if (person == null) return Results.NotFound();

        var hofPlayerIds = await GetHofPlayerIds(context, cache);
        var isHof = hofPlayerIds.Contains(playerId);

        int? hofYear = null;
        if (isHof)
        {
            hofYear = await context.HallOfFame
                .Where(h => h.PlayerId == playerId && h.Inducted == "Y")
                .OrderBy(h => h.Yearid)
                .Select(h => (int?)h.Yearid)
                .FirstOrDefaultAsync();
        }

        // Career batting
        var battingAgg = await context.Batting
            .Where(b => b.PlayerId == playerId)
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                G = g.Sum(b => b.G ?? 0),
                AB = g.Sum(b => b.Ab ?? 0),
                R = g.Sum(b => b.R ?? 0),
                H = g.Sum(b => b.H ?? 0),
                Doubles = g.Sum(b => b._2b ?? 0),
                Triples = g.Sum(b => b._3b ?? 0),
                HR = g.Sum(b => b.Hr ?? 0),
                BB = g.Sum(b => b.Bb ?? 0),
                RBI = g.Sum(b => b.Rbi ?? 0),
                SB = g.Sum(b => b.Sb ?? 0),
                SO = g.Sum(b => b.So ?? 0)
            })
            .FirstOrDefaultAsync();

        CareerBattingDto? careerBatting = null;
        if (battingAgg is { AB: > 0 })
        {
            var avg = (double)battingAgg.H / battingAgg.AB;
            var obp = (battingAgg.AB + battingAgg.BB) > 0
                ? (double)(battingAgg.H + battingAgg.BB) / (battingAgg.AB + battingAgg.BB) : 0;
            var tb = battingAgg.H + battingAgg.Doubles + 2 * battingAgg.Triples + 3 * battingAgg.HR;
            var slg = (double)tb / battingAgg.AB;
            careerBatting = new CareerBattingDto(
                battingAgg.G, battingAgg.AB, battingAgg.R, battingAgg.H,
                battingAgg.Doubles, battingAgg.Triples, battingAgg.HR, battingAgg.RBI,
                battingAgg.SB, battingAgg.BB, battingAgg.SO,
                Math.Round(avg, 3), Math.Round(obp, 3), Math.Round(slg, 3), Math.Round(obp + slg, 3));
        }

        // Career pitching
        var pitchingAgg = await context.Pitching
            .Where(p => p.PlayerId == playerId)
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                G = g.Sum(p => p.G ?? 0),
                GS = g.Sum(p => p.Gs ?? 0),
                W = g.Sum(p => p.W ?? 0),
                L = g.Sum(p => p.L ?? 0),
                SV = g.Sum(p => p.Sv ?? 0),
                CG = g.Sum(p => p.Cg ?? 0),
                SHO = g.Sum(p => p.Sho ?? 0),
                IPOuts = g.Sum(p => p.Ipouts ?? 0),
                H = g.Sum(p => p.H ?? 0),
                ER = g.Sum(p => p.Er ?? 0),
                HR = g.Sum(p => p.Hr ?? 0),
                BB = g.Sum(p => p.Bb ?? 0),
                SO = g.Sum(p => p.So ?? 0)
            })
            .FirstOrDefaultAsync();

        CareerPitchingDto? careerPitching = null;
        if (pitchingAgg is { G: > 0 })
        {
            var ip = pitchingAgg.IPOuts / 3.0;
            var era = ip > 0 ? pitchingAgg.ER * 9.0 / ip : 0;
            var whip = ip > 0 ? (pitchingAgg.BB + pitchingAgg.H) / ip : 0;
            careerPitching = new CareerPitchingDto(
                pitchingAgg.G, pitchingAgg.GS, pitchingAgg.W, pitchingAgg.L, pitchingAgg.SV,
                pitchingAgg.CG, pitchingAgg.SHO, Math.Round(ip, 1),
                pitchingAgg.H, pitchingAgg.ER, pitchingAgg.HR, pitchingAgg.BB, pitchingAgg.SO,
                Math.Round(era, 2), Math.Round(whip, 3));
        }

        // Teams played for
        var battingTeams = await context.Batting
            .Where(b => b.PlayerId == playerId)
            .GroupBy(b => b.TeamId)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(b => b.YearId),
                LastYear = g.Max(b => b.YearId),
                Seasons = g.Select(b => b.YearId).Distinct().Count()
            })
            .ToListAsync();

        var existingTeamIds = battingTeams.Select(t => t.TeamId).ToHashSet();
        var pitchingTeams = await context.Pitching
            .Where(p => p.PlayerId == playerId && p.TeamId != null && !existingTeamIds.Contains(p.TeamId))
            .GroupBy(p => p.TeamId!)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(p => p.YearId),
                LastYear = g.Max(p => p.YearId),
                Seasons = g.Select(p => p.YearId).Distinct().Count()
            })
            .ToListAsync();

        var allTeamData = battingTeams
            .Concat(pitchingTeams)
            .OrderBy(t => t.FirstYear)
            .ToList();

        var teamIds = allTeamData.Select(t => t.TeamId).Distinct().ToList();
        var teamNames = await context.Teams
            .Where(t => teamIds.Contains(t.TeamId))
            .GroupBy(t => t.TeamId)
            .Select(g => new { TeamId = g.Key, Name = g.OrderByDescending(t => t.YearId).First().Name })
            .ToDictionaryAsync(t => t.TeamId, t => t.Name);

        var teams = allTeamData.Select(t =>
            new PlayerTeamDto(t.TeamId, teamNames.GetValueOrDefault(t.TeamId), t.FirstYear, t.LastYear, t.Seasons)
        ).ToList();

        string? birthDate = null;
        if (person.BirthYear != null)
            birthDate = $"{person.BirthYear}-{person.BirthMonth ?? "01"}-{person.BirthDay ?? "01"}";

        string? deathDate = null;
        if (person.DeathYear != null)
            deathDate = $"{person.DeathYear}-{person.DeathMonth ?? "01"}-{person.DeathDay ?? "01"}";

        string? birthPlace = null;
        if (person.BirthCity != null || person.BirthState != null)
            birthPlace = string.Join(", ", new[] { person.BirthCity, person.BirthState, person.BirthCountry }.Where(s => s != null));

        return Results.Ok(new PlayerDetail(
            person.PlayerId, person.NameFirst, person.NameLast, person.NameGiven,
            person.Height, person.Weight, person.Bats, person.Throws,
            birthDate, person.BirthCity, person.BirthState, person.BirthCountry, deathDate,
            person.Debut?.Year, person.FinalGame?.Year,
            isHof, hofYear, careerBatting, careerPitching, teams));
    }

    private static async Task<IResult> GetPlayerBatting(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var seasons = await context.Batting
            .Where(b => b.PlayerId == playerId)
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => b.Stint)
            .Select(b => new SeasonBattingDto(
                b.YearId, b.TeamId, b.Team.Name, b.LgId,
                b.G ?? 0, b.Ab ?? 0, b.R ?? 0, b.H ?? 0,
                b._2b ?? 0, b._3b ?? 0, b.Hr ?? 0, b.Rbi ?? 0,
                b.Sb ?? 0, b.Bb ?? 0, b.So ?? 0,
                (b.Ab ?? 0) > 0 ? (double)(b.H ?? 0) / (b.Ab ?? 0) : 0))
            .ToListAsync();

        return Results.Ok(seasons);
    }

    private static async Task<IResult> GetPlayerPitching(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var seasons = await context.Pitching
            .Where(p => p.PlayerId == playerId)
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => p.Stint)
            .Select(p => new SeasonPitchingDto(
                p.YearId, p.TeamId ?? "", p.Team != null ? p.Team.Name : null, p.LgId ?? "",
                p.G ?? 0, p.Gs ?? 0, p.W ?? 0, p.L ?? 0, p.Sv ?? 0,
                (p.Ipouts ?? 0) / 3.0, p.H ?? 0, p.Er ?? 0, p.So ?? 0, p.Bb ?? 0,
                (p.Ipouts ?? 0) > 0 ? ((p.Er ?? 0) * 27.0) / (p.Ipouts ?? 0) : 0))
            .ToListAsync();

        return Results.Ok(seasons);
    }

    private static async Task<IResult> GetPlayerFielding(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var rows = await context.Fielding
            .Where(f => f.PlayerId == playerId)
            .OrderByDescending(f => f.YearId)
            .ThenBy(f => f.Pos)
            .Select(f => new { f.YearId, f.TeamId, f.LgId, f.Pos, Games = f.G ?? 0, f.Po, f.A, f.E, f.Dp })
            .ToListAsync();

        // Po/A/E/Dp are stored as strings and can be empty (not just null) for older
        // Lahman rows, so parse defensively in memory rather than int.Parse in the query.
        var seasons = rows
            .Select(f => new SeasonFieldingDto(
                f.YearId, f.TeamId, f.LgId, f.Pos,
                f.Games,
                ParseIntOrZero(f.Po),
                ParseIntOrZero(f.A),
                ParseIntOrZero(f.E),
                ParseIntOrZero(f.Dp)))
            .ToList();

        return Results.Ok(seasons);
    }

    private static int ParseIntOrZero(string? value) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;

    private static async Task<IResult> GetPlayerAwards(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var awards = await context.AwardsPlayers
            .Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.YearId)
            .Select(a => new PlayerAwardDto(a.YearId, a.AwardId, a.LgId, a.Notes))
            .ToListAsync();

        return Results.Ok(awards);
    }

    private static async Task<IResult> GetPlayerPostseasonBatting(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var stats = await context.BattingPost
            .Where(b => b.PlayerId == playerId)
            .OrderByDescending(b => b.YearId)
            .ThenBy(b => b.Round)
            .Select(b => new PostseasonBattingDto(
                b.YearId, b.Round, b.TeamId, b.LgId,
                b.G ?? 0, b.Ab ?? 0, b.H ?? 0, b.Hr ?? 0, b.Rbi ?? 0,
                (b.Ab ?? 0) > 0 ? (double)(b.H ?? 0) / (b.Ab ?? 0) : 0))
            .ToListAsync();

        return Results.Ok(stats);
    }

    private static async Task<IResult> GetPlayerPostseasonPitching(string playerId, BaseballDbContext context)
    {
        if (!await context.People.AnyAsync(p => p.PlayerId == playerId))
            return Results.NotFound();

        var stats = await context.PitchingPost
            .Where(p => p.PlayerId == playerId)
            .OrderByDescending(p => p.YearId)
            .ThenBy(p => p.Round)
            .Select(p => new PostseasonPitchingDto(
                p.YearId, p.Round, p.TeamId, p.LgId,
                p.G ?? 0, p.W ?? 0, p.L ?? 0, p.Sv ?? 0,
                (p.Ipouts ?? 0) / 3.0, p.So ?? 0,
                (p.Ipouts ?? 0) > 0 ? ((p.Er ?? 0) * 27.0) / (p.Ipouts ?? 0) : 0))
            .ToListAsync();

        return Results.Ok(stats);
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
}
