using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class AwardsSharePlayers
{
    public string AwardId { get; set; } = null!;

    public short YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string PlayerId { get; set; } = null!;

    public short? PointsWon { get; set; }

    public short? PointsMax { get; set; }

    public string? VotesFirst { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
}