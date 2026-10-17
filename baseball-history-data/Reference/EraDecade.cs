namespace BaseballHistory.Data.Reference;

/// <summary>
/// The decade filter used by the Players and Teams browsers, the REST API,
/// and the MCP server: any year in a decade selects that decade (1927 -> 1920).
/// </summary>
public static class EraDecade
{
    public static bool IsValid(int? era) => era is >= 1870 and <= 2030;

    public static int? From(int? era) => IsValid(era) ? era - era % 10 : null;

    /// <summary>True when the span [firstYear, lastYear] overlaps the decade; false when either end is unknown</summary>
    public static bool Overlaps(int decade, int? firstYear, int? lastYear) =>
        firstYear <= decade + 9 && lastYear >= decade;
}
