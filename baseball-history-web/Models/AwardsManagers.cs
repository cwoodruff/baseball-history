using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class AwardsManagers
{
    public string PlayerId { get; set; } = null!;

    public string AwardId { get; set; } = null!;

    public short YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string Tie { get; set; } = null!;

    public string? Notes { get; set; }
}
