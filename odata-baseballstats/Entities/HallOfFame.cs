namespace odata_baseballstats.Entities;

public partial class HallOfFame
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string VotedBy { get; set; } = null!;

    public short? Ballots { get; set; }

    public short? Needed { get; set; }

    public short? Votes { get; set; }

    public string? Inducted { get; set; }

    public string? Category { get; set; }

    public string? NeededNote { get; set; }

    public virtual Person Player { get; set; } = null!;
}
