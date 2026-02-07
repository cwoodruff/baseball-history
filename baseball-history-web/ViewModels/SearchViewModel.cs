namespace baseball_history_web.ViewModels;

/// <summary>
/// ViewModel for search results
/// </summary>
public class SearchViewModel
{
    public string Query { get; set; } = null!;
    public List<SearchResult> Players { get; set; } = new();
    public List<SearchResult> Teams { get; set; } = new();
    public int TotalResults => Players.Count + Teams.Count;
    public bool HasResults => TotalResults > 0;
}

/// <summary>
/// Individual search result
/// </summary>
public class SearchResult
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public SearchResultType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string Initials { get; set; } = null!;
    public string? TeamId { get; set; } // For team colors
    public bool IsInHallOfFame { get; set; }

    public string Url => Type switch
    {
        SearchResultType.Player => $"/Players/Details/{Id}",
        SearchResultType.Team => $"/Teams/Season/{Id}",
        SearchResultType.Franchise => $"/Teams/Franchise/{Id}",
        _ => "#"
    };

    public string TypeLabel => Type switch
    {
        SearchResultType.Player => "Player",
        SearchResultType.Team => "Team",
        SearchResultType.Franchise => "Franchise",
        _ => "Unknown"
    };
}

public enum SearchResultType
{
    Player,
    Team,
    Franchise
}
