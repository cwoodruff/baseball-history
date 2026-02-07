namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for the Hall of Fame page
/// </summary>
public class HallOfFameViewModel
{
    public List<HallOfFameInductee> Inductees { get; set; } = new();
    public int? SelectedYear { get; set; }
    public string? SelectedCategory { get; set; }

    // Stats
    public int TotalInductees { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalManagers { get; set; }
    public int TotalPioneers { get; set; }
    public int TotalUmpires { get; set; }

    // Available filters
    public List<int> AvailableYears { get; set; } = new();

    public List<string> AvailableCategories { get; set; } =
        new() { "Player", "Manager", "Pioneer/Executive", "Umpire" };

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Hall of Fame inductee record
/// </summary>
public class HallOfFameInductee
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int InductionYear { get; set; }
    public string Category { get; set; } = null!;
    public string? VotedBy { get; set; }
    public double? VotePercentage { get; set; }

    // Career info
    public string? DebutYear { get; set; }
    public string? FinalYear { get; set; }
    public int? CareerYears { get; set; }

    // Key stats (for players)
    public int? TotalHits { get; set; }
    public int? TotalHomeRuns { get; set; }
    public double? CareerAvg { get; set; }
    public int? TotalWins { get; set; } // For pitchers
    public double? CareerEra { get; set; } // For pitchers
    public int? TotalStrikeouts { get; set; } // For pitchers

    public string? PrimaryPosition { get; set; }
    public bool IsPitcher => PrimaryPosition == "P";

    public string FormattedVotePct => VotePercentage.HasValue
        ? $"{VotePercentage:0.0}%"
        : "-";

    public string FormattedCareerAvg => CareerAvg.HasValue
        ? CareerAvg.Value.ToString(".000").TrimStart('0')
        : "-";

    public string FormattedCareerEra => CareerEra.HasValue
        ? CareerEra.Value.ToString("0.00")
        : "-";
}