using baseball_history_mcp.Configuration;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_mcp.Querying;

public sealed class PlayerReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService,
    BaseballMcpRequestPolicy requestPolicy) : IPlayerReadService
{
    public async Task<PagedReadResult<PlayerLookupItem>> SearchPlayersAsync(
        PlayerLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = requestPolicy.Normalize(request);

        if (normalizedRequest.Query is not null && normalizedRequest.LastNameStartsWith is not null)
        {
            throw new BaseballMcpUsageException("Provide either query or lastNameStartsWith, not both.");
        }

        McpInputValidation.ValidatePage(request.Page);
        McpInputValidation.ValidatePageSize(request.PageSize);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.People
            .Where(p => p.NameLast != null)
            .AsQueryable();

        if (normalizedQuery is not null)
        {
            var terms = normalizedQuery
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var term in terms)
            {
                var pattern = $"%{term}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.PlayerId, pattern) ||
                    (p.NameFirst != null && EF.Functions.ILike(p.NameFirst, pattern)) ||
                    (p.NameLast != null && EF.Functions.ILike(p.NameLast, pattern)) ||
                    EF.Functions.ILike((p.NameFirst ?? string.Empty) + " " + (p.NameLast ?? string.Empty), pattern));
            }
        }
        else if (normalizedRequest.LastNameStartsWith is not null)
        {
            query = query.Where(p => p.NameLast != null && EF.Functions.ILike(p.NameLast, $"{normalizedRequest.LastNameStartsWith}%"));
        }

        query = query
            .OrderBy(p => p.NameLast)
            .ThenBy(p => p.NameFirst)
            .ThenBy(p => p.PlayerId);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageWindow = requestPolicy.CreatePlayerLookupWindow(normalizedRequest, totalCount);
        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        var players = await query
            .Skip((pageWindow.Page - 1) * pageWindow.PageSize)
            .Take(pageWindow.PageSize)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst,
                p.NameLast,
                DebutYear = p.Debut,
                FinalYear = p.FinalGame,
                TotalGames = (p.Battings.Sum(b => (int?)b.G) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.G) ?? 0),
                TotalHits = p.Battings.Sum(b => (int?)b.H) ?? 0,
                TotalHomeRuns = p.Battings.Sum(b => (int?)b.Hr) ?? 0,
                LastTeam = p.Battings.OrderByDescending(b => b.YearId).Select(b => b.TeamId).FirstOrDefault()
                    ?? p.Pitchings.OrderByDescending(pi => pi.YearId).Select(pi => pi.TeamId).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return pageWindow.CreateResult(
            players.Select(p => new PlayerLookupItem(
                p.PlayerId,
                FormatName(p.NameFirst, p.NameLast, p.PlayerId),
                p.DebutYear?.Year,
                p.FinalYear?.Year,
                hallOfFamers.Contains(p.PlayerId),
                p.TotalGames > 0 ? p.TotalGames : null,
                p.TotalHits > 0 ? p.TotalHits : null,
                p.TotalHomeRuns > 0 ? p.TotalHomeRuns : null,
                p.LastTeam))
            .ToList(),
            totalCount);
    }

    public async Task<PlayerReadModel?> GetPlayerAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var normalizedPlayerId = requestPolicy.NormalizeRequiredId(playerId, "playerId").ToLowerInvariant();

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var person = await context.People
            .FirstOrDefaultAsync(p => p.PlayerId == normalizedPlayerId, cancellationToken);

        if (person is null)
        {
            return null;
        }

        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);
        var isHallOfFamer = hallOfFamers.Contains(normalizedPlayerId);

        int? hallOfFameInductionYear = null;
        if (isHallOfFamer)
        {
            hallOfFameInductionYear = await context.HallOfFame
                .Where(h => h.PlayerId == normalizedPlayerId && h.Inducted == "Y")
                .OrderBy(h => h.Yearid)
                .Select(h => (int?)h.Yearid)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var battingAggregate = await context.Batting
            .Where(b => b.PlayerId == normalizedPlayerId)
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                Games = g.Sum(b => b.G ?? 0),
                AtBats = g.Sum(b => b.Ab ?? 0),
                Runs = g.Sum(b => b.R ?? 0),
                Hits = g.Sum(b => b.H ?? 0),
                Doubles = g.Sum(b => b._2b ?? 0),
                Triples = g.Sum(b => b._3b ?? 0),
                HomeRuns = g.Sum(b => b.Hr ?? 0),
                Walks = g.Sum(b => b.Bb ?? 0),
                Rbi = g.Sum(b => b.Rbi ?? 0),
                StolenBases = g.Sum(b => b.Sb ?? 0),
                Strikeouts = g.Sum(b => b.So ?? 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        PlayerCareerBattingSummary? careerBatting = null;
        if (battingAggregate is { AtBats: > 0 })
        {
            var average = (double)battingAggregate.Hits / battingAggregate.AtBats;
            var onBasePercentage = (battingAggregate.AtBats + battingAggregate.Walks) > 0
                ? (double)(battingAggregate.Hits + battingAggregate.Walks) / (battingAggregate.AtBats + battingAggregate.Walks)
                : 0;
            var totalBases = battingAggregate.Hits + battingAggregate.Doubles + (2 * battingAggregate.Triples) + (3 * battingAggregate.HomeRuns);
            var slugging = (double)totalBases / battingAggregate.AtBats;

            careerBatting = new PlayerCareerBattingSummary(
                battingAggregate.Games,
                battingAggregate.AtBats,
                battingAggregate.Runs,
                battingAggregate.Hits,
                battingAggregate.Doubles,
                battingAggregate.Triples,
                battingAggregate.HomeRuns,
                battingAggregate.Rbi,
                battingAggregate.StolenBases,
                battingAggregate.Walks,
                battingAggregate.Strikeouts,
                Math.Round(average, 3),
                Math.Round(onBasePercentage, 3),
                Math.Round(slugging, 3),
                Math.Round(onBasePercentage + slugging, 3));
        }

        var pitchingAggregate = await context.Pitching
            .Where(p => p.PlayerId == normalizedPlayerId)
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
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
            .FirstOrDefaultAsync(cancellationToken);

        PlayerCareerPitchingSummary? careerPitching = null;
        if (pitchingAggregate is { Games: > 0 })
        {
            var inningsPitched = pitchingAggregate.InningsPitchedOuts / 3.0;
            var era = inningsPitched > 0 ? pitchingAggregate.EarnedRuns * 9.0 / inningsPitched : 0;
            var whip = inningsPitched > 0 ? (pitchingAggregate.Walks + pitchingAggregate.Hits) / inningsPitched : 0;

            careerPitching = new PlayerCareerPitchingSummary(
                pitchingAggregate.Games,
                pitchingAggregate.GamesStarted,
                pitchingAggregate.Wins,
                pitchingAggregate.Losses,
                pitchingAggregate.Saves,
                pitchingAggregate.CompleteGames,
                pitchingAggregate.Shutouts,
                Math.Round(inningsPitched, 1),
                pitchingAggregate.Hits,
                pitchingAggregate.EarnedRuns,
                pitchingAggregate.HomeRuns,
                pitchingAggregate.Walks,
                pitchingAggregate.Strikeouts,
                Math.Round(era, 2),
                Math.Round(whip, 3));
        }

        var battingTeams = await context.Batting
            .Where(b => b.PlayerId == normalizedPlayerId)
            .GroupBy(b => b.TeamId)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(b => b.YearId),
                LastYear = g.Max(b => b.YearId),
                Seasons = g.Select(b => b.YearId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var existingTeamIds = battingTeams.Select(t => t.TeamId).ToHashSet();
        var pitchingTeams = await context.Pitching
            .Where(p => p.PlayerId == normalizedPlayerId && p.TeamId != null && !existingTeamIds.Contains(p.TeamId))
            .GroupBy(p => p.TeamId!)
            .Select(g => new
            {
                TeamId = g.Key,
                FirstYear = g.Min(p => p.YearId),
                LastYear = g.Max(p => p.YearId),
                Seasons = g.Select(p => p.YearId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var allTeams = battingTeams
            .Concat(pitchingTeams)
            .OrderBy(t => t.FirstYear)
            .ToList();

        var teamIds = allTeams.Select(t => t.TeamId).Distinct().ToList();
        var teamNames = await context.Teams
            .Where(t => teamIds.Contains(t.TeamId))
            .GroupBy(t => t.TeamId)
            .Select(g => new { TeamId = g.Key, Name = g.OrderByDescending(t => t.YearId).First().Name })
            .ToDictionaryAsync(t => t.TeamId, t => t.Name, cancellationToken);

        var teams = allTeams.Select(t => new PlayerTeamTenure(
            t.TeamId,
            teamNames.GetValueOrDefault(t.TeamId),
            t.FirstYear,
            t.LastYear,
            t.Seasons)).ToList();

        return new PlayerReadModel(
            person.PlayerId,
            FormatName(person.NameFirst, person.NameLast, person.PlayerId),
            person.NameGiven,
            person.Height,
            person.Weight,
            person.Bats,
            person.Throws,
            person.Debut?.Year,
            person.FinalGame?.Year,
            isHallOfFamer,
            hallOfFameInductionYear,
            careerBatting,
            careerPitching,
            teams);
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
