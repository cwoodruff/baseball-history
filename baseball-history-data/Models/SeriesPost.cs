using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class SeriesPost
{
    public short YearId { get; set; }

    public string Round { get; set; } = null!;

    public string TeamIdwinner { get; set; } = null!;

    public string LgIdwinner { get; set; } = null!;

    public string? TeamIdloser { get; set; }

    public string? LgIdloser { get; set; }

    public short? Wins { get; set; }

    public short? Losses { get; set; }

    public short? Ties { get; set; }

    // Navigation properties
    public virtual Teams TeamWinner { get; set; } = null!;
    public virtual Teams? TeamLoser { get; set; }
}