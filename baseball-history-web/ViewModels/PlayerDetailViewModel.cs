using baseball_history_web.Services;
using BaseballHistory.Data.Models;

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

    // Only a surname or initial survived the source record (see PlayerRecordFacts)
    public bool IsPartialRecord { get; set; }

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

    // League-indexed career context (v_career_batting; see docs/qualification_and_league_index.sql)
    public int? CareerOpsIndex { get; set; }
    public bool? CareerQualified { get; set; }
    public double? CareerPctOfThreshold { get; set; }

    // Season rows from the shared query layer (stints collapsed, unlike BattingSeasons)
    public List<AdvancedBattingSeason> AdvancedBattingSeasons { get; set; } = new();
    public List<AdvancedPitchingSeason> AdvancedPitchingSeasons { get; set; } = new();

    // Career pitching stats
    public CareerPitchingStats? PitchingStats { get; set; }

    // A player is primarily a pitcher if they have significant pitching innings
    public bool IsPitcher => PitchingStats != null && PitchingStats.InningsPitched > 0;

    // A two-way player has both pitching and batting season data (e.g. Babe Ruth)
    public bool IsTwoWayPlayer => IsPitcher && BattingSeasons.Count > 0;

    // Season-by-season records
    public List<SeasonBattingRecord> BattingSeasons { get; set; } = new();
    public List<SeasonPitchingRecord> PitchingSeasons { get; set; } = new();
    public List<SeasonFieldingRecord> FieldingSeasons { get; set; } = new();

    // Postseason records
    public List<PostseasonBattingRecord> PostseasonBattingSeasons { get; set; } = new();
    public List<PostseasonPitchingRecord> PostseasonPitchingSeasons { get; set; } = new();

    // Managerial career (player-managers and players who later managed)
    public List<ManagerSeasonRow> ManagerialSeasons { get; set; } = new();
    public bool HasManagerialCareer => ManagerialSeasons.Count > 0;

    public bool HasPostseason => PostseasonBattingSeasons.Count > 0 || PostseasonPitchingSeasons.Count > 0;

    // Career fielding totals per position, main position first
    public List<CareerFieldingRecord> FieldingByPosition => FieldingSeasons
        .GroupBy(f => f.Position)
        .Select(g => new CareerFieldingRecord
        {
            Position = g.Key,
            Games = g.Sum(f => f.Games),
            Putouts = g.Sum(f => f.Putouts),
            Assists = g.Sum(f => f.Assists),
            Errors = g.Sum(f => f.Errors),
            DoublePlays = g.Sum(f => f.DoublePlays)
        })
        .OrderByDescending(f => f.Games)
        .ToList();

    public CareerBattingStats? PostseasonBattingTotals => PostseasonBattingSeasons.Count == 0
        ? null
        : new CareerBattingStats
        {
            Games = PostseasonBattingSeasons.Sum(s => s.Games),
            AtBats = PostseasonBattingSeasons.Sum(s => s.AtBats),
            Runs = PostseasonBattingSeasons.Sum(s => s.Runs),
            Hits = PostseasonBattingSeasons.Sum(s => s.Hits),
            Doubles = PostseasonBattingSeasons.Sum(s => s.Doubles),
            Triples = PostseasonBattingSeasons.Sum(s => s.Triples),
            HomeRuns = PostseasonBattingSeasons.Sum(s => s.HomeRuns),
            Rbi = PostseasonBattingSeasons.Sum(s => s.Rbi),
            StolenBases = PostseasonBattingSeasons.Sum(s => s.StolenBases),
            Walks = PostseasonBattingSeasons.Sum(s => s.Walks),
            Strikeouts = PostseasonBattingSeasons.Sum(s => s.Strikeouts)
        };

    public CareerPitchingStats? PostseasonPitchingTotals => PostseasonPitchingSeasons.Count == 0
        ? null
        : new CareerPitchingStats
        {
            Games = PostseasonPitchingSeasons.Sum(s => s.Games),
            GamesStarted = PostseasonPitchingSeasons.Sum(s => s.GamesStarted),
            Wins = PostseasonPitchingSeasons.Sum(s => s.Wins),
            Losses = PostseasonPitchingSeasons.Sum(s => s.Losses),
            Saves = PostseasonPitchingSeasons.Sum(s => s.Saves),
            InningsPitched = PostseasonPitchingSeasons.Sum(s => s.InningsPitched),
            Hits = PostseasonPitchingSeasons.Sum(s => s.Hits),
            EarnedRuns = PostseasonPitchingSeasons.Sum(s => s.EarnedRuns),
            Walks = PostseasonPitchingSeasons.Sum(s => s.Walks),
            Strikeouts = PostseasonPitchingSeasons.Sum(s => s.Strikeouts)
        };

    // Awards and honors
    public List<AwardRecord> Awards { get; set; } = new();
    public List<AllStarRecord> AllStarAppearances { get; set; } = new();

    // 1959-1962 had two All-Star Games per year; selections count seasons,
    // not games, matching the Compare page.
    public int AllStarSelectionCount => AllStarAppearances.Select(a => a.Year).Distinct().Count();

    // Distinct selection years, newest first, for linking to /AllStar/{year}
    public List<short> AllStarYears => AllStarAppearances
        .Select(a => a.Year)
        .Distinct()
        .OrderByDescending(y => y)
        .ToList();

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
            IsPartialRecord = PlayerRecordFacts.IsPartialName(person.NameFirst),
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
                if (int.TryParse(person.BirthMonth, out var month) && int.TryParse(person.BirthDay, out var day) &&
                    int.TryParse(person.BirthYear, out var year))
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
                if (int.TryParse(person.DeathMonth, out var month) && int.TryParse(person.DeathDay, out var day) &&
                    int.TryParse(person.DeathYear, out var year))
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
    public string FormattedInningsPitched => $"{(int)InningsPitched}.{(int)((InningsPitched - (int)InningsPitched) * 3)}";
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
/// <summary>
/// Season batting row from the shared query layer: league-indexed rates and
/// season-relative qualification. NULL stats (unrecorded-era SO, missing
/// league context) render as an em dash rather than a fabricated number.
/// </summary>
public class AdvancedBattingSeason
{
    public short Year { get; set; }
    public string? LgId { get; set; }
    public long Pa { get; set; }
    public decimal? Iso { get; set; }
    public decimal? Babip { get; set; }
    public decimal? BbPct { get; set; }
    public decimal? KPct { get; set; }
    public decimal? OpsIndex { get; set; }
    public decimal? HrPer162 { get; set; }
    public bool Qualified { get; set; }

