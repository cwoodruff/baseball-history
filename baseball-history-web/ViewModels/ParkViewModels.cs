namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for the ballpark browser index page
/// </summary>
public class ParkListViewModel
{
    public List<ParkSummary> Parks { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalParks { get; set; }
    public string? SearchQuery { get; set; }
    public string? SelectedState { get; set; }
    public List<string> StateOptions { get; set; } = new();

    public int TotalPages => (int)Math.Ceiling((double)TotalParks / PageSize);
    public bool HasActiveFilters => SearchQuery != null || SelectedState != null;

    public Dictionary<string, string> FilterQueryParams()
    {
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(SearchQuery))
            queryParams["q"] = SearchQuery;
        if (!string.IsNullOrEmpty(SelectedState))
            queryParams["state"] = SelectedState;
        return queryParams;
    }
}

/// <summary>
/// A single ballpark in the browser list
/// </summary>
public class ParkSummary
{
    public string? ParkKey { get; set; }
    public string? ParkName { get; set; }
    public string? Alias { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public short? FirstYear { get; set; }
    public short? LastYear { get; set; }
    public int TeamCount { get; set; }

    public string Location => ParkLocationFormatter.Format(City, State, Country);

    public string YearsActive => ParkLocationFormatter.FormatYears(FirstYear, LastYear);
}

/// <summary>
/// ViewModel for the ballpark detail page
/// </summary>
public class ParkDetailViewModel
{
    public string ParkKey { get; set; } = null!;
    public string? ParkName { get; set; }
    public string? Alias { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    /// <summary>Team tenures at this park, most recent first</summary>
    public List<ParkTenant> Tenants { get; set; } = new();

    /// <summary>Home-game seasons at this park, most recent first</summary>
    public List<ParkSeasonRow> Seasons { get; set; } = new();

    public string Location => ParkLocationFormatter.Format(City, State, Country);

    public IReadOnlyList<string> AliasNames =>
        string.IsNullOrWhiteSpace(Alias)
            ? Array.Empty<string>()
            : Alias.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public short? FirstYear => Seasons.Count > 0 ? Seasons.Min(s => s.Year) : null;
    public short? LastYear => Seasons.Count > 0 ? Seasons.Max(s => s.Year) : null;
    public string YearsActive => ParkLocationFormatter.FormatYears(FirstYear, LastYear);

    public int SeasonCount => Seasons.Select(s => s.Year).Distinct().Count();
    public int TotalGames => Seasons.Sum(s => s.Games ?? 0);
    public long TotalAttendance => Seasons.Sum(s => (long)(s.Attendance ?? 0));
    public bool HasAttendanceData => Seasons.Any(s => s.Attendance is > 0);

    /// <summary>Attendance summed across all tenants per year, oldest first, for charting</summary>
    public IReadOnlyList<ParkYearAttendance> AttendanceByYear =>
        Seasons
            .Where(s => s.Attendance is > 0)
            .GroupBy(s => s.Year)
            .Select(g => new ParkYearAttendance(g.Key, g.Sum(s => (long)(s.Attendance ?? 0))))
            .OrderBy(a => a.Year)
            .ToList();

    public ParkYearAttendance? PeakAttendanceYear =>
        AttendanceByYear.Count > 0
            ? AttendanceByYear.MaxBy(a => a.Attendance)
            : null;
}

public sealed record ParkYearAttendance(short Year, long Attendance)
{
    public string FormattedAttendance => Attendance.ToString("N0");
}

/// <summary>
/// A team's tenure at a ballpark
/// </summary>
public class ParkTenant
{
    public string TeamId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public short FirstYear { get; set; }
    public short LastYear { get; set; }
    public int SeasonCount { get; set; }
    public int TotalGames { get; set; }

    public string YearsActive => ParkLocationFormatter.FormatYears(FirstYear, LastYear);
}

/// <summary>
/// One team-season of home games at a ballpark
/// </summary>
public class ParkSeasonRow
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? TeamName { get; set; }
    public short? Games { get; set; }
    public short? Openings { get; set; }
    public int? Attendance { get; set; }
    public DateOnly? SpanFirst { get; set; }
    public DateOnly? SpanLast { get; set; }

    public string FormattedAttendance => Attendance is > 0 ? Attendance.Value.ToString("N0") : "—";
}

internal static class ParkLocationFormatter
{
    public static string Format(string? city, string? state, string? country)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(city)) parts.Add(city.Trim());
        if (!string.IsNullOrWhiteSpace(state)) parts.Add(state.Trim());
        if (!string.IsNullOrWhiteSpace(country) &&
            !string.Equals(country.Trim(), "US", StringComparison.OrdinalIgnoreCase))
            parts.Add(country.Trim());
        return string.Join(", ", parts);
    }

    public static string FormatYears(short? firstYear, short? lastYear)
    {
        if (!firstYear.HasValue)
            return "—";
        return firstYear == lastYear ? firstYear.Value.ToString() : $"{firstYear}–{lastYear}";
    }
}
