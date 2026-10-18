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
        int? month = null,
        int? day = null,
        CancellationToken cancellationToken = default)
    {
        (month, day) = ResolveDay(date, month, day);
        if (limit is < 1 or > MaxEntriesPerCategory)
        {
            McpInputValidation.ThrowInvalid($"limit must be between 1 and {MaxEntriesPerCategory}.");
        }

        return (await cache.GetOrCreateAsync($"baseball-history-mcp:this-day:{month}-{day}:{limit}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await LoadAsync(month.Value, day.Value, limit, cancellationToken);
        }))!;
    }

    /// <summary>The day comes from date (MM-DD) or month + day, never both; neither means today</summary>
    private static (int Month, int Day) ResolveDay(string? date, int? month, int? day)
    {
        var normalized = McpInputValidation.NormalizeOptionalText(date);
        if (month is not null || day is not null)
        {
            if (normalized is not null)
            {
                McpInputValidation.ThrowInvalid("Pass either date (MM-DD) or month and day, not both.");
            }

            if (month is null || day is null)
            {
                McpInputValidation.ThrowInvalid("month and day must be passed together.");
            }

            if (!IsValidDay(month!.Value, day!.Value))
            {
                McpInputValidation.ThrowInvalid("month must be 1 to 12 and day must exist in that month.");
            }

            return (month.Value, day.Value);
        }

        if (normalized is null)
        {
            var today = DateTime.Now;
            return (today.Month, today.Day);
        }

        var parts = normalized.Split('-');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var parsedMonth) &&
            int.TryParse(parts[1], out var parsedDay) &&
            IsValidDay(parsedMonth, parsedDay))
        {
            return (parsedMonth, parsedDay);
        }

        McpInputValidation.ThrowInvalid("date must be MM-DD, for example 02-06.");
        return default;
    }

    // 2000 is a leap year, so Feb 29 is valid
    private static bool IsValidDay(int month, int day) =>
        month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(2000, month);

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
