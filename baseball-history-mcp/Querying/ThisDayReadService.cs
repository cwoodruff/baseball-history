using System.Globalization;
using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace baseball_history_mcp.Querying;

/// <summary>
/// "This Day in Baseball": birthdays, debuts and final games on one calendar
/// day, ranked Hall of Famers first and then most recent, matching the
/// companion site's home page widget.
/// </summary>
public sealed class ThisDayReadService(
    IDbContextFactory<BaseballDbContext> contextFactory,
    IHallOfFameReadService hallOfFameReadService,
    IMemoryCache cache) : IThisDayReadService
{
    public const int MaxEntriesPerCategory = 25;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public async Task<ThisDayReadModel> GetThisDayAsync(
        string? date,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var (month, day) = ParseDate(date);
        if (limit is < 1 or > MaxEntriesPerCategory)
        {
            McpInputValidation.ThrowInvalid($"limit must be between 1 and {MaxEntriesPerCategory}.");
        }

        return (await cache.GetOrCreateAsync($"baseball-history-mcp:this-day:{month}-{day}:{limit}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await LoadAsync(month, day, limit, cancellationToken);
        }))!;
    }

    private static (int Month, int Day) ParseDate(string? date)
    {
        var normalized = McpInputValidation.NormalizeOptionalText(date);
        if (normalized is null)
        {
            var today = DateTime.Now;
            return (today.Month, today.Day);
        }

        var parts = normalized.Split('-');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var month) && month is >= 1 and <= 12 &&
            int.TryParse(parts[1], out var day) && day >= 1 &&
            day <= DateTime.DaysInMonth(2000, month)) // 2000 is a leap year, so Feb 29 is valid
        {
            return (month, day);
        }

        McpInputValidation.ThrowInvalid("date must be MM-DD, for example 02-06.");
        return default;
    }

    private async Task<ThisDayReadModel> LoadAsync(int month, int day, int limit, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var hallOfFamers = await hallOfFameReadService.GetInductedPlayerIdsAsync(cancellationToken);

        // birthMonth/birthDay are unpadded numeric strings in the Lahman data
        var monthText = month.ToString(CultureInfo.InvariantCulture);
        var dayText = day.ToString(CultureInfo.InvariantCulture);

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
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, p.BirthYear })
            .ToListAsync(cancellationToken);

        var debuts = await context.People
            .Where(p => p.Debut != null && candidateDates.Contains(p.Debut))
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, Date = p.Debut })
            .ToListAsync(cancellationToken);

        var finales = await context.People
            .Where(p => p.FinalGame != null && candidateDates.Contains(p.FinalGame))
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast, Date = p.FinalGame })
            .ToListAsync(cancellationToken);

        IReadOnlyList<ThisDayEntryReadModel> Rank(IEnumerable<ThisDayEntryReadModel> entries) => entries
            .OrderByDescending(e => e.IsInHallOfFame)
            .ThenByDescending(e => e.Year)
            .ThenBy(e => e.FullName)
            .Take(limit)
            .ToList();

        return new ThisDayReadModel(
            month,
            day,
            $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)} {day}",
            limit,
            Rank(birthdays.Select(b => new ThisDayEntryReadModel(
                b.PlayerId,
                FormatName(b.NameFirst, b.NameLast, b.PlayerId),
                int.TryParse(b.BirthYear, out var birthYear) ? birthYear : null,
                hallOfFamers.Contains(b.PlayerId)))),
            Rank(debuts.Select(d => new ThisDayEntryReadModel(
                d.PlayerId,
                FormatName(d.NameFirst, d.NameLast, d.PlayerId),
                d.Date?.Year,
                hallOfFamers.Contains(d.PlayerId)))),
            Rank(finales.Select(f => new ThisDayEntryReadModel(
                f.PlayerId,
                FormatName(f.NameFirst, f.NameLast, f.PlayerId),
                f.Date?.Year,
                hallOfFamers.Contains(f.PlayerId)))));
    }

    private static string FormatName(string? firstName, string? lastName, string fallback) =>
        string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim() switch
        {
            { Length: > 0 } fullName => fullName,
            _ => fallback
        };
}
