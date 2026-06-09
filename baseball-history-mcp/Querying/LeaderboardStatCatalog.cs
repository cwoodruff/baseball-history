using baseball_history_mcp.Metadata;

namespace baseball_history_mcp.Querying;

internal sealed record LeaderboardStatDefinition(
    string Key,
    string Label,
    IReadOnlyList<string> Aliases,
    string SortDirection,
    string Description,
    bool UsesPlayingTimeTieBreaker = false);

internal static class LeaderboardStatCatalog
{
    private static readonly IReadOnlyList<LeaderboardStatDefinition> BattingDefinitions =
    [
        new("hr", "Home runs", ["homeruns"], "descending", "Ranks by total home runs."),
        new("h", "Hits", ["hits"], "descending", "Ranks by total hits."),
        new("r", "Runs", ["runs"], "descending", "Ranks by total runs scored."),
        new("rbi", "Runs batted in", [], "descending", "Ranks by total RBI."),
        new("sb", "Stolen bases", ["stolenbases"], "descending", "Ranks by total stolen bases."),
        new("2b", "Doubles", ["doubles"], "descending", "Ranks by doubles."),
        new("3b", "Triples", ["triples"], "descending", "Ranks by triples."),
        new("bb", "Walks", ["walks"], "descending", "Ranks by bases on balls."),
        new("g", "Games", ["games"], "descending", "Ranks by games played."),
        new("ab", "At-bats", ["atbats"], "descending", "Ranks by official at-bats."),
        new("avg", "Batting average", ["battingaverage"], "descending", "Ranks by hits divided by at-bats.", true),
        new("obp", "On-base percentage", [], "descending", "Ranks by on-base percentage using hits plus walks.", true),
        new("slg", "Slugging percentage", [], "descending", "Ranks by slugging percentage.", true),
        new("ops", "OPS", [], "descending", "Ranks by on-base percentage plus slugging percentage.", true)
    ];

    private static readonly IReadOnlyList<LeaderboardStatDefinition> PitchingDefinitions =
    [
        new("w", "Wins", ["wins"], "descending", "Ranks by total wins."),
        new("l", "Losses", ["losses"], "descending", "Ranks by total losses."),
        new("so", "Strikeouts", ["strikeouts"], "descending", "Ranks by total strikeouts."),
        new("sv", "Saves", ["saves"], "descending", "Ranks by total saves."),
        new("cg", "Complete games", [], "descending", "Ranks by complete games."),
        new("sho", "Shutouts", [], "descending", "Ranks by shutouts."),
        new("ip", "Innings pitched", [], "descending", "Ranks by innings pitched workload."),
        new("g", "Games", ["games"], "descending", "Ranks by pitching appearances."),
        new("gs", "Games started", [], "descending", "Ranks by games started."),
        new("hr", "Home runs allowed", [], "descending", "Ranks by home runs allowed."),
        new("k9", "Strikeouts per nine", [], "descending", "Ranks by strikeouts per nine innings.", true),
        new("wpct", "Winning percentage", [], "descending", "Ranks by winning percentage.", true),
        new("era", "Earned run average", [], "ascending", "Ranks by earned run average; lower is better.", true),
        new("whip", "WHIP", [], "ascending", "Ranks by walks plus hits per inning pitched; lower is better.", true),
        new("bb9", "Walks per nine", [], "ascending", "Ranks by walks allowed per nine innings; lower is better.", true)
    ];

    private static readonly Dictionary<string, LeaderboardStatDefinition> BattingLookup = CreateLookup(BattingDefinitions);
    private static readonly Dictionary<string, LeaderboardStatDefinition> PitchingLookup = CreateLookup(PitchingDefinitions);

    public static IReadOnlyList<SupportedStatDefinition> GetBattingMetadata() => BattingDefinitions
        .Select(definition => new SupportedStatDefinition(
            definition.Key,
            definition.Label,
            definition.Aliases,
            definition.SortDirection,
            definition.Description))
        .ToList();

    public static IReadOnlyList<SupportedStatDefinition> GetPitchingMetadata() => PitchingDefinitions
        .Select(definition => new SupportedStatDefinition(
            definition.Key,
            definition.Label,
            definition.Aliases,
            definition.SortDirection,
            definition.Description))
        .ToList();

    public static LeaderboardStatDefinition NormalizeBattingStat(string? stat)
    {
        var normalized = McpInputValidation.NormalizeOptionalText(stat)?.ToLowerInvariant() ?? "hr";
        if (BattingLookup.TryGetValue(normalized, out var definition))
        {
            return definition;
        }

        McpInputValidation.ThrowInvalid(
            $"stat must be one of: {string.Join(", ", BattingDefinitions.Select(definition => definition.Key))}.");
        return null!;
    }

    public static LeaderboardStatDefinition NormalizePitchingStat(string? stat)
    {
        var normalized = McpInputValidation.NormalizeOptionalText(stat)?.ToLowerInvariant() ?? "w";
        if (PitchingLookup.TryGetValue(normalized, out var definition))
        {
            return definition;
        }

        McpInputValidation.ThrowInvalid(
            $"stat must be one of: {string.Join(", ", PitchingDefinitions.Select(definition => definition.Key))}.");
        return null!;
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
