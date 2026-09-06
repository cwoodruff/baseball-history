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
    public string? ParkKey { get; set; }
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

    public static TeamSeasonViewModel FromRecord(TeamSeasonRecord team)
    {
        var vm = new TeamSeasonViewModel
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            Year = team.Year,
            LgId = team.LgId,
            DivId = team.DivId,
            FranchiseId = team.FranchiseId,
            FranchiseName = team.FranchiseName,
            Wins = team.Wins,
            Losses = team.Losses,
            Rank = team.Rank,
            WonDivision = team.WonDivision,
            WonWildCard = team.WonWildCard,
            WonPennant = team.WonPennant,
            WonWorldSeries = team.WonWorldSeries,
            ParkName = team.ParkName
        };

        if (TryParsePositiveInt(team.Attendance, out var attendance))
        {
            vm.Attendance = attendance;
        }

        if (TryParseInt(team.Runs, out var runs) &&
            TryParseInt(team.AtBats, out var ab) &&
            TryParseInt(team.Hits, out var hits))
        {
            vm.Batting = new TeamBattingStats
            {
                Runs = runs,
                AtBats = ab,
                Hits = hits,
                Doubles = ParseIntOrZero(team.Doubles),
                Triples = ParseIntOrZero(team.Triples),
                HomeRuns = ParseIntOrZero(team.HomeRuns),
                Walks = ParseIntOrZero(team.Walks),
                Strikeouts = ParseIntOrZero(team.Strikeouts),
                StolenBases = ParseIntOrZero(team.StolenBases)
            };
        }

        if (TryParseInt(team.RunsAllowed, out var runsAllowed) &&
            TryParseInt(team.EarnedRuns, out var earnedRuns))
        {
            vm.Pitching = new TeamPitchingStats
            {
                RunsAllowed = runsAllowed,
                EarnedRuns = earnedRuns,
                Era = double.TryParse(team.Era, out var era) ? era : 0,
                CompleteGames = ParseIntOrZero(team.CompleteGames),
                Shutouts = ParseIntOrZero(team.Shutouts),
                Saves = ParseIntOrZero(team.Saves),
                HitsAllowed = ParseIntOrZero(team.HitsAllowed),
                HomeRunsAllowed = ParseIntOrZero(team.HomeRunsAllowed),
                WalksAllowed = ParseIntOrZero(team.WalksAllowed),
                StrikeoutsThrown = ParseIntOrZero(team.StrikeoutsThrown)
            };
        }

        return vm;
    }

    private static bool TryParseInt(string? value, out int parsed) => int.TryParse(value, out parsed);

    private static bool TryParsePositiveInt(string? value, out int parsed)
    {
        if (int.TryParse(value, out parsed) && parsed > 0)
        {
            return true;
        }

        parsed = 0;
        return false;
    }

    private static int ParseIntOrZero(string? value) => int.TryParse(value, out var parsed) ? parsed : 0;
}

public sealed record TeamSeasonRecord(
    string TeamId,
    string? TeamName,
    short Year,
    string LgId,
    string? DivId,
    string? FranchiseId,
    string? FranchiseName,
    short Wins,
    short Losses,
    byte? Rank,
    bool WonDivision,
    bool WonWildCard,
    bool WonPennant,
    bool WonWorldSeries,
    string? ParkName,
    string? Attendance,
    string? Runs,
    string? AtBats,
    string? Hits,
    string? Doubles,
    string? Triples,
    string? HomeRuns,
    string? Walks,
    string? Strikeouts,
    string? StolenBases,
    string? RunsAllowed,
    string? EarnedRuns,
    string? Era,
    string? CompleteGames,
    string? Shutouts,
    string? Saves,
    string? HitsAllowed,
    string? HomeRunsAllowed,
    string? WalksAllowed,
    string? StrikeoutsThrown);

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
