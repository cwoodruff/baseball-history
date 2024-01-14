using System;
using System.Collections.Generic;

namespace odata_baseballstats.Entities;

public partial class Person
{
    public string PlayerId { get; set; } = null!;

    public short? BirthYear { get; set; }

    public short? BirthMonth { get; set; }

    public short? BirthDay { get; set; }

    public string? BirthCountry { get; set; }

    public string? BirthState { get; set; }

    public string? BirthCity { get; set; }

    public short? DeathYear { get; set; }

    public short? DeathMonth { get; set; }

    public short? DeathDay { get; set; }

    public string? DeathCountry { get; set; }

    public string? DeathState { get; set; }

    public string? DeathCity { get; set; }

    public string? NameFirst { get; set; }

    public string? NameLast { get; set; }

    public string? NameGiven { get; set; }

    public short? Weight { get; set; }

    public short? Height { get; set; }

    public string? Bats { get; set; }

    public string? Throws { get; set; }

    public DateOnly? Debut { get; set; }

    public DateOnly? FinalGame { get; set; }

    public virtual ICollection<AllstarFull> AllstarFulls { get; set; } = new List<AllstarFull>();

    public virtual ICollection<Appearance> Appearances { get; set; } = new List<Appearance>();

    public virtual ICollection<AwardsManager> AwardsManagers { get; set; } = new List<AwardsManager>();

    public virtual ICollection<AwardsPlayer> AwardsPlayers { get; set; } = new List<AwardsPlayer>();

    public virtual ICollection<AwardsShareManager> AwardsShareManagers { get; set; } = new List<AwardsShareManager>();

    public virtual ICollection<AwardsSharePlayer> AwardsSharePlayers { get; set; } = new List<AwardsSharePlayer>();

    public virtual ICollection<BattingPost> BattingPosts { get; set; } = new List<BattingPost>();

    public virtual ICollection<Batting> Battings { get; set; } = new List<Batting>();

    public virtual ICollection<CollegePlaying> CollegePlayings { get; set; } = new List<CollegePlaying>();

    public virtual ICollection<FieldingOf> FieldingOfs { get; set; } = new List<FieldingOf>();

    public virtual ICollection<FieldingOfsplit> FieldingOfsplits { get; set; } = new List<FieldingOfsplit>();

    public virtual ICollection<FieldingPost> FieldingPosts { get; set; } = new List<FieldingPost>();

    public virtual ICollection<Fielding> Fieldings { get; set; } = new List<Fielding>();

    public virtual ICollection<HallOfFame> HallOfFames { get; set; } = new List<HallOfFame>();

    public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();

    public virtual ICollection<ManagersHalf> ManagersHalves { get; set; } = new List<ManagersHalf>();

    public virtual ICollection<PitchingPost> PitchingPosts { get; set; } = new List<PitchingPost>();

    public virtual ICollection<Pitching> Pitchings { get; set; } = new List<Pitching>();

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
}
