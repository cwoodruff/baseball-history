using baseball_history_web.Services;

namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for the Negro Leagues hub index
/// </summary>
public class NegroLeaguesHubViewModel
{
    public List<NegroLeagueCard> Leagues { get; set; } = new();
}

/// <summary>
/// One league card on the hub index
/// </summary>
public class NegroLeagueCard
{
    public NegroLeagueInfo Info { get; set; } = null!;
    public int TeamSeasons { get; set; }
    public int ClubCount { get; set; }
}

/// <summary>
/// ViewModel for a single league's detail page
/// </summary>
public class NegroLeagueDetailViewModel
{
    public NegroLeagueInfo Info { get; set; } = null!;

    /// <summary>Seasons, most recent first</summary>
    public List<NegroLeagueSeasonSummary> Seasons { get; set; } = new();

    /// <summary>Clubs that played in this league, by pennants then seasons</summary>
    public List<NegroLeagueClub> Clubs { get; set; } = new();
}

/// <summary>
/// One season row on a league detail page
/// </summary>
public class NegroLeagueSeasonSummary
{
    public short Year { get; set; }
    public int TeamCount { get; set; }
    public string? ChampionTeamId { get; set; }
    public string? ChampionName { get; set; }
}

/// <summary>
/// A club's aggregate record within one league
/// </summary>
public class NegroLeagueClub
{
    public string TeamId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short FirstYear { get; set; }
    public short LastYear { get; set; }
    public int Seasons { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Pennants { get; set; }

    public string YearsLabel => FirstYear == LastYear ? FirstYear.ToString() : $"{FirstYear}–{LastYear}";
    public string Record => $"{Wins}-{Losses}";

    public double WinningPercentage => (Wins + Losses) > 0 ? (double)Wins / (Wins + Losses) : 0;
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
}

/// <summary>
/// ViewModel for a league season page: standings plus leaders
/// </summary>
public class NegroLeagueSeasonViewModel
{
    public NegroLeagueInfo Info { get; set; } = null!;
    public short Year { get; set; }

    /// <summary>Standings ordered by rank, then wins</summary>
    public List<NegroLeagueStandingsRow> Standings { get; set; } = new();

    public List<short> AvailableYears { get; set; } = new();

    public List<LeaderLine> BattingAverageLeaders { get; set; } = new();
    public List<LeaderLine> HomeRunLeaders { get; set; } = new();
    public List<LeaderLine> EraLeaders { get; set; } = new();
    public List<LeaderLine> WinLeaders { get; set; } = new();

    public short? PreviousYear
    {
        get
        {
            var previous = AvailableYears.Where(y => y < Year).ToList();
            return previous.Count > 0 ? previous.Max() : null;
        }
    }

    public short? NextYear
    {
        get
        {
            var next = AvailableYears.Where(y => y > Year).ToList();
            return next.Count > 0 ? next.Min() : null;
        }
    }
}

/// <summary>
/// One team's line in a league season standings table
/// </summary>
public class NegroLeagueStandingsRow
{
    public string TeamId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public byte? Rank { get; set; }
    public short Wins { get; set; }
    public short Losses { get; set; }
    public short? Games { get; set; }
    public bool WonPennant { get; set; }
    public double? GamesBehind { get; set; }

    public double WinningPercentage => (Wins + Losses) > 0 ? (double)Wins / (Wins + Losses) : 0;
    public string FormattedWinPct => WinningPercentage.ToString(".000").TrimStart('0');
    public string Record => $"{Wins}-{Losses}";

    public string FormattedGamesBehind =>
        GamesBehind switch
        {
            null => "—",
            <= 0 => "—",
            var gb => gb.Value % 1 == 0 ? gb.Value.ToString("0") : gb.Value.ToString("0.0")
        };

    /// <summary>
    /// Standard games-behind: ((leaderW - W) + (L - leaderL)) / 2, computed
    /// against the team leading by winning percentage.
    /// </summary>
    public static void ComputeGamesBehind(List<NegroLeagueStandingsRow> standings)
    {
        var leader = standings
            .OrderByDescending(s => s.WinningPercentage)
            .ThenByDescending(s => s.Wins)
            .FirstOrDefault();
        if (leader == null)
            return;

        foreach (var row in standings)
        {
            row.GamesBehind = ((leader.Wins - row.Wins) + (row.Losses - leader.Losses)) / 2.0;
        }
    }
}

/// <summary>
/// One player's line in a compact leaders table
/// </summary>
public class LeaderLine
{
    public int Rank { get; set; }
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public string? TeamName { get; set; }
    public string Value { get; set; } = null!;
    public bool IsInHallOfFame { get; set; }
}
