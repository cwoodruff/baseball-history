using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class HallOfFame
{
    public string PlayerId { get; set; } = null!;

    public short Yearid { get; set; }

    public string VotedBy { get; set; } = null!;

    public string? Ballots { get; set; }

    public string? Needed { get; set; }

    public string? Votes { get; set; }

    public string? Inducted { get; set; }

    public string? Category { get; set; }

    public string? NeededNote { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
}