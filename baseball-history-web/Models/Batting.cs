using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class Batting
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public byte Stint { get; set; }

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short? G { get; set; }

    public short? Ab { get; set; }

    public short? R { get; set; }

    public short? H { get; set; }

    public short? _2b { get; set; }

    public short? _3b { get; set; }

    public short? Hr { get; set; }

    public string? Rbi { get; set; }

    public string? Sb { get; set; }

    public string? Cs { get; set; }

    public short? Bb { get; set; }

    public string? So { get; set; }

    public string? Ibb { get; set; }

    public string? Hbp { get; set; }

    public string? Sh { get; set; }

    public string? Sf { get; set; }

    public string? Gidp { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
    public virtual Teams Team { get; set; } = null!;
}