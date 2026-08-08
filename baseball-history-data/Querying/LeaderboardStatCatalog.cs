namespace BaseballHistory.Data.Querying;

public sealed record LeaderboardStatDefinition(
    string Key,
    string Label,
    IReadOnlyList<string> Aliases,
    string SortDirection,
    bool IsRateStat);

public static class LeaderboardStatCatalog
{
    private static readonly IReadOnlyList<LeaderboardStatDefinition> BattingDefinitions =
    [
        new("hr", "Home runs", ["homeruns"], "descending", false),
        new("h", "Hits", ["hits"], "descending", false),
        new("r", "Runs", ["runs"], "descending", false),
        new("rbi", "Runs batted in", [], "descending", false),
        new("sb", "Stolen bases", ["stolenbases"], "descending", false),
        new("2b", "Doubles", ["doubles"], "descending", false),
        new("3b", "Triples", ["triples"], "descending", false),
        new("bb", "Walks", ["walks"], "descending", false),
        new("g", "Games", ["games"], "descending", false),
        new("ab", "At-bats", ["atbats"], "descending", false),
        new("avg", "Batting Average", ["battingaverage"], "descending", true),
        new("obp", "On-base percentage", [], "descending", true),
        new("slg", "Slugging percentage", [], "descending", true),
        new("ops", "OPS", [], "descending", true)
    ];

    private static readonly IReadOnlyList<LeaderboardStatDefinition> PitchingDefinitions =
    [
        new("w", "Wins", ["wins"], "descending", false),
        new("l", "Losses", ["losses"], "descending", false),
        new("so", "Strikeouts", ["strikeouts"], "descending", false),
        new("bb", "Walks", ["walks"], "descending", false),
        new("sv", "Saves", ["saves"], "descending", false),
        new("cg", "Complete games", [], "descending", false),
        new("sho", "Shutouts", [], "descending", false),
        new("ip", "Innings pitched", [], "descending", false),
        new("g", "Games", ["games"], "descending", false),
        new("gs", "Games started", [], "descending", false),
        new("hr", "Home runs allowed", [], "descending", false),
        new("k9", "Strikeouts per nine", [], "descending", true),
        new("wpct", "Winning percentage", [], "descending", true),
        new("era", "ERA", [], "ascending", true),
        new("whip", "WHIP", [], "ascending", true),
        new("bb9", "Walks per nine", [], "ascending", true)
    ];

    private static readonly Dictionary<string, LeaderboardStatDefinition> BattingLookup = CreateLookup(BattingDefinitions);
    private static readonly Dictionary<string, LeaderboardStatDefinition> PitchingLookup = CreateLookup(PitchingDefinitions);

    public static IReadOnlyList<LeaderboardStatDefinition> GetBattingStats() => BattingDefinitions;
    public static IReadOnlyList<LeaderboardStatDefinition> GetPitchingStats() => PitchingDefinitions;

    public static LeaderboardStatDefinition? GetBattingStat(string stat)
    {
        var normalized = stat.ToLowerInvariant();
        return BattingLookup.GetValueOrDefault(normalized);
    }

    public static LeaderboardStatDefinition? GetPitchingStat(string stat)
    {
        var normalized = stat.ToLowerInvariant();
        return PitchingLookup.GetValueOrDefault(normalized);
    }

    private static Dictionary<string, LeaderboardStatDefinition> CreateLookup(IEnumerable<LeaderboardStatDefinition> definitions)
    {
        var lookup = new Dictionary<string, LeaderboardStatDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            lookup[definition.Key] = definition;
            foreach (var alias in definition.Aliases)
            {
                lookup[alias] = definition;
            }
        }

        return lookup;
    }
}
