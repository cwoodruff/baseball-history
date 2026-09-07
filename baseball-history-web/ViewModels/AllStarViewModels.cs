namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for the All-Star Games year index
/// </summary>
public class AllStarIndexViewModel
{
    /// <summary>One row per season, most recent first</summary>
    public List<AllStarYearSummary> Years { get; set; } = new();
}

/// <summary>
/// One All-Star season (1959-1962 had two AL-NL games; 1933-1948 also
/// includes the Negro Leagues East-West Game)
/// </summary>
public class AllStarYearSummary
{
    public short Year { get; set; }
    public int GameCount { get; set; }
    public int Selections { get; set; }
    public bool HasEastWestGame { get; set; }
}

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

    public static string SquadName(string lgId) => lgId switch
    {
        "EAS" => "East",
        "WES" => "West",
        "NOS" => "North",
        "SAS" => "South",
        _ => lgId
    };
}

/// <summary>
/// ViewModel for one season's All-Star roster page
/// </summary>
public class AllStarYearViewModel
{
    public short Year { get; set; }

    /// <summary>The season's games (two for 1959-1962), in game order</summary>
    public List<AllStarGameViewModel> Games { get; set; } = new();

    public List<short> AvailableYears { get; set; } = new();

    public bool HasMultipleGames => Games.Count > 1;

    /// <summary>AL-NL games only; 2 for 1959-1962</summary>
    public int MlbGameCount => Games.Count(g => g.GroupKey.StartsWith("MLB|"));

    public short? PreviousYear
    {
        get
        {
            var previous = AvailableYears.Where(y => y < Year).ToList();
            return previous.Count > 0 ? previous.Max() : null;
        }
    }

    public short? NextYear
    {
        get
        {
            var next = AvailableYears.Where(y => y > Year).ToList();
            return next.Count > 0 ? next.Min() : null;
        }
    }
}

/// <summary>
/// One All-Star Game with rosters grouped by league
/// </summary>
public class AllStarGameViewModel
{
    public string GroupKey { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int GameNum { get; set; }
    public DateOnly? GameDate { get; set; }

    /// <summary>League rosters, AL first</summary>
    public List<AllStarRosterGroup> Rosters { get; set; } = new();

    public string FormattedDate => GameDate?.ToString("MMMM d, yyyy") ?? "";

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

/// <summary>
/// One league's roster within a game
/// </summary>
public class AllStarRosterGroup
{
    public string LgId { get; set; } = null!;
    public List<AllStarRosterRow> Players { get; set; } = new();

    public string DisplayName => AllStarGames.SquadName(LgId);

    /// <summary>The Negro Leagues showcase squads aren't season leagues, so their
    /// team links can't point at /Teams/Season with the squad code</summary>
    public bool IsSeasonLeague => LgId is "AL" or "NL";
}

/// <summary>
/// One player on an All-Star roster
/// </summary>
public class AllStarRosterRow
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }

    /// <summary>The club's actual season league for the team-season link;
    /// null when the club couldn't be resolved for that year</summary>
    public string? LinkLgId { get; set; }

    public int? StartingPos { get; set; }
    public bool Played { get; set; }
    public bool IsInHallOfFame { get; set; }

    public bool IsStarter => StartingPos is > 0;

    public string PositionName => StartingPos switch
    {
        1 => "P",
        2 => "C",
        3 => "1B",
        4 => "2B",
        5 => "3B",
        6 => "SS",
        7 => "LF",
        8 => "CF",
        9 => "RF",
        10 => "DH",
        _ => "—"
    };
}
