using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class FieldingOf
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public byte Stint { get; set; }

    public string? Glf { get; set; }

    public string? Gcf { get; set; }

    public string? Grf { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
}