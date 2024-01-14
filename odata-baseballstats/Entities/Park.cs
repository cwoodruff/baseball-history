namespace odata_baseballstats.Entities;

public partial class Park
{
    public string ParkId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Alias { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
}
