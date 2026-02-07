namespace baseball_history_web.Models;

public partial class Salaries
{
    public short YearId { get; set; }

    public string TeamId { get; set; } = null!;

    public string LgId { get; set; } = null!;

    public string PlayerId { get; set; } = null!;

    public long? Salary { get; set; }
}
