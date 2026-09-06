namespace BaseballHistory.Data.Models;

// Keyless entities over the shared query-layer views defined in
// docs/qualification_and_league_index.sql. Sums of smallint surface as bigint
// and ROUND()/CEIL() as numeric, hence the long?/decimal? property types.

/// <summary>One row per player-season from v_player_season_rates.</summary>
public class PlayerSeasonRate
{
    public string PlayerId { get; set; } = null!;
    public short YearId { get; set; }
    public string? LgId { get; set; }
    public long? G { get; set; }
    public long? Pa { get; set; }
    public long? Ab { get; set; }
    public long? H { get; set; }
    public long? Hr { get; set; }
    public long? Tb { get; set; }
    public decimal? TeamGamesWtd { get; set; }
    public decimal? PaThreshold { get; set; }
    public bool? Qualified { get; set; }
    public decimal? Avg { get; set; }
    public decimal? Obp { get; set; }
    public decimal? Slg { get; set; }
    public decimal? Iso { get; set; }
    public decimal? Babip { get; set; }
    public decimal? BbPct { get; set; }
    public decimal? KPct { get; set; }
    public decimal? OpsIndex { get; set; }
    public decimal? HrPer162 { get; set; }
    public decimal? HPer162 { get; set; }
    public decimal? RbiPer162 { get; set; }
}

/// <summary>One row per player from v_career_batting.</summary>
public class CareerBattingSummary
{
    public string PlayerId { get; set; } = null!;
    public short? FirstYear { get; set; }
    public short? LastYear { get; set; }
    public long? Seasons { get; set; }
    public long? Pa { get; set; }
    public long? Ab { get; set; }
    public decimal? CareerPaThreshold { get; set; }
    public bool? Qualified { get; set; }
    public decimal? PctOfThreshold { get; set; }
    public decimal? Avg { get; set; }
    public decimal? Obp { get; set; }
    public decimal? Slg { get; set; }
    public decimal? OpsIndex { get; set; }
}

/// <summary>One row per player-season from v_player_season_pitching.</summary>
public class PlayerSeasonPitchingAdvanced
{
    public string PlayerId { get; set; } = null!;
    public short YearId { get; set; }
    public string? LgId { get; set; }
    public long? G { get; set; }
    public long? Gs { get; set; }
    public long? Ipouts { get; set; }
    public decimal? Ip { get; set; }
    public decimal? TeamGamesWtd { get; set; }
    public decimal? IpoutsThreshold { get; set; }
    public bool? Qualified { get; set; }
    public decimal? Era { get; set; }
    public decimal? Whip { get; set; }
    public decimal? K9 { get; set; }
    public decimal? Bb9 { get; set; }
}
