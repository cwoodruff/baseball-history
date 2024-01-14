namespace odata_baseballstats.Entities;

public partial class CollegePlaying
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string SchoolId { get; set; } = null!;

    public virtual Person Player { get; set; } = null!;

    public virtual School School { get; set; } = null!;
}