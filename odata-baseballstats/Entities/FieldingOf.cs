namespace odata_baseballstats.Entities;

public partial class FieldingOf
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public short Stint { get; set; }

    public short? Glf { get; set; }

    public short? Gcf { get; set; }

    public short? Grf { get; set; }

    public virtual Person Player { get; set; } = null!;
}
