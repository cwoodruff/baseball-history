namespace odata_baseballstats.Entities;

public partial class Team
{
    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public short YearId { get; set; }

    public string? FranchId { get; set; }

    public string? ParkId { get; set; }

    public string? DivId { get; set; }

    public short? Rank { get; set; }

    public short? G { get; set; }

    public short? Ghome { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public string? DivWin { get; set; }

    public string? Wcwin { get; set; }

    public string? LgWin { get; set; }

    public string? Wswin { get; set; }

    public short? R { get; set; }

    public short? Ab { get; set; }

    public short? H { get; set; }

    public short? _2b { get; set; }

    public short? _3b { get; set; }

    public short? Hr { get; set; }

    public short? Bb { get; set; }

    public short? So { get; set; }

    public short? Sb { get; set; }

    public short? Cs { get; set; }

    public short? Hbp { get; set; }

    public short? Sf { get; set; }

    public short? Ra { get; set; }

    public short? Er { get; set; }

    public double? Era { get; set; }

    public short? Cg { get; set; }

    public short? Sho { get; set; }

    public short? Sv { get; set; }

    public int? Ipouts { get; set; }

    public short? Ha { get; set; }

    public short? Hra { get; set; }

    public short? Bba { get; set; }

    public short? Soa { get; set; }

    public int? E { get; set; }

    public int? Dp { get; set; }

    public double? Fp { get; set; }

    public string? Name { get; set; }

    public string? Park { get; set; }

    public int? Attendance { get; set; }

    public int? Bpf { get; set; }

    public int? Ppf { get; set; }

    public virtual ICollection<AllstarFull> AllstarFulls { get; set; } = new List<AllstarFull>();

    public virtual ICollection<Appearance> Appearances { get; set; } = new List<Appearance>();

    public virtual ICollection<BattingPost> BattingPosts { get; set; } = new List<BattingPost>();

    public virtual ICollection<Batting> Battings { get; set; } = new List<Batting>();

    public virtual ICollection<FieldingOfsplit> FieldingOfsplits { get; set; } = new List<FieldingOfsplit>();

    public virtual ICollection<FieldingPost> FieldingPosts { get; set; } = new List<FieldingPost>();

    public virtual ICollection<Fielding> Fieldings { get; set; } = new List<Fielding>();

    public virtual TeamsFranchise? Franch { get; set; }

    public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();

    public virtual ICollection<ManagersHalf> ManagersHalves { get; set; } = new List<ManagersHalf>();

    public virtual Park? ParkNavigation { get; set; }

    public virtual ICollection<PitchingPost> PitchingPosts { get; set; } = new List<PitchingPost>();

    public virtual ICollection<Pitching> Pitchings { get; set; } = new List<Pitching>();

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

    public virtual ICollection<SeriesPost> SeriesPostTeamNavigations { get; set; } = new List<SeriesPost>();

    public virtual ICollection<SeriesPost> SeriesPostTeams { get; set; } = new List<SeriesPost>();

    public virtual ICollection<TeamsHalf> TeamsHalves { get; set; } = new List<TeamsHalf>();
}
