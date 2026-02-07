namespace baseball_history_web.Models;

public partial class FieldingPost
{
    public string PlayerId { get; set; } = null!;

    public short YearId { get; set; }

    public string? TeamId { get; set; }

    public string? LgId { get; set; }

    public string Round { get; set; } = null!;

    public string Pos { get; set; } = null!;

    public short? G { get; set; }

    public string? Gs { get; set; }

    public short? InnOuts { get; set; }

    public short? Po { get; set; }

    public short? A { get; set; }

    public short? E { get; set; }

    public short? Dp { get; set; }

    public string? Tp { get; set; }

    public string? Pb { get; set; }

    public string? Sb { get; set; }

    public string? Cs { get; set; }
}
