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

    // Navigation properties
    public virtual TeamsFranchises? Franchise { get; set; }
    public virtual ICollection<Appearances> Appearances { get; set; } = new List<Appearances>();
    public virtual ICollection<Batting> Battings { get; set; } = new List<Batting>();
    public virtual ICollection<BattingPost> BattingPosts { get; set; } = new List<BattingPost>();
    public virtual ICollection<Fielding> Fieldings { get; set; } = new List<Fielding>();
    public virtual ICollection<FieldingOfsplit> FieldingOfsplits { get; set; } = new List<FieldingOfsplit>();
    public virtual ICollection<FieldingPost> FieldingPosts { get; set; } = new List<FieldingPost>();
    public virtual ICollection<HomeGames> HomeGames { get; set; } = new List<HomeGames>();
    public virtual ICollection<Managers> Managers { get; set; } = new List<Managers>();
    public virtual ICollection<ManagersHalf> ManagersHalves { get; set; } = new List<ManagersHalf>();
    public virtual ICollection<Pitching> Pitchings { get; set; } = new List<Pitching>();
    public virtual ICollection<PitchingPost> PitchingPosts { get; set; } = new List<PitchingPost>();
    public virtual ICollection<Salaries> Salaries { get; set; } = new List<Salaries>();
    public virtual ICollection<SeriesPost> SeriesPostsWon { get; set; } = new List<SeriesPost>();
    public virtual ICollection<SeriesPost> SeriesPostsLost { get; set; } = new List<SeriesPost>();
    public virtual ICollection<TeamsHalf> TeamsHalves { get; set; } = new List<TeamsHalf>();
}
