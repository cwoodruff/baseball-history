namespace odata_baseballstats.Entities;

public partial class AwardsManager
{
    public string PlayerId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string AwardId { get; set; } = null!;

    public string? Tie { get; set; }

    public string? Notes { get; set; }

    public virtual Person Player { get; set; } = null!;
}