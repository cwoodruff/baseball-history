using System;
using System.Collections.Generic;

namespace BaseballHistory.Data.Models;

public partial class Salaries
{
    public short YearId { get; set; }

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public string PlayerId { get; set; } = null!;

    public long? Salary { get; set; }

    // Navigation properties
    public virtual People Player { get; set; } = null!;
    public virtual Teams Team { get; set; } = null!;
}