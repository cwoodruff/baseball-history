namespace baseball_history_web.Models;

public partial class Fielding
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public byte Stint { get; set; }

    public string? TeamId { get; set; }

    public string? LgId { get; set; }

    public string Pos { get; set; } = null!;

    public short? G { get; set; }

    public string? Gs { get; set; }

    public string? InnOuts { get; set; }

    public string? Po { get; set; }

    public string? A { get; set; }

    public string? E { get; set; }

    public string? Dp { get; set; }

    public string? Pb { get; set; }

    public string? Wp { get; set; }

    public string? Sb { get; set; }

    public string? Cs { get; set; }

    public string? Zr { get; set; }
}
