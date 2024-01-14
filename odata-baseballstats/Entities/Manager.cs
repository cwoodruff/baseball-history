using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class Manager
{
    public string PlayerId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public short Inseason { get; set; }

    public short? G { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public short? Rank { get; set; }

    public string? PlyrMgr { get; set; }

    public virtual Person Player { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
