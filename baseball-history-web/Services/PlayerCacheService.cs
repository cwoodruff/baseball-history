using baseball_history_web.Models;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Services;

/// <summary>
/// Pre-warms and caches the default Players page (letter A, page 1)
/// so it renders instantly without hitting the database.
/// </summary>
public class PlayerCacheService(IServiceProvider serviceProvider, IMemoryCache cache) : BackgroundService
{
    private const string CacheKey = "players_first_page";
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    public static PlayerListViewModel? GetCachedFirstPage(IMemoryCache cache) =>
        cache.TryGetValue(CacheKey, out PlayerListViewModel? vm) ? vm : null;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Build immediately at startup
        await BuildCache(stoppingToken);

        // Refresh periodically
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshInterval, stoppingToken);
            await BuildCache(stoppingToken);
        }
    }

    private async Task BuildCache(CancellationToken ct)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BaseballDbContext>();

            const int pageSize = 48;
            const char letter = 'A';

            // Get available letters
            var availableLetters = await context.People
                .Where(p => p.NameLast != null && p.NameLast.Length > 0)
                .Select(p => p.NameLast!.Substring(0, 1).ToUpper())
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync(ct);

            var letters = availableLetters
                .Where(l => l.Length == 1 && char.IsLetter(l[0]))
                .Select(l => l[0])
                .ToList();

            // Get HOF player IDs
            var hofPlayerIds = await context.HallOfFame
                .Where(h => h.Inducted == "Y")
                .Select(h => h.PlayerId)
                .Distinct()
                .ToHashSetAsync(ct);

            // Query players for letter A, page 1
            var query = context.People
                .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith(letter.ToString()))
                .OrderBy(p => p.NameLast)
                .ThenBy(p => p.NameFirst);

            var totalPlayers = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)totalPlayers / pageSize);

            var players = await query
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
                .ToListAsync(ct);

            var vm = new PlayerListViewModel
            {
                CurrentLetter = letter.ToString(),
                CurrentPage = 1,
                TotalPages = totalPages,
                TotalPlayers = totalPlayers,
                PageSize = pageSize,
                AvailableLetters = letters,
                Players = players.Select(p => new PlayerSummary
                {
                    PlayerId = p.PlayerId,
                    FirstName = p.NameFirst,
                    LastName = p.NameLast,
                    FullName = $"{p.NameFirst} {p.NameLast}".Trim(),
                    DebutYear = p.DebutYear?.Year.ToString(),
                    FinalYear = p.FinalYear?.Year.ToString(),
                    IsInHallOfFame = hofPlayerIds.Contains(p.PlayerId),
                    TotalGames = p.TotalGames > 0 ? p.TotalGames : null,
                    TotalHits = p.TotalHits > 0 ? p.TotalHits : null,
                    TotalHomeRuns = p.TotalHR > 0 ? p.TotalHR : null,
                    LastTeamId = p.LastTeam
                }).ToList()
            };

            cache.Set(CacheKey, vm, RefreshInterval.Add(TimeSpan.FromMinutes(5)));
        }
        catch (OperationCanceledException)
        {
            // Shutting down, ignore
        }
    }
}
