using BaseballHistory.Data.Reference;

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

    /// <summary>Parses the game date from a Lahman gameID; see AllStarGames.ParseGameDate</summary>
    public static DateOnly? ParseGameDate(string? gameId) => AllStarGames.ParseGameDate(gameId);
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
