using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_web.Services;

/// <summary>
/// "This Day in Baseball": birthdays, debuts and final games for one
/// month/day. Shared by the home page widget and /api/this-day.
/// </summary>
public class ThisDayService(BaseballDbContext context, IMemoryCache cache)
{
    public const int DefaultEntriesPerCategory = 5;
    public const int MaxEntriesPerCategory = 25;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    /// <summary>
    /// Cached per month/day/limit so the widget rotates daily while each
    /// answer is computed at most once a day.
    /// </summary>
    public async Task<ThisDayViewModel> GetAsync(int month, int day, int entriesPerCategory = DefaultEntriesPerCategory)
    {
        var cacheKey = entriesPerCategory == DefaultEntriesPerCategory
            ? $"this_day_{month}_{day}"
            : $"this_day_{month}_{day}_{entriesPerCategory}";

        return (await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await LoadAsync(month, day, entriesPerCategory);
        }))!;
    }

    private async Task<ThisDayViewModel> LoadAsync(int month, int day, int entriesPerCategory)
    {
        var hofPlayerIds = await HallOfFameIds.GetAsync(context, cache);

        // birthMonth/birthDay are unpadded numeric strings in the Lahman data
        var monthText = month.ToString();
        var dayText = day.ToString();

        // debut/finalGame are varchar columns behind a DateOnly converter, so
        // Month/Day never translate to SQL; equality against concrete dates
        // does, one candidate per season year
        var candidateDates = new List<DateOnly?>();
        for (var candidateYear = 1871; candidateYear <= DateTime.Now.Year; candidateYear++)
        {
            if (day <= DateTime.DaysInMonth(candidateYear, month))
            {
                candidateDates.Add(new DateOnly(candidateYear, month, day));
            }
        }

        var birthdays = await context.People
            .Where(p => p.BirthMonth == monthText && p.BirthDay == dayText)
            .Select(p => new
            {
                p.PlayerId,
                FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
                p.BirthYear
            })
            .ToListAsync();

        var debuts = await context.People
            .Where(p => p.Debut != null && candidateDates.Contains(p.Debut))
            .Select(p => new
            {
                p.PlayerId,
                FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
                Date = p.Debut
            })
            .ToListAsync();

        var finales = await context.People
            .Where(p => p.FinalGame != null && candidateDates.Contains(p.FinalGame))
            .Select(p => new
            {
                p.PlayerId,
                FullName = (p.NameFirst ?? "") + " " + (p.NameLast ?? ""),
                Date = p.FinalGame
            })
            .ToListAsync();

        List<ThisDayEntry> Rank(IEnumerable<ThisDayEntry> entries) => entries
            .OrderByDescending(e => e.IsInHallOfFame)
            .ThenByDescending(e => e.Year)
            .ThenBy(e => e.FullName)
            .Take(entriesPerCategory)
            .ToList();

        return new ThisDayViewModel
        {
            Month = month,
            Day = day,
            Birthdays = Rank(birthdays.Select(b => new ThisDayEntry
            {
                PlayerId = b.PlayerId,
                FullName = b.FullName.Trim(),
                Year = int.TryParse(b.BirthYear, out var birthYear) ? birthYear : null,
                IsInHallOfFame = hofPlayerIds.Contains(b.PlayerId)
            })),
            Debuts = Rank(debuts.Select(d => new ThisDayEntry
            {
                PlayerId = d.PlayerId,
                FullName = d.FullName.Trim(),
                Year = d.Date?.Year,
                IsInHallOfFame = hofPlayerIds.Contains(d.PlayerId)
            })),
            Finales = Rank(finales.Select(f => new ThisDayEntry
            {
                PlayerId = f.PlayerId,
                FullName = f.FullName.Trim(),
                Year = f.Date?.Year,
                IsInHallOfFame = hofPlayerIds.Contains(f.PlayerId)
            }))
        };
    }
}
