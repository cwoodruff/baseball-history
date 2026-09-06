namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for the managers browser index
/// </summary>
public class ManagerListViewModel
{
    public List<ManagerSummary> Managers { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalManagers { get; set; }
    public string? SearchQuery { get; set; }
    public string Sort { get; set; } = "wins";

    public int TotalPages => (int)Math.Ceiling((double)TotalManagers / PageSize);
    public bool HasActiveFilters => SearchQuery != null;

    public Dictionary<string, string> FilterQueryParams()
    {
        var queryParams = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(SearchQuery))
            queryParams["q"] = SearchQuery;
        if (Sort != "wins")
            queryParams["sort"] = Sort;
        return queryParams;
    }
}

/// <summary>
/// One manager's career line in the browser list
/// </summary>
public class ManagerSummary
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public short FirstYear { get; set; }
    public short LastYear { get; set; }
    public int Seasons { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public bool IsInHallOfFame { get; set; }

    public string YearsLabel => FirstYear == LastYear ? FirstYear.ToString() : $"{FirstYear}–{LastYear}";
    public string Record => $"{Wins}-{Losses}";
    public double WinningPercentage => (Wins + Losses) > 0 ? (double)Wins / (Wins + Losses) : 0;
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// ViewModel for a manager's career page
/// </summary>
public class ManagerDetailViewModel
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsInHallOfFame { get; set; }
    public bool WasPlayer { get; set; }

    /// <summary>Season-by-season stints, most recent first</summary>
    public List<ManagerSeasonRow> Seasons { get; set; } = new();

    /// <summary>Split-season halves (1892, 1981 only), most recent first</summary>
    public List<ManagerHalfRow> Halves { get; set; } = new();

    /// <summary>Manager awards, most recent first</summary>
    public List<ManagerAwardRow> Awards { get; set; } = new();

    public int Games => Seasons.Sum(s => s.Games ?? 0);
    public int Wins => Seasons.Sum(s => (int)(s.Wins ?? 0));
    public int Losses => Seasons.Sum(s => (int)(s.Losses ?? 0));
    public int SeasonCount => Seasons.Select(s => s.Year).Distinct().Count();
    public int TeamCount => Seasons.Select(s => s.TeamId).Distinct().Count();
    public int Pennants => Seasons.Count(s => s.WonPennant);
    public int WorldSeriesTitles => Seasons.Count(s => s.WonWorldSeries);
    public bool WasPlayerManager => Seasons.Any(s => s.IsPlayerManager);

    public short? FirstYear => Seasons.Count > 0 ? Seasons.Min(s => s.Year) : null;
    public short? LastYear => Seasons.Count > 0 ? Seasons.Max(s => s.Year) : null;
    public string YearsLabel =>
        FirstYear.HasValue
            ? FirstYear == LastYear ? FirstYear.Value.ToString() : $"{FirstYear}–{LastYear}"
            : "—";

    public string Record => $"{Wins}-{Losses}";
    public double WinningPercentage => (Wins + Losses) > 0 ? (double)Wins / (Wins + Losses) : 0;
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// One managerial stint (team-season, ordered by in-season sequence)
/// </summary>
public class ManagerSeasonRow
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? TeamName { get; set; }
    public byte Inseason { get; set; }
    public short? Games { get; set; }
    public short? Wins { get; set; }
    public short? Losses { get; set; }
    public byte? Rank { get; set; }
    public bool IsPlayerManager { get; set; }
    public bool WonPennant { get; set; }
    public bool WonWorldSeries { get; set; }

    public string Record => $"{Wins ?? 0}-{Losses ?? 0}";
    public double WinningPercentage =>
        (Wins ?? 0) + (Losses ?? 0) > 0 ? (double)(Wins ?? 0) / ((Wins ?? 0) + (Losses ?? 0)) : 0;
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// One half of a split season (1892 or 1981)
/// </summary>
public class ManagerHalfRow
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? TeamName { get; set; }
    public byte Half { get; set; }
    public short? Games { get; set; }
    public short? Wins { get; set; }
    public short? Losses { get; set; }
    public byte? Rank { get; set; }

    public string Record => $"{Wins ?? 0}-{Losses ?? 0}";
}

/// <summary>
/// A manager award (BBWAA/TSN Manager of the Year, ...)
/// </summary>
public class ManagerAwardRow
{
    public short Year { get; set; }
    public string AwardId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public bool HasVotingData { get; set; }
}
