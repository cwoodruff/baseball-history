using baseball_history_web.Models;

namespace baseball_history_web.ViewModels;

/// <summary>
/// Detailed view of a player including career statistics
/// </summary>
public class PlayerDetailViewModel
{
    // Basic info
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? GivenName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Physical attributes
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public string? Bats { get; set; }
    public string? Throws { get; set; }

    // Birth/Death info
    public string? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? DeathDate { get; set; }

    // Career span
    public string? Debut { get; set; }
    public string? FinalGame { get; set; }
    public int? CareerYears { get; set; }

    // Hall of Fame
    public bool IsInHallOfFame { get; set; }
    public int? HofInductionYear { get; set; }

    // Career batting stats
    public CareerBattingStats? BattingStats { get; set; }

    // Career pitching stats
    public CareerPitchingStats? PitchingStats { get; set; }

    // Season-by-season records
    public List<SeasonBattingRecord> BattingSeasons { get; set; } = new();
    public List<SeasonPitchingRecord> PitchingSeasons { get; set; } = new();

    // Awards and honors
    public List<AwardRecord> Awards { get; set; } = new();
    public List<AllStarRecord> AllStarAppearances { get; set; } = new();

    // Teams played for
    public List<TeamRecord> Teams { get; set; } = new();

    public static PlayerDetailViewModel FromPeople(People person)
    {
        var vm = new PlayerDetailViewModel
        {
            PlayerId = person.PlayerId,
            FirstName = person.NameFirst,
            LastName = person.NameLast,
            GivenName = person.NameGiven,
            FullName = $"{person.NameFirst} {person.NameLast}".Trim(),
            Height = person.Height,
            Weight = person.Weight,
            Bats = person.Bats,
            Throws = person.Throws,
            Debut = person.Debut?.ToString("MMMM d, yyyy"),
            FinalGame = person.FinalGame?.ToString("MMMM d, yyyy")
        };

        // Build birth/death info
        if (!string.IsNullOrEmpty(person.BirthYear))
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(person.BirthMonth) && !string.IsNullOrEmpty(person.BirthDay))
            {
                if (int.TryParse(person.BirthMonth, out var month) && int.TryParse(person.BirthDay, out var day) && int.TryParse(person.BirthYear, out var year))
                {
                    try
                    {
                        vm.BirthDate = new DateOnly(year, month, day).ToString("MMMM d, yyyy");
                    }
                    catch
                    {
                        vm.BirthDate = person.BirthYear;
                    }
                }
            }
            else
            {
                vm.BirthDate = person.BirthYear;
            }
        }

        var birthPlaceParts = new List<string>();
        if (!string.IsNullOrEmpty(person.BirthCity)) birthPlaceParts.Add(person.BirthCity);
        if (!string.IsNullOrEmpty(person.BirthState)) birthPlaceParts.Add(person.BirthState);
        if (!string.IsNullOrEmpty(person.BirthCountry)) birthPlaceParts.Add(person.BirthCountry);
        vm.BirthPlace = birthPlaceParts.Count > 0 ? string.Join(", ", birthPlaceParts) : null;

        if (!string.IsNullOrEmpty(person.DeathYear))
        {
            if (!string.IsNullOrEmpty(person.DeathMonth) && !string.IsNullOrEmpty(person.DeathDay))
            {
                if (int.TryParse(person.DeathMonth, out var month) && int.TryParse(person.DeathDay, out var day) && int.TryParse(person.DeathYear, out var year))
                {
                    try
                    {
                        vm.DeathDate = new DateOnly(year, month, day).ToString("MMMM d, yyyy");
                    }
                    catch
                    {
                        vm.DeathDate = person.DeathYear;
                    }
                }
            }
            else
            {
                vm.DeathDate = person.DeathYear;
            }
        }

        // Calculate career years
        if (person.Debut.HasValue && person.FinalGame.HasValue)
        {
            vm.CareerYears = person.FinalGame.Value.Year - person.Debut.Value.Year + 1;
        }

        return vm;
    }
}

/// <summary>
/// Career batting statistics summary
/// </summary>
public class CareerBattingStats
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
    public double SluggingPercentage => AtBats > 0 ? (double)(Hits + Doubles + 2 * Triples + 3 * HomeRuns) / AtBats : 0;
    public double Ops => OnBasePercentage + SluggingPercentage;

    public string FormattedAvg => BattingAverage.ToString(".000").TrimStart('0');
    public string FormattedObp => OnBasePercentage.ToString(".000").TrimStart('0');
    public string FormattedSlg => SluggingPercentage.ToString(".000").TrimStart('0');
    public string FormattedOps => Ops.ToString(".000").TrimStart('0');
}

/// <summary>
/// Career pitching statistics summary
/// </summary>
public class CareerPitchingStats
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
    public double StrikeoutsPer9 => InningsPitched > 0 ? (Strikeouts * 9.0) / InningsPitched : 0;

    public string FormattedEra => Era.ToString("0.00");
    public string FormattedWhip => Whip.ToString("0.00");
    public string WinLossRecord => $"{Wins}-{Losses}";
}

/// <summary>
/// Single season batting record
/// </summary>
public class SeasonBattingRecord
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public string LgId { get; set; } = null!;
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
    public string FormattedAvg => BattingAverage.ToString(".000").TrimStart('0');
}

/// <summary>
/// Single season pitching record
/// </summary>
public class SeasonPitchingRecord
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public string LgId { get; set; } = null!;
    public int Games { get; set; }
    public int GamesStarted { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Saves { get; set; }
    public double InningsPitched { get; set; }
    public int Hits { get; set; }
    public int EarnedRuns { get; set; }
    public int Strikeouts { get; set; }
    public int Walks { get; set; }

    public double Era => InningsPitched > 0 ? (EarnedRuns * 9.0) / InningsPitched : 0;
    public string FormattedEra => Era.ToString("0.00");
    public string WinLossRecord => $"{Wins}-{Losses}";
}

/// <summary>
/// Award record for a player
/// </summary>
public class AwardRecord
{
    public short Year { get; set; }
    public string AwardId { get; set; } = null!;
    public string LgId { get; set; } = null!;
    public string? Notes { get; set; }
}

/// <summary>
/// All-Star game appearance record
/// </summary>
public class AllStarRecord
{
    public short Year { get; set; }
    public string LgId { get; set; } = null!;
    public string TeamId { get; set; } = null!;
    public int GameNum { get; set; }
}

/// <summary>
/// Team a player played for
/// </summary>
public class TeamRecord
{
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public short FirstYear { get; set; }
    public short LastYear { get; set; }
    public int Seasons { get; set; }
}
