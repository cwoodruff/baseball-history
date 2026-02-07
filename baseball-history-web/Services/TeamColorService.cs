namespace baseball_history_web.Services;

/// <summary>
/// Service for mapping team/franchise IDs to their color schemes
/// </summary>
public class TeamColorService
{
    private static readonly Dictionary<string, TeamColors> TeamColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // American League East
        ["BAL"] = new("#DF4601", "#000000", "#FFFFFF", "Baltimore Orioles"),
        ["BOS"] = new("#BD3039", "#0C2340", "#FFFFFF", "Boston Red Sox"),
        ["NYA"] = new("#003087", "#E4002B", "#FFFFFF", "New York Yankees"),
        ["NYY"] = new("#003087", "#E4002B", "#FFFFFF", "New York Yankees"),
        ["TBA"] = new("#092C5C", "#8FBCE6", "#FFFFFF", "Tampa Bay Rays"),
        ["TBD"] = new("#092C5C", "#8FBCE6", "#FFFFFF", "Tampa Bay Rays"),
        ["TBR"] = new("#092C5C", "#8FBCE6", "#FFFFFF", "Tampa Bay Rays"),
        ["TOR"] = new("#134A8E", "#1D2D5C", "#FFFFFF", "Toronto Blue Jays"),

        // American League Central
        ["CHA"] = new("#27251F", "#C4CED4", "#FFFFFF", "Chicago White Sox"),
        ["CHW"] = new("#27251F", "#C4CED4", "#FFFFFF", "Chicago White Sox"),
        ["CLE"] = new("#00385D", "#E50022", "#FFFFFF", "Cleveland Guardians"),
        ["DET"] = new("#0C2340", "#FA4616", "#FFFFFF", "Detroit Tigers"),
        ["KCA"] = new("#004687", "#BD9B60", "#FFFFFF", "Kansas City Royals"),
        ["KCR"] = new("#004687", "#BD9B60", "#FFFFFF", "Kansas City Royals"),
        ["MIN"] = new("#002B5C", "#D31145", "#FFFFFF", "Minnesota Twins"),

        // American League West
        ["HOU"] = new("#002D62", "#EB6E1F", "#FFFFFF", "Houston Astros"),
        ["ANA"] = new("#BA0021", "#003263", "#FFFFFF", "Los Angeles Angels"),
        ["CAL"] = new("#BA0021", "#003263", "#FFFFFF", "Los Angeles Angels"),
        ["LAA"] = new("#BA0021", "#003263", "#FFFFFF", "Los Angeles Angels"),
        ["OAK"] = new("#003831", "#EFB21E", "#FFFFFF", "Oakland Athletics"),
        ["SEA"] = new("#0C2C56", "#005C5C", "#FFFFFF", "Seattle Mariners"),
        ["TEX"] = new("#003278", "#C0111F", "#FFFFFF", "Texas Rangers"),

        // National League East
        ["ATL"] = new("#CE1141", "#13274F", "#FFFFFF", "Atlanta Braves"),
        ["FLO"] = new("#00A3E0", "#EF3340", "#000000", "Miami Marlins"),
        ["FLA"] = new("#00A3E0", "#EF3340", "#000000", "Miami Marlins"),
        ["MIA"] = new("#00A3E0", "#EF3340", "#000000", "Miami Marlins"),
        ["NYN"] = new("#002D72", "#FF5910", "#FFFFFF", "New York Mets"),
        ["NYM"] = new("#002D72", "#FF5910", "#FFFFFF", "New York Mets"),
        ["PHI"] = new("#E81828", "#002D72", "#FFFFFF", "Philadelphia Phillies"),
        ["MON"] = new("#003087", "#E4002B", "#FFFFFF", "Montreal Expos"),
        ["WAS"] = new("#AB0003", "#14225A", "#FFFFFF", "Washington Nationals"),
        ["WSN"] = new("#AB0003", "#14225A", "#FFFFFF", "Washington Nationals"),

        // National League Central
        ["CHN"] = new("#0E3386", "#CC3433", "#FFFFFF", "Chicago Cubs"),
        ["CHC"] = new("#0E3386", "#CC3433", "#FFFFFF", "Chicago Cubs"),
        ["CIN"] = new("#C6011F", "#000000", "#FFFFFF", "Cincinnati Reds"),
        ["MIL"] = new("#12284B", "#B6922E", "#FFFFFF", "Milwaukee Brewers"),
        ["PIT"] = new("#27251F", "#FDB827", "#FFFFFF", "Pittsburgh Pirates"),
        ["SLN"] = new("#C41E3A", "#0C2340", "#FFFFFF", "St. Louis Cardinals"),
        ["STL"] = new("#C41E3A", "#0C2340", "#FFFFFF", "St. Louis Cardinals"),

