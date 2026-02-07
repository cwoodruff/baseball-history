using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class CollegePlaying
{
    public string PlayerId { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public string YearId { get; set; } = null!;
}
