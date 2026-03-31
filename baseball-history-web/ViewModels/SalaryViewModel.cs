namespace baseball_history_web.ViewModels;

public class SalaryViewModel
{
    public List<SalaryEntry> Salaries { get; set; } = new();
    public int? SelectedYear { get; set; }
    public string? SelectedTeam { get; set; }

    public List<int> AvailableYears { get; set; } = new();
    public List<TeamOption> AvailableTeams { get; set; } = new();

    public int TotalEntries { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 50;

    // Team payroll summary (when filtering by team+year)
    public long? TeamPayroll { get; set; }
    public string? TeamName { get; set; }
}

public class SalaryEntry
{
    public short Year { get; set; }
    public string PlayerId { get; set; } = null!;
    public string PlayerName { get; set; } = null!;
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
    public string LgId { get; set; } = null!;
    public long Salary { get; set; }
    public bool IsInHallOfFame { get; set; }

    public string FormattedSalary => Salary.ToString("C0");
}

public class TeamOption
{
    public string TeamId { get; set; } = null!;
    public string? TeamName { get; set; }
}