        // National League West
        ["ARI"] = new("#A71930", "#E3D4AD", "#000000", "Arizona Diamondbacks"),
        ["COL"] = new("#33006F", "#C4CED4", "#FFFFFF", "Colorado Rockies"),
        ["LAN"] = new("#005A9C", "#EF3E42", "#FFFFFF", "Los Angeles Dodgers"),
        ["LAD"] = new("#005A9C", "#EF3E42", "#FFFFFF", "Los Angeles Dodgers"),
        ["SDN"] = new("#2F241D", "#FFC425", "#FFFFFF", "San Diego Padres"),
        ["SDP"] = new("#2F241D", "#FFC425", "#FFFFFF", "San Diego Padres"),
        ["SFN"] = new("#FD5A1E", "#27251F", "#FFFFFF", "San Francisco Giants"),
        ["SFG"] = new("#FD5A1E", "#27251F", "#FFFFFF", "San Francisco Giants"),

        // Historical Teams
        ["BRO"] = new("#005A9C", "#FFFFFF", "#FFFFFF", "Brooklyn Dodgers"),
        ["NY1"] = new("#FD5A1E", "#000000", "#FFFFFF", "New York Giants"),
        ["NYG"] = new("#FD5A1E", "#000000", "#FFFFFF", "New York Giants"),
        ["BSN"] = new("#CE1141", "#13274F", "#FFFFFF", "Boston Braves"),
        ["SLA"] = new("#BA5624", "#27251F", "#FFFFFF", "St. Louis Browns"),
        ["PHA"] = new("#003831", "#EFB21E", "#FFFFFF", "Philadelphia Athletics"),
        ["WS1"] = new("#AB0003", "#14225A", "#FFFFFF", "Washington Senators"),
        ["WS2"] = new("#AB0003", "#14225A", "#FFFFFF", "Washington Senators"),
        ["WSA"] = new("#AB0003", "#14225A", "#FFFFFF", "Washington Senators"),
        ["SE1"] = new("#005C5C", "#FDB827", "#FFFFFF", "Seattle Pilots"),
    };

    // Default colors for unknown teams
    private static readonly TeamColors DefaultColors = new("#002D72", "#D50032", "#FFFFFF", "Unknown Team");

    /// <summary>
    /// Gets the team colors for a given team ID
    /// </summary>
    public TeamColors GetTeamColors(string? teamId)
    {
        if (string.IsNullOrEmpty(teamId))
            return DefaultColors;

        return TeamColorMap.TryGetValue(teamId, out var colors) ? colors : DefaultColors;
    }

    /// <summary>
    /// Gets the CSS data-team attribute value for a given team ID
    /// </summary>
    public string GetDataTeamAttribute(string? teamId)
    {
        return string.IsNullOrEmpty(teamId) ? "" : $"data-team=\"{teamId}\"";
    }

    /// <summary>
    /// Gets the CSS data-franchise attribute value for a given franchise ID
    /// </summary>
    public string GetDataFranchiseAttribute(string? franchiseId)
    {
        return string.IsNullOrEmpty(franchiseId) ? "" : $"data-franchise=\"{franchiseId}\"";
    }

    /// <summary>
    /// Checks if a team ID has a known color scheme
    /// </summary>
    public bool HasTeamColors(string? teamId)
    {
        return !string.IsNullOrEmpty(teamId) && TeamColorMap.ContainsKey(teamId);
    }

    /// <summary>
    /// Gets all known team IDs
    /// </summary>
    public IEnumerable<string> GetAllTeamIds()
    {
        return TeamColorMap.Keys.Distinct();
    }
}

/// <summary>
/// Represents a team's color scheme
/// </summary>
public record TeamColors(
    string Primary,
    string Secondary,
    string Accent,
    string TeamName
)
{
    /// <summary>
    /// Gets CSS inline style for team primary color background
    /// </summary>
    public string PrimaryBackgroundStyle => $"background-color: {Primary}; color: {Accent};";

    /// <summary>
    /// Gets CSS inline style for team secondary color background
    /// </summary>
    public string SecondaryBackgroundStyle => $"background-color: {Secondary}; color: {Accent};";

    /// <summary>
    /// Gets CSS inline style for gradient background
    /// </summary>
    public string GradientStyle => $"background: linear-gradient(135deg, {Primary} 0%, {Secondary} 100%); color: {Accent};";
}
