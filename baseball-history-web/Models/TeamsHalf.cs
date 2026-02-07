using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class TeamsHalf
{
    public short YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public byte Half { get; set; }

    public string? DivId { get; set; }

    public string? DivWin { get; set; }

    public byte? Rank { get; set; }

    public string? G { get; set; }

    public string? W { get; set; }

    public string? L { get; set; }

    // Navigation properties
    public virtual Teams Team { get; set; } = null!;
}
