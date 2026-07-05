using baseball_history_web.Models;

namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for displaying a list of team franchises
/// </summary>
public class TeamListViewModel
{
    public List<FranchiseSummary> ActiveFranchises { get; set; } = new();
    public List<FranchiseSummary> InactiveFranchises { get; set; } = new();
    public string? SelectedLeague { get; set; }

    // Filters (issue #43)
    public string? SearchQuery { get; set; }
    public int? Era { get; set; }

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchQuery) || Era.HasValue;

    public int TotalFranchises => ActiveFranchises.Count + InactiveFranchises.Count;

    // Decades from the first professional season through today
    public static IEnumerable<int> EraOptions => PlayerListViewModel.EraOptions;
}

/// <summary>
/// Summary view of a franchise for list display
/// </summary>
public class FranchiseSummary
{
    public string FranchiseId { get; set; } = null!;
    public string FranchiseName { get; set; } = null!;
    public bool IsActive { get; set; }
    public short? FirstYear { get; set; }
    public short? LastYear { get; set; }
    public int TotalSeasons { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int WorldSeriesWins { get; set; }
    public int PennantWins { get; set; }
    public string? CurrentTeamId { get; set; }
    public string? CurrentLeague { get; set; }
    public string? CurrentDivision { get; set; }

    public double WinningPercentage => (TotalWins + TotalLosses) > 0
        ? (double)TotalWins / (TotalWins + TotalLosses)
        : 0;

    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');

    public static FranchiseSummary FromFranchise(TeamsFranchises franchise, IEnumerable<Teams> teams)
    {
        var teamsList = teams.ToList();
        var latestTeam = teamsList.OrderByDescending(t => t.YearId).FirstOrDefault();

        return new FranchiseSummary
        {
            FranchiseId = franchise.FranchId,
            FranchiseName = franchise.FranchName ?? franchise.FranchId,
            IsActive = franchise.Active == "Y",
            FirstYear = teamsList.Min(t => t.YearId),
            LastYear = teamsList.Max(t => t.YearId),
            TotalSeasons = teamsList.Count,
            TotalWins = teamsList.Sum(t => t.W ?? 0),
            TotalLosses = teamsList.Sum(t => t.L ?? 0),
            WorldSeriesWins = teamsList.Count(t => t.Wswin == "Y"),
            PennantWins = teamsList.Count(t => t.LgWin == "Y"),
            CurrentTeamId = latestTeam?.TeamId,
            CurrentLeague = latestTeam?.LgId,
            CurrentDivision = latestTeam?.DivId
        };
    }
}