namespace baseball_history_web.Models;

public partial class HomeGames
{
    public int? Yearkey { get; set; }

    public string? Leaguekey { get; set; }

    public string? Teamkey { get; set; }

    public string? Parkkey { get; set; }

    public DateOnly? Spanfirst { get; set; }

    public DateOnly? Spanlast { get; set; }

    public short? Games { get; set; }

    public short? Openings { get; set; }

    public int? Attendance { get; set; }
}
