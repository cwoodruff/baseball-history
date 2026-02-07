using System;
using System.Collections.Generic;

namespace baseball_history_web.Models;

public partial class People
{
    public int Id { get; set; }

    public string PlayerId { get; set; } = null!;

    public string? BirthYear { get; set; }

    public string? BirthMonth { get; set; }

    public string? BirthDay { get; set; }

    public string? BirthCity { get; set; }

    public string? BirthCountry { get; set; }

    public string? BirthState { get; set; }

    public string? DeathYear { get; set; }

    public string? DeathMonth { get; set; }

    public string? DeathDay { get; set; }

    public string? DeathCountry { get; set; }

    public string? DeathState { get; set; }

    public string? DeathCity { get; set; }

    public string? NameFirst { get; set; }

    public string? NameLast { get; set; }

    public string? NameGiven { get; set; }

    public string? Weight { get; set; }

    public string? Height { get; set; }

    public string? Bats { get; set; }

    public string? Throws { get; set; }

    public DateOnly? Debut { get; set; }

    public string? BbrefId { get; set; }

    public DateOnly? FinalGame { get; set; }

    public string? RetroId { get; set; }

    // Navigation properties
    public virtual ICollection<AllstarFull> AllstarFulls { get; set; } = new List<AllstarFull>();
    public virtual ICollection<Appearances> Appearances { get; set; } = new List<Appearances>();
    public virtual ICollection<AwardsManagers> AwardsManagers { get; set; } = new List<AwardsManagers>();
    public virtual ICollection<AwardsPlayers> AwardsPlayers { get; set; } = new List<AwardsPlayers>();
    public virtual ICollection<AwardsShareManagers> AwardsShareManagers { get; set; } = new List<AwardsShareManagers>();
    public virtual ICollection<AwardsSharePlayers> AwardsSharePlayers { get; set; } = new List<AwardsSharePlayers>();
    public virtual ICollection<Batting> Battings { get; set; } = new List<Batting>();
    public virtual ICollection<BattingPost> BattingPosts { get; set; } = new List<BattingPost>();
    public virtual ICollection<CollegePlaying> CollegePlayings { get; set; } = new List<CollegePlaying>();
    public virtual ICollection<Fielding> Fieldings { get; set; } = new List<Fielding>();
    public virtual ICollection<FieldingOf> FieldingOfs { get; set; } = new List<FieldingOf>();
    public virtual ICollection<FieldingOfsplit> FieldingOfsplits { get; set; } = new List<FieldingOfsplit>();
    public virtual ICollection<FieldingPost> FieldingPosts { get; set; } = new List<FieldingPost>();
    public virtual ICollection<HallOfFame> HallOfFames { get; set; } = new List<HallOfFame>();
    public virtual ICollection<Managers> Managers { get; set; } = new List<Managers>();
    public virtual ICollection<ManagersHalf> ManagersHalves { get; set; } = new List<ManagersHalf>();
    public virtual ICollection<Pitching> Pitchings { get; set; } = new List<Pitching>();
    public virtual ICollection<PitchingPost> PitchingPosts { get; set; } = new List<PitchingPost>();
    public virtual ICollection<Salaries> Salaries { get; set; } = new List<Salaries>();
}
