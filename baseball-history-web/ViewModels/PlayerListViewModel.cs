using BaseballHistory.Data.Models;

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

    // Filters (issue #42)
    public string? SearchQuery { get; set; }
    public string? Position { get; set; }
    public int? Era { get; set; }
    public string SortBy { get; set; } = "name";

    public bool HasActiveFilters => !string.IsNullOrWhiteSpace(SearchQuery) ||
                                    !string.IsNullOrEmpty(Position) ||
                                    Era.HasValue ||
                                    SortBy != "name";

    public static readonly IReadOnlyList<(string Value, string Label)> PositionOptions = new[]
    {
        ("P", "Pitcher"),
        ("C", "Catcher"),
        ("1B", "First Base"),
        ("2B", "Second Base"),
        ("3B", "Third Base"),
        ("SS", "Shortstop"),
        ("OF", "Outfield"),
        ("DH", "Designated Hitter")
    };

    public static readonly IReadOnlyList<(string Value, string Label)> SortOptions = new[]
    {
        ("name", "Name"),
        ("hr", "Home Runs"),
        ("hits", "Hits"),
        ("games", "Games")
    };

    // Decades from the first professional season through today
    public static IEnumerable<int> EraOptions =>
        Enumerable.Range(0, (DateTime.UtcNow.Year - 1870) / 10 + 1).Select(i => 1870 + i * 10);

    /// <summary>
    /// Non-default filter values as query params, used to keep filters sticky
    /// across pagination and alphabet navigation.
    /// </summary>
    public Dictionary<string, string> FilterQueryParams(bool includeSearch = true)
    {
        var p = new Dictionary<string, string>();
        if (includeSearch && !string.IsNullOrWhiteSpace(SearchQuery)) p["q"] = SearchQuery;
        if (!string.IsNullOrEmpty(Position)) p["pos"] = Position;
        if (Era.HasValue) p["era"] = Era.Value.ToString();
        if (SortBy != "name") p["sort"] = SortBy;
        return p;
    }
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

    public static PlayerSummary FromPeople(People person, bool isHof = false, int? games = null, int? hits = null,
        int? hrs = null)
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