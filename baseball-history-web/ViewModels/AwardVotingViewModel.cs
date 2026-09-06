namespace baseball_history_web.ViewModels;

public class AwardVotingViewModel
{
    public List<AwardWinnerEntry> Winners { get; set; } = new();
    public AwardRaceDetail? VotingDetail { get; set; }

    public string? SelectedAward { get; set; }
    public int? SelectedYear { get; set; }
    public string? SelectedLeague { get; set; }

    /// <summary>"players" (default) or "managers" — which award tables the page reads</summary>
    public string Scope { get; set; } = "players";
    public bool IsManagersScope => Scope == "managers";

    public List<string> AvailableAwards { get; set; } = new();
    public List<int> AvailableYears { get; set; } = new();
    public List<string> AvailableLeagues { get; set; } = new();

    public int TotalEntries { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 50;
}

public class AwardWinnerEntry
{
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public string AwardId { get; set; } = null!;
    public int Year { get; set; }
    public string LgId { get; set; } = null!;
    public string? Notes { get; set; }
    public bool IsInHallOfFame { get; set; }
    public bool HasVotingData { get; set; }
}

public class AwardRaceDetail
{
    public string AwardId { get; set; } = null!;
    public int Year { get; set; }
    public string LgId { get; set; } = null!;
    public List<AwardVoteEntry> Votes { get; set; } = new();
}

public class AwardVoteEntry
{
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public short PointsWon { get; set; }
    public short PointsMax { get; set; }
    public string? VotesFirst { get; set; }
    public double VoteShare { get; set; }
    public bool IsInHallOfFame { get; set; }
    public bool IsWinner { get; set; }

    public string FormattedVoteShare => $"{VoteShare:0.0}%";
}
