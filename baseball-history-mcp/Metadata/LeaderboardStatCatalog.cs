namespace baseball_history_mcp.Metadata;

public static class LeaderboardStatCatalog
{
    private static readonly IReadOnlyList<SupportedStatDefinition> BattingStats =
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
        new("avg", "Batting average", ["battingaverage"], "descending", "Ranks by hits divided by at-bats."),
        new("obp", "On-base percentage", [], "descending", "Ranks by on-base percentage using hits plus walks."),
        new("slg", "Slugging percentage", [], "descending", "Ranks by slugging percentage."),
        new("ops", "OPS", [], "descending", "Ranks by on-base percentage plus slugging percentage.")
    ];

    private static readonly IReadOnlyList<SupportedStatDefinition> PitchingStats =
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
        new("k9", "Strikeouts per nine", [], "descending", "Ranks by strikeouts per nine innings."),
        new("wpct", "Winning percentage", [], "descending", "Ranks by winning percentage."),
        new("era", "Earned run average", [], "ascending", "Ranks by earned run average; lower is better."),
        new("whip", "WHIP", [], "ascending", "Ranks by walks plus hits per inning pitched; lower is better."),
        new("bb9", "Walks per nine", [], "ascending", "Ranks by walks allowed per nine innings; lower is better.")
    ];

    private static readonly SupportedStatDefinition DefaultBattingStat = BattingStats[0];
    private static readonly SupportedStatDefinition DefaultPitchingStat = PitchingStats[0];

    public static IReadOnlyList<SupportedStatCategory> Categories { get; } =
    [
        new("batting", true, true, BattingStats),
        new("pitching", true, true, PitchingStats)
    ];

    public static SupportedStatDefinition ResolveBatting(string? stat) =>
        Resolve(stat, BattingStats, DefaultBattingStat, "batting");

    public static SupportedStatDefinition ResolvePitching(string? stat) =>
        Resolve(stat, PitchingStats, DefaultPitchingStat, "pitching");

    private static SupportedStatDefinition Resolve(
        string? stat,
        IReadOnlyList<SupportedStatDefinition> definitions,
        SupportedStatDefinition fallback,
        string category)
    {
        if (string.IsNullOrWhiteSpace(stat))
        {
            return fallback;
        }

        var normalized = stat.Trim().ToLowerInvariant();
        var match = definitions.FirstOrDefault(definition =>
            definition.Key == normalized || definition.Aliases.Contains(normalized, StringComparer.OrdinalIgnoreCase));

        return match ?? throw new BaseballMcpUsageException(
            $"Unsupported {category} stat '{stat}'. Use one of: {string.Join(", ", definitions.Select(definition => definition.Key))}.");
    }
}
