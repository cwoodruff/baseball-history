namespace odata_baseballstats.Entities;

public partial class TeamsFranchise
{
    public string FranchId { get; set; } = null!;

    public string? FranchName { get; set; }

    public string? Active { get; set; }

    public string? Naassoc { get; set; }

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}