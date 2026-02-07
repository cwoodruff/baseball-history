using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class AwardsPlayers
{
    public string PlayerId { get; set; } = null!;

    public string AwardId { get; set; } = null!;

    public int YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string Tie { get; set; } = null!;

    public string Notes { get; set; } = null!;
}
