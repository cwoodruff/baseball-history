namespace odata_baseballstats.Entities;

public partial class HomeGame
{
    public short YearId { get; set; }

    public string LgId { get; set; } = null!;

    public string TeamId { get; set; } = null!;

    public string ParkId { get; set; } = null!;

    public DateOnly? SpanFirst { get; set; }

    public DateOnly? SpanLast { get; set; }

    public short? Games { get; set; }

    public short? Openings { get; set; }

    public int? Attendance { get; set; }
}