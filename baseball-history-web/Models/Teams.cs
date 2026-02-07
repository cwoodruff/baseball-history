using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class Teams
{
    public short YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string? FranchId { get; set; }

    public string? DivId { get; set; }

    public byte? Rank { get; set; }

    public short? G { get; set; }

    public string? Ghome { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public string? DivWin { get; set; }

    public string? Wcwin { get; set; }

    public string? LgWin { get; set; }

    public string? Wswin { get; set; }

    public string? R { get; set; }

    public string? Ab { get; set; }

    public string? H { get; set; }

    public string? _2b { get; set; }

    public string? _3b { get; set; }

    public string? Hr { get; set; }

    public string? Bb { get; set; }

    public string? So { get; set; }

    public string? Sb { get; set; }

    public string? Cs { get; set; }

    public string? Hbp { get; set; }

    public string? Sf { get; set; }

    public string? Ra { get; set; }

    public string? Er { get; set; }

    public string? Era { get; set; }

    public string? Cg { get; set; }

    public string? Sho { get; set; }

    public string? Sv { get; set; }

    public string? Ipouts { get; set; }

    public string? Ha { get; set; }

    public string? Hra { get; set; }

    public string? Bba { get; set; }

    public string? Soa { get; set; }

    public string? E { get; set; }

    public string? Dp { get; set; }

    public string? Fp { get; set; }

    public string? Name { get; set; }

    public string? Park { get; set; }

    public string? Attendance { get; set; }

    public string? Bpf { get; set; }

    public string? Ppf { get; set; }

    public string? TeamIdbr { get; set; }

    public string? TeamIdlahman45 { get; set; }

    public string? TeamIdretro { get; set; }
}
