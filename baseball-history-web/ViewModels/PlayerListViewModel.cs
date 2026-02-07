using baseball_history_web.Models;

namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for displaying a list of players with pagination
/// </summary>
public class PlayerListViewModel
{
    public List<PlayerSummary> Players { get; set; } = new();
    public string? CurrentLetter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalPlayers { get; set; }
    public int PageSize { get; set; } = 50;
    public List<char> AvailableLetters { get; set; } = new();
}

/// <summary>
/// Summary view of a player for list display
/// </summary>
public class PlayerSummary
{
    public string PlayerId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? BirthYear { get; set; }
    public string? DebutYear { get; set; }
    public string? FinalYear { get; set; }
    public bool IsInHallOfFame { get; set; }
    public int? TotalGames { get; set; }
    public int? TotalHits { get; set; }
    public int? TotalHomeRuns { get; set; }
    public string? PrimaryPosition { get; set; }
    public string? LastTeamId { get; set; }

    public static PlayerSummary FromPeople(People person, bool isHof = false, int? games = null, int? hits = null, int? hrs = null)
    {
        return new PlayerSummary
        {
            PlayerId = person.PlayerId,
            FirstName = person.NameFirst,
            LastName = person.NameLast,
            FullName = $"{person.NameFirst} {person.NameLast}".Trim(),
            BirthYear = person.BirthYear,
            DebutYear = person.Debut?.Year.ToString(),
            FinalYear = person.FinalGame?.Year.ToString(),
            IsInHallOfFame = isHof,
            TotalGames = games,
            TotalHits = hits,
            TotalHomeRuns = hrs
        };
    }
}
