using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class AllstarFull
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string? GameNum { get; set; }

    public string GameId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public byte? Gp { get; set; }

    public string? StartingPos { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
}
