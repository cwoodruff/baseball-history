using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class Appearances
{
    public short YearId { get; set; }

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public string PlayerId { get; set; } = null!;

    public short? GAll { get; set; }

    public string? Gs { get; set; }

    public string? GBatting { get; set; }

    public string? GDefense { get; set; }

    public string? GP { get; set; }

    public string? GC { get; set; }

    public string? G1b { get; set; }

    public string? G2b { get; set; }

    public string? G3b { get; set; }

    public string? GSs { get; set; }

    public string? GLf { get; set; }

    public string? GCf { get; set; }

    public string? GRf { get; set; }

    public string? GOf { get; set; }

    public string? GDh { get; set; }

    public string? GPh { get; set; }

    public string? GPr { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
    public virtual Teams Team { get; set; } = null!;
}