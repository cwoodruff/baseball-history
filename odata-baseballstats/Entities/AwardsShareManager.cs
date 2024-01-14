using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class AwardsShareManager
{
    public string PlayerId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string AwardId { get; set; } = null!;

    public short? PointsWon { get; set; }

    public short? PointsMax { get; set; }

    public short? VotesFirst { get; set; }

    public virtual Person Player { get; set; } = null!;
}
