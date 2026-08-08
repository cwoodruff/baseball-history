using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using BaseballHistory.Data.Querying;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace baseball_history_web.Pages.Compare;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public CompareViewModel ViewModel { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Player1 { get; set; }
    [BindProperty(SupportsGet = true)] public string? Player2 { get; set; }
    [BindProperty(SupportsGet = true)] public int? FromYear { get; set; }
    [BindProperty(SupportsGet = true)] public int? ToYear { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var hofPlayerIds = await GetHofPlayerIds();

        if (!string.IsNullOrWhiteSpace(Player1))
            ViewModel.Player1 = await LoadPlayer(Player1.Trim(), hofPlayerIds);

        if (!string.IsNullOrWhiteSpace(Player2))
            ViewModel.Player2 = await LoadPlayer(Player2.Trim(), hofPlayerIds);

        // Return partial view for htmx requests
        if (Request.IsHtmxNonBoostedRequest())
        {
            return Partial("_CompareMain", ViewModel);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetSearchAsync(string? q, int side = 1)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Partial("_CompareSearchResults", (new List<PlayerSummary>(), side, Player1, Player2));

        var searchTerm = q.Trim().ToLower();

        var players = await context.People
            .Where(p =>
                (p.NameFirst != null && p.NameFirst.ToLower().Contains(searchTerm)) ||
                (p.NameLast != null && p.NameLast.ToLower().Contains(searchTerm)) ||
                (p.NameFirst != null && p.NameLast != null &&
                 (p.NameFirst.ToLower() + " " + p.NameLast.ToLower()).Contains(searchTerm)))
            .OrderByDescending(p => (p.Battings.Sum(b => (int?)b.Hr) ?? 0) + (p.Pitchings.Sum(pi => (int?)pi.W) ?? 0))
            .ThenBy(p => p.NameLast)
            .Take(8)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, p.Debut, p.FinalGame })
            .ToListAsync();

        var hofPlayerIds = await GetHofPlayerIds();

        var results = players.Select(p => new PlayerSummary
        {
            PlayerId = p.PlayerId,
            FirstName = p.NameFirst,
            LastName = p.NameLast,
            FullName = $"{p.NameFirst} {p.NameLast}".Trim(),
            DebutYear = p.Debut?.Year.ToString(),
            FinalYear = p.FinalGame?.Year.ToString(),
            IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId)
        }).ToList();

        return Partial("_CompareSearchResults", (results, side, Player1, Player2));
    }

    private async Task<ComparePlayer?> LoadPlayer(string playerId, HashSet<string> hofPlayerIds)
    {
        // Projection-first: only load needed fields from People
        var person = await context.People
            .Where(p => p.PlayerId == playerId)
            .Select(p => new
            {
                p.PlayerId,
                p.NameFirst,
                p.NameLast,
                p.Bats,
                p.Throws,
                p.Debut,
                p.FinalGame,
                p.BirthYear
            })
            .FirstOrDefaultAsync();
        
        if (person == null) return null;

        var player = new ComparePlayer
        {
            PlayerId = person.PlayerId,
            FullName = $"{person.NameFirst} {person.NameLast}".Trim(),
            Bats = person.Bats,
            Throws = person.Throws,
            Debut = person.Debut?.Year.ToString(),
            FinalGame = person.FinalGame?.Year.ToString(),
            IsInHallOfFame = hofPlayerIds.Contains(playerId)
        };

        if (person.Debut.HasValue && person.FinalGame.HasValue)
            player.CareerYears = person.FinalGame.Value.Year - person.Debut.Value.Year + 1;

        if (!string.IsNullOrEmpty(person.BirthYear))
            player.BirthDate = person.BirthYear;

        if (player.IsInHallOfFame)
        {
            player.HofInductionYear = await context.HallOfFame
                .Where(h => h.PlayerId == playerId && h.Inducted == "Y")
                .OrderBy(h => h.Yearid)
                .Select(h => (int?)h.Yearid)
                .FirstOrDefaultAsync();
        }

        // Build batting query with optional year-range filter
        var battingQuery = context.Batting.Where(b => b.PlayerId == playerId);
        if (FromYear.HasValue)
            battingQuery = battingQuery.Where(b => b.YearId >= FromYear.Value);
        if (ToYear.HasValue)
            battingQuery = battingQuery.Where(b => b.YearId <= ToYear.Value);

        // Career batting aggregation (with year-range filter if specified)
        var battingAgg = await battingQuery
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                G = g.Sum(b => b.G ?? 0), AB = g.Sum(b => b.Ab ?? 0),
                R = g.Sum(b => b.R ?? 0), H = g.Sum(b => b.H ?? 0),
                Doubles = g.Sum(b => b._2b ?? 0), Triples = g.Sum(b => b._3b ?? 0),
                HR = g.Sum(b => b.Hr ?? 0), BB = g.Sum(b => b.Bb ?? 0),
                RBI = g.Sum(b => b.Rbi ?? 0), SB = g.Sum(b => b.Sb ?? 0),
                SO = g.Sum(b => b.So ?? 0)
            })
            .FirstOrDefaultAsync();

        if (battingAgg is { AB: > 0 })
        {
            player.BattingStats = new CompareCareerBattingStats
            {
                Games = battingAgg.G, AtBats = battingAgg.AB, Runs = battingAgg.R,
                Hits = battingAgg.H, Doubles = battingAgg.Doubles, Triples = battingAgg.Triples,
                HomeRuns = battingAgg.HR, Walks = battingAgg.BB, Rbi = battingAgg.RBI,
                StolenBases = battingAgg.SB, Strikeouts = battingAgg.SO
            };
        }

        // Compute qualified batting seasons (full career, not year-range-filtered)
        var battingSeasonStats = await context.Batting
            .Where(b => b.PlayerId == playerId)
            .GroupBy(b => new { b.PlayerId, b.YearId })
            .Select(g => new
            {
                YearId = g.Key.YearId,
                AB = g.Sum(b => b.Ab ?? 0),
                BB = g.Sum(b => b.Bb ?? 0),
                HBP = g.Sum(b => b.Hbp),
                SH = g.Sum(b => b.Sh),
                SF = g.Sum(b => b.Sf),
                // Take max Team.G for the year (should all be same per year, but use max to be safe)
                TeamGames = g.Max(b => b.Team != null && b.Team.G.HasValue ? (int)b.Team.G.Value : 0)
            })
            .ToListAsync();

        if (battingSeasonStats.Any())
        {
            player.TotalBattingSeasons = battingSeasonStats.Count;
            player.QualifiedBattingSeasons = battingSeasonStats.Count(s =>
            {
                var pa = QualificationRules.CalculatePlateAppearances(s.AB, s.BB, s.HBP, s.SH, s.SF);
                var threshold = Math.Max(100, QualificationRules.CalculateSeasonBattingThreshold(s.TeamGames));
                return pa >= threshold;
            });
        }

        // Build pitching query with optional year-range filter
        var pitchingQuery = context.Pitching.Where(p => p.PlayerId == playerId);
        if (FromYear.HasValue)
            pitchingQuery = pitchingQuery.Where(p => p.YearId >= FromYear.Value);
        if (ToYear.HasValue)
            pitchingQuery = pitchingQuery.Where(p => p.YearId <= ToYear.Value);

        // Career pitching aggregation (with year-range filter if specified)
        var pitchingAgg = await pitchingQuery
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                G = g.Sum(p => p.G ?? 0), GS = g.Sum(p => p.Gs ?? 0),
                W = g.Sum(p => p.W ?? 0), L = g.Sum(p => p.L ?? 0),
                SV = g.Sum(p => p.Sv ?? 0), CG = g.Sum(p => p.Cg ?? 0),
                SHO = g.Sum(p => p.Sho ?? 0), IPOuts = g.Sum(p => p.Ipouts ?? 0),
                H = g.Sum(p => p.H ?? 0), ER = g.Sum(p => p.Er ?? 0),
                HR = g.Sum(p => p.Hr ?? 0), BB = g.Sum(p => p.Bb ?? 0),
                SO = g.Sum(p => p.So ?? 0)
            })
            .FirstOrDefaultAsync();

        if (pitchingAgg is { G: > 0 })
        {
            player.PitchingStats = new CompareCareerPitchingStats
            {
                Games = pitchingAgg.G, GamesStarted = pitchingAgg.GS,
                Wins = pitchingAgg.W, Losses = pitchingAgg.L, Saves = pitchingAgg.SV,
                CompleteGames = pitchingAgg.CG, Shutouts = pitchingAgg.SHO,
                InningsPitched = pitchingAgg.IPOuts / 3.0,
                Hits = pitchingAgg.H, EarnedRuns = pitchingAgg.ER, HomeRuns = pitchingAgg.HR,
                Walks = pitchingAgg.BB, Strikeouts = pitchingAgg.SO
            };
        }

        var postseasonBattingAgg = await context.BattingPost
            .Where(b => b.PlayerId == playerId)
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                G = g.Sum(b => b.G ?? 0),
                AB = g.Sum(b => b.Ab ?? 0),
                H = g.Sum(b => b.H ?? 0),
                HR = g.Sum(b => b.Hr ?? 0),
                RBI = g.Sum(b => b.Rbi ?? 0)
            })
            .FirstOrDefaultAsync();

        if (postseasonBattingAgg is { AB: > 0 })
        {
            player.PostseasonBattingStats = new ComparePostseasonBattingStats
            {
                Games = postseasonBattingAgg.G,
                AtBats = postseasonBattingAgg.AB,
                Hits = postseasonBattingAgg.H,
                HomeRuns = postseasonBattingAgg.HR,
                Rbi = postseasonBattingAgg.RBI
            };
        }

        var postseasonPitchingAgg = await context.PitchingPost
            .Where(p => p.PlayerId == playerId)
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                G = g.Sum(p => p.G ?? 0),
                W = g.Sum(p => p.W ?? 0),
                L = g.Sum(p => p.L ?? 0),
                SV = g.Sum(p => p.Sv ?? 0),
                IPOuts = g.Sum(p => p.Ipouts ?? 0),
                ER = g.Sum(p => p.Er ?? 0),
                SO = g.Sum(p => p.So ?? 0)
            })
            .FirstOrDefaultAsync();

        if (postseasonPitchingAgg is { IPOuts: > 0 })
        {
            player.PostseasonPitchingStats = new ComparePostseasonPitchingStats
            {
                Games = postseasonPitchingAgg.G,
                Wins = postseasonPitchingAgg.W,
                Losses = postseasonPitchingAgg.L,
                Saves = postseasonPitchingAgg.SV,
                InningsPitched = postseasonPitchingAgg.IPOuts / 3.0,
                EarnedRuns = postseasonPitchingAgg.ER,
                Strikeouts = postseasonPitchingAgg.SO
            };
        }

        var fieldingRows = await context.Fielding
            .Where(f => f.PlayerId == playerId)
            .Select(f => new { f.Pos, Games = f.G ?? 0, f.Po, f.A, f.E, f.Dp })
            .ToListAsync();

        var primaryFielding = fieldingRows
            .GroupBy(f => f.Pos)
            .Select(g => new
            {
                Position = g.Key,
                Games = g.Sum(f => f.Games),
                Putouts = g.Sum(f => ParseIntOrZero(f.Po)),
                Assists = g.Sum(f => ParseIntOrZero(f.A)),
                Errors = g.Sum(f => ParseIntOrZero(f.E)),
                DoublePlays = g.Sum(f => ParseIntOrZero(f.Dp))
            })
            .OrderByDescending(g => g.Games)
            .ThenBy(g => g.Position)
            .FirstOrDefault();

        if (primaryFielding is { Games: > 0 })
        {
            player.FieldingStats = new CompareFieldingStats
            {
                PrimaryPosition = primaryFielding.Position,
                Games = primaryFielding.Games,
                Putouts = primaryFielding.Putouts,
                Assists = primaryFielding.Assists,
                Errors = primaryFielding.Errors,
                DoublePlays = primaryFielding.DoublePlays
            };
        }

        // Compute qualified pitching seasons (full career, not year-range-filtered)
        var pitchingSeasonStats = await context.Pitching
            .Where(p => p.PlayerId == playerId)
            .GroupBy(p => new { p.PlayerId, p.YearId })
            .Select(g => new
            {
                YearId = g.Key.YearId,
                IPouts = g.Sum(p => p.Ipouts ?? 0),
                // Take max Team.G for the year (should all be same per year, but use max to be safe)
                TeamGames = g.Max(p => p.Team != null && p.Team.G.HasValue ? (int)p.Team.G.Value : 0)
            })
            .ToListAsync();

        if (pitchingSeasonStats.Any())
        {
            player.TotalPitchingSeasons = pitchingSeasonStats.Count;
            player.QualifiedPitchingSeasons = pitchingSeasonStats.Count(s =>
            {
                var threshold = Math.Max(90, QualificationRules.CalculateSeasonPitchingThreshold(s.TeamGames));
                return s.IPouts >= threshold;
            });
        }

        // Awards summary
        var awards = await context.AwardsPlayers
            .Where(a => a.PlayerId == playerId)
            .Select(a => a.AwardId)
            .ToListAsync();

        player.AwardCount = awards.Count;
        player.MvpCount = awards.Count(a => a.Contains("MVP"));
        player.GoldGloveCount = awards.Count(a => a.Contains("Gold Glove"));
        player.SilverSluggerCount = awards.Count(a => a.Contains("Silver Slugger"));

        player.AllStarCount = await context.AllstarFull
            .Where(a => a.PlayerId == playerId)
            .Select(a => a.YearId)
            .Distinct()
            .CountAsync();

        var teamIds = await context.Batting
            .Where(b => b.PlayerId == playerId)
            .Select(b => b.TeamId)
            .Union(context.Pitching.Where(p => p.PlayerId == playerId && p.TeamId != null).Select(p => p.TeamId!))
            .Distinct()
            .ToListAsync();

        var teamNames = await context.Teams
            .Where(t => teamIds.Contains(t.TeamId))
            .GroupBy(t => t.TeamId)
            .Select(g => g.OrderByDescending(t => t.YearId).First().Name)
            .ToListAsync();

        player.TeamNames = teamNames.Where(n => n != null).Select(n => n!).Distinct().ToList();

        return player;
    }

    private async Task<HashSet<string>> GetHofPlayerIds()
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

    private static int ParseIntOrZero(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
}