    public string FormattedIso => FormatRate(Iso);
    public string FormattedBabip => FormatRate(Babip);
    public string FormattedBbPct => BbPct?.ToString("0.0") ?? "—";
    public string FormattedKPct => KPct?.ToString("0.0") ?? "—";
    public string FormattedOpsIndex => OpsIndex?.ToString("0") ?? "—";
    public string FormattedHrPer162 => HrPer162?.ToString("0.0") ?? "—";

    internal static string FormatRate(decimal? value) => value?.ToString(".000") ?? "—";
}

/// <summary>
/// Season pitching row from the shared query layer.
/// </summary>
public class AdvancedPitchingSeason
{
    public short Year { get; set; }
    public string? LgId { get; set; }
    public decimal? Ip { get; set; }
    public decimal? K9 { get; set; }
    public decimal? Bb9 { get; set; }
    public decimal? Whip { get; set; }
    public bool Qualified { get; set; }

    public string FormattedIp => Ip?.ToString("0.0") ?? "—";
    public string FormattedK9 => K9?.ToString("0.00") ?? "—";
    public string FormattedBb9 => Bb9?.ToString("0.00") ?? "—";
    public string FormattedWhip => Whip?.ToString("0.000") ?? "—";
}

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
    public string FormattedInningsPitched => $"{(int)InningsPitched}.{(int)((InningsPitched - (int)InningsPitched) * 3)}";
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

    // Lahman award ids are mostly full names already; expand the terse or
    // abbreviation-style ids so the UI never shows a raw code.
    private static readonly Dictionary<string, string> FriendlyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MVP"] = "Most Valuable Player",
        ["ROY"] = "Rookie of the Year",
        ["CYA"] = "Cy Young Award",
        ["GG"] = "Gold Glove",
        ["SS"] = "Silver Slugger",
        ["WS MVP"] = "World Series MVP",
        ["ALCS MVP"] = "AL Championship Series MVP",
        ["NLCS MVP"] = "NL Championship Series MVP",
        ["TSN All-Star"] = "Sporting News All-Star",
        ["TSN Guide MVP"] = "Sporting News Guide MVP",
        ["TSN Major League Player of the Year"] = "Sporting News Player of the Year",
        ["TSN Pitcher of the Year"] = "Sporting News Pitcher of the Year",
        ["TSN Player of the Year"] = "Sporting News Player of the Year",
        ["TSN Fireman of the Year"] = "Sporting News Fireman of the Year",
        ["TSN Reliever of the Year"] = "Sporting News Reliever of the Year"
    };

    public string DisplayName => FriendlyNames.GetValueOrDefault(AwardId, AwardId);
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
/// Single season fielding record at one position
/// </summary>
public class SeasonFieldingRecord
{
    public short Year { get; set; }
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public string LgId { get; set; } = null!;
    public string Position { get; set; } = null!;
    public int Games { get; set; }
    public int Putouts { get; set; }
    public int Assists { get; set; }
    public int Errors { get; set; }
    public int DoublePlays { get; set; }

