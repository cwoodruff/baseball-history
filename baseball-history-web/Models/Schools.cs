using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class Schools
{
    public string SchoolId { get; set; } = null!;

    public string? NameFull { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    // Navigation properties
    public virtual ICollection<CollegePlaying> CollegePlayings { get; set; } = new List<CollegePlaying>();
}
