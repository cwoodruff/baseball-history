namespace baseball_history_web.ViewModels;

public class PostseasonViewModel
{
    public List<PostseasonSeriesEntry> Series { get; set; } = new();
    public int? SelectedYear { get; set; }
    public string? SelectedRound { get; set; }

    public List<int> AvailableYears { get; set; } = new();

    public static readonly Dictionary<string, string> RoundNames = new()
    {
        ["WS"] = "World Series",
        ["ALCS"] = "AL Championship Series",
        ["NLCS"] = "NL Championship Series",
        ["ALDS"] = "AL Division Series",
        ["NLDS"] = "NL Division Series",
        ["ALWC"] = "AL Wild Card",
        ["NLWC"] = "NL Wild Card",
        ["AEDIV"] = "AL Division Series",
        ["AWDIV"] = "AL Division Series",
        ["NEDIV"] = "NL Division Series",
        ["NWDIV"] = "NL Division Series"
    };

    // Player postseason rows use numbered round codes (ALDS1, NLWC2) that the
    // series table does not; strip the trailing digit before the name lookup.
    public static string RoundDisplayName(string round)
    {
        if (RoundNames.TryGetValue(round, out var name))
            return name;

        var trimmed = round.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        return RoundNames.GetValueOrDefault(trimmed, round);
    }

    // Playoff chronology for sorting rounds within a year: Wild Card ->
    // Division Series -> Championship Series -> World Series.
    public static int RoundChronologicalRank(string round)
    {
        var trimmed = round.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        return trimmed switch
        {
            "ALWC" or "NLWC" => 0,
            "ALDS" or "NLDS" or "AEDIV" or "AWDIV" or "NEDIV" or "NWDIV" => 1,
            "ALCS" or "NLCS" or "CS" => 2,
            "WS" => 3,
            _ => 4
        };
    }

    public int TotalSeries { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 50;
}

public class PostseasonSeriesEntry
{
    public short Year { get; set; }
    public string Round { get; set; } = null!;
    public string RoundName => PostseasonViewModel.RoundNames.GetValueOrDefault(Round, Round);
    public string WinnerTeamId { get; set; } = null!;
    public string? WinnerTeamName { get; set; }
    public string WinnerLgId { get; set; } = null!;
    public string? LoserTeamId { get; set; }
    public string? LoserTeamName { get; set; }
    public string? LoserLgId { get; set; }
    public short Wins { get; set; }
    public short Losses { get; set; }
    public short Ties { get; set; }

    public string SeriesResult => Ties > 0 ? $"{Wins}-{Losses}-{Ties}" : $"{Wins}-{Losses}";
}
