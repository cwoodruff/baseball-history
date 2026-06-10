namespace baseball_history_mcp.Querying;

internal static class McpInputValidator
{
    private static readonly HashSet<string> BattingStats =
    [
        "hr", "h", "r", "rbi", "sb", "2b", "3b", "bb", "g", "ab", "avg", "obp", "slg", "ops"
    ];

    private static readonly HashSet<string> PitchingStats =
    [
        "w", "l", "so", "sv", "cg", "sho", "ip", "g", "gs", "hr", "k9", "wpct", "era", "whip", "bb9"
    ];

    public static string NormalizePlayerId(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new BaseballMcpUsageException("playerId is required.");
        }

        return playerId.Trim().ToLowerInvariant();
    }

    public static string NormalizeFranchiseId(string franchiseId)
    {
        if (string.IsNullOrWhiteSpace(franchiseId))
        {
            throw new BaseballMcpUsageException("franchiseId is required.");
        }

        return franchiseId.Trim().ToUpperInvariant();
    }

    public static string NormalizeTeamId(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId))
        {
            throw new BaseballMcpUsageException("teamId is required.");
        }

        return teamId.Trim().ToUpperInvariant();
    }

    public static (string TeamId, string League, int Year) NormalizeTeamSeason(string teamId, string league, int year)
    {
        if (string.IsNullOrWhiteSpace(teamId) || string.IsNullOrWhiteSpace(league))
        {
            throw new BaseballMcpUsageException("teamId and league are required.");
        }

        if (year is < 1871 or > 2100)
        {
            throw new BaseballMcpUsageException("year must be within the supported Lahman range.");
        }

        return (teamId.Trim().ToUpperInvariant(), league.Trim().ToUpperInvariant(), year);
    }

    public static string NormalizeBattingStat(string stat)
    {
        var normalized = NormalizeStat(stat);
        if (!BattingStats.Contains(normalized))
        {
            throw new BaseballMcpUsageException($"Unsupported batting stat '{stat}'.");
        }

        return normalized;
    }

    public static string NormalizePitchingStat(string stat)
    {
        var normalized = NormalizeStat(stat);
        if (!PitchingStats.Contains(normalized))
        {
            throw new BaseballMcpUsageException($"Unsupported pitching stat '{stat}'.");
        }

        return normalized;
    }

    public static void ValidateYearRange(int? fromYear, int? toYear)
    {
        if (fromYear.HasValue && fromYear.Value is < 1871 or > 2100)
        {
            throw new BaseballMcpUsageException("fromYear must be within the supported Lahman range.");
        }

        if (toYear.HasValue && toYear.Value is < 1871 or > 2100)
        {
            throw new BaseballMcpUsageException("toYear must be within the supported Lahman range.");
        }

        if (fromYear.HasValue && toYear.HasValue && fromYear > toYear)
        {
            throw new BaseballMcpUsageException("fromYear must be less than or equal to toYear.");
        }
    }

    public static string? NormalizeLeague(string? league) =>
        string.IsNullOrWhiteSpace(league) ? null : league.Trim().ToUpperInvariant();

    private static string NormalizeStat(string stat)
    {
        if (string.IsNullOrWhiteSpace(stat))
        {
            throw new BaseballMcpUsageException("stat is required.");
        }

        return stat.Trim().ToLowerInvariant();
    }
}
