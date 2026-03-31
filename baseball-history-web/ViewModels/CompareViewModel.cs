namespace baseball_history_web.ViewModels;

public class CompareViewModel
{
    public ComparePlayer? Player1 { get; set; }
    public ComparePlayer? Player2 { get; set; }

    public bool BothSelected => Player1 != null && Player2 != null;
    public bool HasBatters => (Player1?.BattingStats != null) || (Player2?.BattingStats != null);
    public bool HasPitchers => (Player1?.PitchingStats != null) || (Player2?.PitchingStats != null);
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

    public CareerBattingStats? BattingStats { get; set; }
    public CareerPitchingStats? PitchingStats { get; set; }

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
