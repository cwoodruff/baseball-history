namespace baseball_history_web.ViewModels;

/// <summary>
/// Model for the empty state component
/// </summary>
public class EmptyStateModel
{
    public string Title { get; set; } = "No Results Found";
    public string? Message { get; set; }
    public string? Icon { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }

    public static EmptyStateModel NoPlayers(string? letter = null) => new()
    {
        Title = "No Players Found",
        Message = letter != null
            ? $"No players found with last names starting with '{letter}'."
            : "Try adjusting your search criteria.",
        Icon = "&#9918;"
    };

    public static EmptyStateModel NoTeams() => new()
    {
        Title = "No Teams Found",
        Message = "Try adjusting your search criteria.",
        Icon = "&#9918;"
    };

    public static EmptyStateModel NoSearchResults(string query) => new()
    {
        Title = "No Results",
        Message = $"No results found for \"{query}\".",
        Icon = "&#128269;"
    };

    public static EmptyStateModel NoStats() => new()
    {
        Title = "No Statistics Available",
        Message = "No statistics match the selected criteria.",
        Icon = "&#128202;"
    };
}
