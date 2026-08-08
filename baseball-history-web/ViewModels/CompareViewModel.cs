namespace baseball_history_web.ViewModels;

public class CompareViewModel
{
    public ComparePlayer? Player1 { get; set; }
    public ComparePlayer? Player2 { get; set; }

    public bool BothSelected => Player1 != null && Player2 != null;
    public bool HasBatters => (Player1?.BattingStats != null) || (Player2?.BattingStats != null);
    public bool HasPitchers => (Player1?.PitchingStats != null) || (Player2?.PitchingStats != null);
    public bool HasPostseasonBatting => (Player1?.PostseasonBattingStats != null) || (Player2?.PostseasonBattingStats != null);
    public bool HasPostseasonPitching => (Player1?.PostseasonPitchingStats != null) || (Player2?.PostseasonPitchingStats != null);
    public bool HasFielding => (Player1?.FieldingStats != null) || (Player2?.FieldingStats != null);
}

public class ComparePlayer
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Bats { get; set; }
    public string? Throws { get; set; }
    public string? BirthDate { get; set; }
    public string? Debut { get; set; }
    public string? FinalGame { get; set; }
    public int? CareerYears { get; set; }
    public bool IsInHallOfFame { get; set; }
    public int? HofInductionYear { get; set; }

    public CompareCareerBattingStats? BattingStats { get; set; }
    public CompareCareerPitchingStats? PitchingStats { get; set; }
    public ComparePostseasonBattingStats? PostseasonBattingStats { get; set; }
    public ComparePostseasonPitchingStats? PostseasonPitchingStats { get; set; }
    public CompareFieldingStats? FieldingStats { get; set; }

    public int AwardCount { get; set; }
    public int AllStarCount { get; set; }
    public int MvpCount { get; set; }
    public int GoldGloveCount { get; set; }
    public int SilverSluggerCount { get; set; }

    public List<string> TeamNames { get; set; } = new();

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}"
                : FullName.Length >= 2 ? FullName[..2].ToUpper() : FullName.ToUpper();
        }
    }

}

public class CompareCareerBattingStats
{
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
    public double BattingAverage => AtBats > 0 ? (double)Hits / AtBats : 0;
    public double OnBasePercentage => (AtBats + Walks) > 0 ? (double)(Hits + Walks) / (AtBats + Walks) : 0;
    public double SluggingPercentage => AtBats > 0 ? (double)(Hits + Doubles + (2 * Triples) + (3 * HomeRuns)) / AtBats : 0;
    public double Ops => OnBasePercentage + SluggingPercentage;
    public string FormattedAvg => BattingAverage.ToString("0.000").TrimStart('0');
    public string FormattedObp => OnBasePercentage.ToString("0.000").TrimStart('0');
    public string FormattedSlg => SluggingPercentage.ToString("0.000").TrimStart('0');
    public string FormattedOps => Ops.ToString("0.000").TrimStart('0');
}

public class CompareCareerPitchingStats
{
    public int Games { get; set; }
    public int GamesStarted { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Saves { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public double InningsPitched { get; set; }
    public int Hits { get; set; }
    public int EarnedRuns { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public double Era => InningsPitched > 0 ? (EarnedRuns * 9.0) / InningsPitched : 0;
    public double Whip => InningsPitched > 0 ? (Walks + Hits) / InningsPitched : 0;
    public string WinLossRecord => $"{Wins}-{Losses}";
    public string FormattedEra => Era.ToString("0.00");
    public string FormattedWhip => Whip.ToString("0.000");
}

public class ComparePostseasonBattingStats
{
    public int Games { get; set; }
    public int AtBats { get; set; }
    public int Hits { get; set; }
    public int HomeRuns { get; set; }
    public int Rbi { get; set; }
    public double BattingAverage => AtBats > 0 ? (double)Hits / AtBats : 0;
    public string FormattedAvg => BattingAverage.ToString("0.000").TrimStart('0');
}

public class ComparePostseasonPitchingStats
{
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Strikeouts { get; set; }
    public int EarnedRuns { get; set; }
    public double Era => InningsPitched > 0 ? (EarnedRuns * 9.0) / InningsPitched : 0;
    public string FormattedEra => Era.ToString("0.00");
}

public class CompareFieldingStats
{
    public string PrimaryPosition { get; set; } = null!;
    public int Games { get; set; }
    public int Putouts { get; set; }
    public int Assists { get; set; }
    public int Errors { get; set; }
    public int DoublePlays { get; set; }
    public double FieldingPercentage => (Putouts + Assists + Errors) > 0
        ? (double)(Putouts + Assists) / (Putouts + Assists + Errors)
        : 0;
    public string FormattedFieldingPercentage => FieldingPercentage.ToString("0.000").TrimStart('0');
}
