using System.Globalization;

namespace baseball_history_web.Services;

/// <summary>
/// Several Lahman numeric columns (fielding PO/A/E/DP, postseason SO/CS) are
/// stored as strings and can be empty rather than null in older rows, so they
/// must be parsed defensively in memory, never cast inside an EF query.
/// </summary>
public static class LahmanNumbers
{
    public static int ParseIntOrZero(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
}
