namespace odata_baseballstats.Entities;

public partial class AllstarFull
{
    public string PlayerId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string GameId { get; set; } = null!;

    public short? StartingPos { get; set; }

    public short? GameNum { get; set; }

    public short? Gp { get; set; }

    public virtual Person Player { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}