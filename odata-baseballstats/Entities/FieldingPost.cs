using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class FieldingPost
{
    public string PlayerId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string Round { get; set; } = null!;

    public string Pos { get; set; } = null!;

    public short? G { get; set; }

    public short? Gs { get; set; }

    public short? InnOuts { get; set; }

    public short? Po { get; set; }

    public short? A { get; set; }

    public short? E { get; set; }

    public short? Dp { get; set; }

    public short? Tp { get; set; }

    public short? Pb { get; set; }

    public short? Sb { get; set; }

    public short? Cs { get; set; }

    public virtual Person Player { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
