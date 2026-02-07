namespace baseball_history_web.Models;

public partial class CollegePlaying
{
    public string PlayerId { get; set; } = null!;

    public string? SchoolId { get; set; }

    public string? YearId { get; set; }
}
