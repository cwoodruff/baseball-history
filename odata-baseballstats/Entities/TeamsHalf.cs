namespace odata_baseballstats.Entities;

public partial class TeamsHalf
{
    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string Half { get; set; } = null!;

    public string? DivId { get; set; }

    public string? DivWin { get; set; }

    public short? Rank { get; set; }

    public short? G { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public virtual Team Team { get; set; } = null!;
}