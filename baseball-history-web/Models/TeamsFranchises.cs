namespace baseball_history_web.Models;

public partial class TeamsFranchises
{
    public string FranchId { get; set; } = null!;

    public string? FranchName { get; set; }

    public string? Active { get; set; }

    public string? Naassoc { get; set; }
}
