using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class SeriesPost
{
    public string TeamIdwinner { get; set; } = null!;

    public string LgIdwinner { get; set; } = null!;

    public short YearId { get; set; }

    public string Round { get; set; } = null!;

    public string? TeamIdloser { get; set; }

    public string? LgIdloser { get; set; }

    public short? Wins { get; set; }

    public short? Losses { get; set; }

    public short? Ties { get; set; }

    public virtual Team? Team { get; set; }

    public virtual Team TeamNavigation { get; set; } = null!;
}
