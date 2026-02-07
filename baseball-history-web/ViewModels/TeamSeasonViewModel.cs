using baseball_history_web.Models;

namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for displaying a team's season details
/// </summary>
public class TeamSeasonViewModel
{
    // Team identification
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public short Year { get; set; }
    public string LgId { get; set; } = null!;
    public string? DivId { get; set; }
    public string? FranchiseId { get; set; }
    public string? FranchiseName { get; set; }

    // Season record
    public short Wins { get; set; }
    public short Losses { get; set; }
    public byte? Rank { get; set; }
    public bool WonDivision { get; set; }
    public bool WonWildCard { get; set; }
    public bool WonPennant { get; set; }
    public bool WonWorldSeries { get; set; }

    // Team batting stats
    public TeamBattingStats? Batting { get; set; }

    // Team pitching stats
    public TeamPitchingStats? Pitching { get; set; }

    // Ballpark info
    public string? ParkName { get; set; }
    public int? Attendance { get; set; }

    // Roster
    public List<RosterPlayer> Batters { get; set; } = new();
    public List<RosterPlayer> Pitchers { get; set; } = new();
    public List<ManagerInfo> Managers { get; set; } = new();

    // Other seasons for navigation
    public List<short> AvailableYears { get; set; } = new();

    public double WinningPercentage => (Wins + Losses) > 0
        ? (double)Wins / (Wins + Losses)
        : 0;

    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
    public string Record => $"{Wins}-{Losses}";
    public string FormattedAttendance => Attendance?.ToString("N0") ?? "N/A";

    public static TeamSeasonViewModel FromTeam(Teams team)
    {
        var vm = new TeamSeasonViewModel
        {
            TeamId = team.TeamId,
            TeamName = team.Name,
            Year = team.YearId,
            LgId = team.LgId,
            DivId = team.DivId,
            FranchiseId = team.FranchId,
            FranchiseName = team.Franchise?.FranchName,
            Wins = team.W ?? 0,
            Losses = team.L ?? 0,
            Rank = team.Rank,
            WonDivision = team.DivWin == "Y",
            WonWildCard = team.Wcwin == "Y",
            WonPennant = team.LgWin == "Y",
            WonWorldSeries = team.Wswin == "Y",
            ParkName = team.Park
        };

        if (int.TryParse(team.Attendance, out var attendance))
        {
            vm.Attendance = attendance;
        }

        // Build batting stats
        if (int.TryParse(team.R, out var runs) &&
            int.TryParse(team.Ab, out var ab) &&
            int.TryParse(team.H, out var hits))
        {
            vm.Batting = new TeamBattingStats
            {
                Runs = runs,
                AtBats = ab,
                Hits = hits,
                Doubles = int.TryParse(team._2b, out var d) ? d : 0,
                Triples = int.TryParse(team._3b, out var t) ? t : 0,
                HomeRuns = int.TryParse(team.Hr, out var hr) ? hr : 0,
                Walks = int.TryParse(team.Bb, out var bb) ? bb : 0,
                Strikeouts = int.TryParse(team.So, out var so) ? so : 0,
                StolenBases = int.TryParse(team.Sb, out var sb) ? sb : 0
            };
        }

        // Build pitching stats
        if (int.TryParse(team.Ra, out var runsAllowed) &&
            int.TryParse(team.Er, out var earnedRuns))
        {
            vm.Pitching = new TeamPitchingStats
            {
                RunsAllowed = runsAllowed,
                EarnedRuns = earnedRuns,
                Era = double.TryParse(team.Era, out var era) ? era : 0,
                CompleteGames = int.TryParse(team.Cg, out var cg) ? cg : 0,
                Shutouts = int.TryParse(team.Sho, out var sho) ? sho : 0,
                Saves = int.TryParse(team.Sv, out var sv) ? sv : 0,
                HitsAllowed = int.TryParse(team.Ha, out var ha) ? ha : 0,
                HomeRunsAllowed = int.TryParse(team.Hra, out var hra) ? hra : 0,
                WalksAllowed = int.TryParse(team.Bba, out var bba) ? bba : 0,
                StrikeoutsThrown = int.TryParse(team.Soa, out var soa) ? soa : 0
            };
        }

        return vm;
    }
}

/// <summary>
/// Team batting statistics for a season
/// </summary>
public class TeamBattingStats
{
    public int Runs { get; set; }
    public int AtBats { get; set; }
    public int Hits { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int HomeRuns { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public int StolenBases { get; set; }

    public double BattingAverage => AtBats > 0 ? (double)Hits / AtBats : 0;
    public string FormattedAvg => BattingAverage.ToString(".000").TrimStart('0');
}

/// <summary>
/// Team pitching statistics for a season
/// </summary>
public class TeamPitchingStats
{
    public int RunsAllowed { get; set; }
    public int EarnedRuns { get; set; }
    public double Era { get; set; }
    public int CompleteGames { get; set; }
    public int Shutouts { get; set; }
    public int Saves { get; set; }
    public int HitsAllowed { get; set; }
    public int HomeRunsAllowed { get; set; }
    public int WalksAllowed { get; set; }
    public int StrikeoutsThrown { get; set; }

    public string FormattedEra => Era.ToString("0.00");
}

/// <summary>
/// Player on a team roster
/// </summary>
public class RosterPlayer
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Position { get; set; }
    public int Games { get; set; }

    // For batters
    public int? AtBats { get; set; }
    public int? Hits { get; set; }
    public int? HomeRuns { get; set; }
    public int? Rbi { get; set; }
    public double? BattingAverage { get; set; }

    // For pitchers
    public int? Wins { get; set; }
    public int? Losses { get; set; }
    public double? Era { get; set; }
    public int? Strikeouts { get; set; }
    public int? Saves { get; set; }

    public bool IsInHallOfFame { get; set; }

    public string FormattedAvg => BattingAverage?.ToString(".000").TrimStart('0') ?? ".000";
    public string FormattedEra => Era?.ToString("0.00") ?? "0.00";
    public string? WinLossRecord => Wins.HasValue ? $"{Wins}-{Losses}" : null;
}

/// <summary>
/// Manager information for a team season
/// </summary>
public class ManagerInfo
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Order { get; set; }
    public bool IsInHallOfFame { get; set; }

    public string Record => $"{Wins}-{Losses}";
}