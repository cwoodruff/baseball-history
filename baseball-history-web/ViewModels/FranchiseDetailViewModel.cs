namespace baseball_history_web.ViewModels;

public class FranchiseDetailViewModel
{
    public FranchiseSummary? Franchise { get; set; }
    public List<TeamSeasonSummary> Seasons { get; set; } = new();
}

public class TeamSeasonSummary
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? DivId { get; set; }
    public short Wins { get; set; }
    public short Losses { get; set; }
    public byte? Rank { get; set; }
    public bool WonDivision { get; set; }
    public bool WonPennant { get; set; }
    public bool WonWorldSeries { get; set; }

    public double WinningPercentage => (Wins + Losses) > 0
        ? (double)Wins / (Wins + Losses)
        : 0;

    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
    public string Record => $"{Wins}-{Losses}";
}
