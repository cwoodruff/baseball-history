using System.Globalization;

namespace baseball_history_web.ViewModels;

/// <summary>
/// "This day in baseball" home widget: birthdays, debuts, and final games
/// matching one month/day
/// </summary>
public class ThisDayViewModel
{
    public int Month { get; set; }
    public int Day { get; set; }

    public List<ThisDayEntry> Birthdays { get; set; } = new();
    public List<ThisDayEntry> Debuts { get; set; } = new();
    public List<ThisDayEntry> Finales { get; set; } = new();

    public bool HasContent => Birthdays.Count > 0 || Debuts.Count > 0 || Finales.Count > 0;

    /// <summary>e.g. "February 6" (year-independent; leap-day safe)</summary>
    public string DateLabel =>
        $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month)} {Day}";

    /// <summary>
    /// Parses an optional "MM-DD" override; falls back to the given date's
    /// month/day when absent or malformed.
    /// </summary>
    public static (int Month, int Day) ResolveDay(string? overrideDay, DateOnly today)
    {
        if (!string.IsNullOrEmpty(overrideDay))
        {
            var parts = overrideDay.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var month) && month is >= 1 and <= 12 &&
                int.TryParse(parts[1], out var day) && day >= 1 &&
                day <= DateTime.DaysInMonth(2000, month)) // 2000 is a leap year, so Feb 29 is valid
            {
                return (month, day);
            }
        }

        return (today.Month, today.Day);
    }
}

/// <summary>
/// One player line in the this-day widget
/// </summary>
public class ThisDayEntry
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int? Year { get; set; }
    public bool IsInHallOfFame { get; set; }
}