    public double FieldingPercentage => (Putouts + Assists + Errors) > 0
        ? (double)(Putouts + Assists) / (Putouts + Assists + Errors)
        : 0;

    public string FormattedPct => FieldingPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// Career fielding totals at one position
/// </summary>
public class CareerFieldingRecord
{
    public string Position { get; set; } = null!;
    public int Games { get; set; }
    public int Putouts { get; set; }
    public int Assists { get; set; }
    public int Errors { get; set; }
    public int DoublePlays { get; set; }

    public double FieldingPercentage => (Putouts + Assists + Errors) > 0
        ? (double)(Putouts + Assists) / (Putouts + Assists + Errors)
        : 0;

    public string FormattedPct => FieldingPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// Postseason batting line for one year and round
/// </summary>
public class PostseasonBattingRecord
{
    public short Year { get; set; }
    public string Round { get; set; } = null!;
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

    public string RoundName => PostseasonViewModel.RoundDisplayName(Round);
    public double BattingAverage => AtBats > 0 ? (double)Hits / AtBats : 0;
    public string FormattedAvg => BattingAverage.ToString(".000").TrimStart('0');
}

/// <summary>
/// Postseason pitching line for one year and round
/// </summary>
public class PostseasonPitchingRecord
{
    public short Year { get; set; }
    public string Round { get; set; } = null!;
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
    public int Walks { get; set; }
    public int Strikeouts { get; set; }

    public string RoundName => PostseasonViewModel.RoundDisplayName(Round);
    public double Era => InningsPitched > 0 ? (EarnedRuns * 9.0) / InningsPitched : 0;
    public string FormattedEra => Era.ToString("0.00");
    public string FormattedInningsPitched => $"{(int)InningsPitched}.{(int)((InningsPitched - (int)InningsPitched) * 3)}";
    public string WinLossRecord => $"{Wins}-{Losses}";
}

/// <summary>
/// Team a player played for
/// </summary>
public class TeamRecord
{
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public string? FranchId { get; set; }
    public short FirstYear { get; set; }
    public short LastYear { get; set; }
    public int Seasons { get; set; }
}