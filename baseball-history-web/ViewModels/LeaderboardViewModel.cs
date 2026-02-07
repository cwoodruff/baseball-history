namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for displaying stat leaderboards
/// </summary>
public class LeaderboardViewModel
{
    public string Title { get; set; } = null!;
    public LeaderboardType Type { get; set; }
    public string StatColumn { get; set; } = null!;
    public string StatLabel { get; set; } = null!;

    // Filters
    public int? FromYear { get; set; }
    public int? ToYear { get; set; }
    public string? League { get; set; }
    public int MinimumAtBats { get; set; } = 0;
    public int MinimumInningsPitched { get; set; } = 0;
    public bool SingleSeason { get; set; }

    // Results
    public List<BattingLeaderEntry> BattingLeaders { get; set; } = new();
    public List<PitchingLeaderEntry> PitchingLeaders { get; set; } = new();

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalEntries { get; set; }
    public int PageSize { get; set; } = 100;

    // Available filter options
    public List<int> AvailableYears { get; set; } = new();
    public List<string> AvailableLeagues { get; set; } = new();
    public Dictionary<string, string> AvailableStats { get; set; } = new();
}

public enum LeaderboardType
{
    Batting,
    Pitching
}

/// <summary>
/// Entry in a batting leaderboard
/// </summary>
public class BattingLeaderEntry
{
    public int Rank { get; set; }
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public short? Year { get; set; } // For single-season leaderboards
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public bool IsInHallOfFame { get; set; }

    // Stats
    public int Games { get; set; }
    public int AtBats { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int Rbi { get; set; }
    public int StolenBases { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }

    // Calculated stats
    public double BattingAverage => AtBats > 0 ? (double)Hits / AtBats : 0;
    public double OnBasePercentage => (AtBats + Walks) > 0 ? (double)(Hits + Walks) / (AtBats + Walks) : 0;
    public double SluggingPercentage => AtBats > 0 ? (double)(Hits + Doubles + 2 * Triples + 3 * HomeRuns) / AtBats : 0;
    public double Ops => OnBasePercentage + SluggingPercentage;
    public int TotalBases => Hits + Doubles + 2 * Triples + 3 * HomeRuns;
    public int ExtraBaseHits => Doubles + Triples + HomeRuns;

    public string FormattedAvg => BattingAverage.ToString(".000").TrimStart('0');
    public string FormattedObp => OnBasePercentage.ToString(".000").TrimStart('0');
    public string FormattedSlg => SluggingPercentage.ToString(".000").TrimStart('0');
    public string FormattedOps => Ops.ToString(".000").TrimStart('0');

    // Get the value for the stat being sorted
    public object GetStatValue(string statColumn)
    {
        return statColumn.ToLower() switch
        {
            "hr" or "homeruns" => HomeRuns,
            "h" or "hits" => Hits,
            "r" or "runs" => Runs,
            "rbi" => Rbi,
            "sb" or "stolenbases" => StolenBases,
            "avg" or "battingaverage" => BattingAverage,
            "obp" or "onbasepercentage" => OnBasePercentage,
            "slg" or "sluggingpercentage" => SluggingPercentage,
            "ops" => Ops,
            "2b" or "doubles" => Doubles,
            "3b" or "triples" => Triples,
            "tb" or "totalbases" => TotalBases,
            "bb" or "walks" => Walks,
            "g" or "games" => Games,
            "ab" or "atbats" => AtBats,
            _ => Hits
        };
    }
}

/// <summary>
/// Entry in a pitching leaderboard
/// </summary>
public class PitchingLeaderEntry
{
    public int Rank { get; set; }
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public short? Year { get; set; } // For single-season leaderboards
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public bool IsInHallOfFame { get; set; }

    // Stats
    public int Games { get; set; }
    public int GamesStarted { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Saves { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public double InningsPitched { get; set; }
    public int Hits { get; set; }
    public int Runs { get; set; }
    public int EarnedRuns { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }

    // Calculated stats
    public double Era => InningsPitched > 0 ? (EarnedRuns * 9.0) / InningsPitched : 0;
    public double Whip => InningsPitched > 0 ? (Walks + Hits) / InningsPitched : 0;
    public double StrikeoutsPer9 => InningsPitched > 0 ? (Strikeouts * 9.0) / InningsPitched : 0;
    public double WalksPer9 => InningsPitched > 0 ? (Walks * 9.0) / InningsPitched : 0;
    public double StrikeoutToWalkRatio => Walks > 0 ? (double)Strikeouts / Walks : Strikeouts;
    public double WinningPercentage => (Wins + Losses) > 0 ? (double)Wins / (Wins + Losses) : 0;

    public string FormattedEra => Era.ToString("0.00");
    public string FormattedWhip => Whip.ToString("0.00");
    public string FormattedIp => InningsPitched.ToString("0.0");
    public string WinLossRecord => $"{Wins}-{Losses}";
    public string FormattedK9 => StrikeoutsPer9.ToString("0.0");
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');

    // Get the value for the stat being sorted
    public object GetStatValue(string statColumn)
    {
        return statColumn.ToLower() switch
        {
            "w" or "wins" => Wins,
            "l" or "losses" => Losses,
            "era" => Era,
            "so" or "strikeouts" => Strikeouts,
            "sv" or "saves" => Saves,
            "cg" or "completegames" => CompleteGames,
            "sho" or "shutouts" => Shutouts,
            "ip" or "inningspitched" => InningsPitched,
            "whip" => Whip,
            "k9" or "strikeoutsper9" => StrikeoutsPer9,
            "bb9" or "walksper9" => WalksPer9,
            "g" or "games" => Games,
            "gs" or "gamesstarted" => GamesStarted,
            "wpct" or "winningpercentage" => WinningPercentage,
            _ => Wins
        };
    }
}

/// <summary>
/// Stat descriptor for dropdown menus
/// </summary>
public static class LeaderboardStats
{
    public static readonly Dictionary<string, string> BattingStats = new()
    {
        { "hr", "Home Runs" },
        { "h", "Hits" },
        { "r", "Runs" },
        { "rbi", "RBI" },
        { "sb", "Stolen Bases" },
        { "avg", "Batting Average" },
        { "obp", "On-Base Percentage" },
        { "slg", "Slugging Percentage" },
        { "ops", "OPS" },
        { "2b", "Doubles" },
        { "3b", "Triples" },
        { "tb", "Total Bases" },
        { "bb", "Walks" },
        { "g", "Games" }
    };

    public static readonly Dictionary<string, string> PitchingStats = new()
    {
        { "w", "Wins" },
        { "era", "ERA" },
        { "so", "Strikeouts" },
        { "sv", "Saves" },
        { "cg", "Complete Games" },
        { "sho", "Shutouts" },
        { "ip", "Innings Pitched" },
        { "whip", "WHIP" },
        { "k9", "K/9" },
        { "wpct", "Win %" },
        { "l", "Losses" },
        { "g", "Games" },
        { "gs", "Games Started" }
    };
}