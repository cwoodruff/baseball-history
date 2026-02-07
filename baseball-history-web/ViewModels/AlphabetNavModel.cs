namespace baseball_history_web.ViewModels;

/// <summary>
/// Model for the alphabet navigation component
/// </summary>
public class AlphabetNavModel
{
    public char? CurrentLetter { get; set; }
    public List<char> AvailableLetters { get; set; } = new();
    public string BaseUrl { get; set; } = null!;
    public string Target { get; set; } = "#content";
    public Dictionary<string, string> QueryParams { get; set; } = new();

    /// <summary>
    /// Gets the URL for a specific letter
    /// </summary>
    public string GetLetterUrl(string letter)
    {
        var queryParams = new Dictionary<string, string>(QueryParams)
        {
            ["letter"] = letter,
            ["page"] = "1" // Reset to page 1 when changing letter
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{BaseUrl}?{queryString}";
    }

    public static AlphabetNavModel ForPlayers(char? currentLetter, IEnumerable<char> availableLetters)
    {
        return new AlphabetNavModel
        {
            CurrentLetter = currentLetter,
            AvailableLetters = availableLetters.ToList(),
            BaseUrl = "/Players",
            Target = "#player-list"
        };
    }
}