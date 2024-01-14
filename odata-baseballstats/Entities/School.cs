namespace odata_baseballstats.Entities;

public partial class School
{
    public string SchoolId { get; set; } = null!;

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<CollegePlaying> CollegePlayings { get; set; } = new List<CollegePlaying>();
}