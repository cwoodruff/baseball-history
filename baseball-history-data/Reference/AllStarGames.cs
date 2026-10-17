namespace BaseballHistory.Data.Reference;

/// <summary>
/// Classification of AllstarFull rows into distinct games. AL/NL rows carry
/// Retrosheet game IDs; the Negro Leagues showcase games (East-West at
/// Comiskey Park, North-South in New Orleans) have empty game IDs and are
/// identified by their squad league codes instead.
/// </summary>
public static class AllStarGames
{
    public const string EastWestKey = "EW";
    public const string NorthSouthKey = "NS";

    public static string GroupKey(string lgId, string gameId) => lgId switch
    {
        "AL" or "NL" => "MLB|" + gameId,
        "EAS" or "WES" => EastWestKey,
        "NOS" or "SAS" => NorthSouthKey,
        _ => "OTHER|" + lgId
    };

    public static string TitleFor(string groupKey, IReadOnlyList<string> leagues) => groupKey switch
    {
        EastWestKey => "East-West Game",
        NorthSouthKey => "North-South Game",
        _ when groupKey.StartsWith("MLB|") => "All-Star Game",
        _ => string.Join(" vs. ", leagues)
    };

    /// <summary>Games sort MLB first, then East-West, then North-South, then others</summary>
    public static int TypeOrder(string groupKey) => groupKey switch
    {
        _ when groupKey.StartsWith("MLB|") => 0,
        EastWestKey => 1,
        NorthSouthKey => 2,
        _ => 3
    };

    /// <summary>API/MCP label for a game: AllStar (AL vs. NL), EastWest, NorthSouth, or Other</summary>
    public static string GameType(string groupKey) => groupKey switch
    {
        EastWestKey => "EastWest",
        NorthSouthKey => "NorthSouth",
        _ when groupKey.StartsWith("MLB|") => "AllStar",
        _ => "Other"
    };

    public static string SquadName(string lgId) => lgId switch
    {
        "EAS" => "East",
        "WES" => "West",
        "NOS" => "North",
        "SAS" => "South",
        _ => lgId
    };

    /// <summary>
    /// Parses the game date from a Lahman gameID like "NLS195507120"
    /// (host-league prefix + yyyyMMdd + game-of-day digit).
    /// </summary>
    public static DateOnly? ParseGameDate(string? gameId)
    {
        if (gameId == null || gameId.Length < 11)
            return null;
        return DateOnly.TryParseExact(gameId.Substring(3, 8), "yyyyMMdd", out var date) ? date : null;
    }
}
