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

    public static EmptyStateModel NoManagers() => new()
    {
        Title = "No Managers Found",
        Message = "Try adjusting your search criteria.",
        Icon = "&#9918;"
    };

    public static EmptyStateModel NoParks() => new()
    {
        Title = "No Ballparks Found",
        Message = "Try adjusting your search criteria.",
        Icon = "&#127942;"
    };

    public static EmptyStateModel NoParkSeasons() => new()
    {
        Title = "No Game History",
        Message = "No home-game records are available for this ballpark.",
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