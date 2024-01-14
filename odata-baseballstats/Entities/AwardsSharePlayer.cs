namespace odata_baseballstats.Entities;

public partial class AwardsSharePlayer
{
    public string PlayerId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string AwardId { get; set; } = null!;

    public double? PointsWon { get; set; }

    public short? PointsMax { get; set; }

    public double? VotesFirst { get; set; }

    public virtual Person Player { get; set; } = null!;
}
