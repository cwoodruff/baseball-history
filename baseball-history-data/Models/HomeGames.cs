using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class HomeGames
{
    public short Yearkey { get; set; }

    public string Leaguekey { get; set; } = null!;

    public string Teamkey { get; set; } = null!;

    public string Parkkey { get; set; } = null!;

    public DateOnly? Spanfirst { get; set; }

    public DateOnly? Spanlast { get; set; }

    public short? Games { get; set; }

    public short? Openings { get; set; }

    public int? Attendance { get; set; }

    // Navigation properties
    public virtual Parks Park { get; set; } = null!;
    public virtual Teams Team { get; set; } = null!;
}