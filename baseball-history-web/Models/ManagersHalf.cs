namespace baseball_history_web.Models;

public partial class ManagersHalf
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string TeamId { get; set; } = null!;

    public string? LgId { get; set; }

    public byte? Inseason { get; set; }

    public byte Half { get; set; }

    public short? G { get; set; }

    public short? W { get; set; }

    public short? L { get; set; }

    public byte? Rank { get; set; }
}
