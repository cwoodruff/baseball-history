using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class PitchingPost
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string Round { get; set; } = null!;

    public string? TeamId { get; set; }

    public string? LgId { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public short? G { get; set; }

    public short? Gs { get; set; }

    public short? Cg { get; set; }

    public short? Sho { get; set; }

    public short? Sv { get; set; }

    public int? Ipouts { get; set; }

    public short? H { get; set; }

    public short? Er { get; set; }

    public short? Hr { get; set; }

    public short? Bb { get; set; }

    public short? So { get; set; }

    public string? Baopp { get; set; }

    public string? Era { get; set; }

    public string? Ibb { get; set; }

    public string? Wp { get; set; }

    public string? Hbp { get; set; }

    public string? Bk { get; set; }

    public string? Bfp { get; set; }

    public string? Gf { get; set; }

    public short? R { get; set; }

    public string? Sh { get; set; }

    public string? Sf { get; set; }

    public string? Gidp { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
    public virtual Teams? Team { get; set; }
}
