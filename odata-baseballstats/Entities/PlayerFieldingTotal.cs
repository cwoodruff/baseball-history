using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class PlayerFieldingTotal
{
    public string PlayerId { get; set; } = null!;

    public string Pos { get; set; } = null!;

    public int? G { get; set; }

    public int? Gs { get; set; }

    public int? InnOuts { get; set; }

    public int? Po { get; set; }

    public int? A { get; set; }

    public int? E { get; set; }

    public int? Dp { get; set; }

    public int? Pb { get; set; }

    public int? Wp { get; set; }

    public int? Sb { get; set; }

    public int? Cs { get; set; }

    public double? Zr { get; set; }
}
