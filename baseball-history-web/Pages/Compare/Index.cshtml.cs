using baseball_history_web.Extensions;
using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Pages.Compare;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class IndexModel(BaseballDbContext context, IMemoryCache cache) : PageModel
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public CompareViewModel ViewModel { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Player1 { get; set; }
    [BindProperty(SupportsGet = true)] public string? Player2 { get; set; }

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

        // Career batting
        var battingAgg = await context.Batting
            .Where(b => b.PlayerId == playerId)
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
            player.BattingStats = new CareerBattingStats
            {
                Games = battingAgg.G, AtBats = battingAgg.AB, Runs = battingAgg.R,
                Hits = battingAgg.H, Doubles = battingAgg.Doubles, Triples = battingAgg.Triples,
                HomeRuns = battingAgg.HR, Walks = battingAgg.BB, Rbi = battingAgg.RBI,
                StolenBases = battingAgg.SB, Strikeouts = battingAgg.SO
            };
        }

        // Career pitching
        var pitchingAgg = await context.Pitching
            .Where(p => p.PlayerId == playerId)
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
            player.PitchingStats = new CareerPitchingStats
            {
                Games = pitchingAgg.G, GamesStarted = pitchingAgg.GS,
                Wins = pitchingAgg.W, Losses = pitchingAgg.L, Saves = pitchingAgg.SV,
                CompleteGames = pitchingAgg.CG, Shutouts = pitchingAgg.SHO,
                InningsPitched = pitchingAgg.IPOuts / 3.0,
                Hits = pitchingAgg.H, EarnedRuns = pitchingAgg.ER, HomeRuns = pitchingAgg.HR,
                Walks = pitchingAgg.BB, Strikeouts = pitchingAgg.SO
            };
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
}
