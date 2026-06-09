namespace baseball_history_mcp.Configuration;

public sealed class BaseballMcpOptions
{
    public const string SectionName = "BaseballMcp";

    public int QueryTimeoutSeconds { get; init; } = 30;

    public BaseballMcpLimitOptions Limits { get; init; } = new();
}

public sealed class BaseballMcpLimitOptions
{
    public int PlayerSearchPageSizeMax { get; init; } = 100;

    public int FranchiseListPageSizeMax { get; init; } = 50;

    public int LeaderboardPageSizeMax { get; init; } = 100;

    public int HallOfFamePageSizeMax { get; init; } = 50;

    public int HallOfFameVotingHistoryYearsMax { get; init; } = 25;

    public int SalaryHistorySeasonsMax { get; init; } = 40;

    public int SalaryLeaderboardPageSizeMax { get; init; } = 50;
}

internal static class BaseballMcpOptionsValidator
{
    public static BaseballMcpOptions Validate(BaseballMcpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.QueryTimeoutSeconds <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:QueryTimeoutSeconds must be greater than zero.");
        }

        if (options.Limits.PlayerSearchPageSizeMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:PlayerSearchPageSizeMax must be greater than zero.");
        }

        if (options.Limits.LeaderboardPageSizeMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:LeaderboardPageSizeMax must be greater than zero.");
        }

        if (options.Limits.FranchiseListPageSizeMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:FranchiseListPageSizeMax must be greater than zero.");
        }

        if (options.Limits.HallOfFamePageSizeMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:HallOfFamePageSizeMax must be greater than zero.");
        }

        if (options.Limits.HallOfFameVotingHistoryYearsMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:HallOfFameVotingHistoryYearsMax must be greater than zero.");
        }

        if (options.Limits.SalaryHistorySeasonsMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:SalaryHistorySeasonsMax must be greater than zero.");
        }

        if (options.Limits.SalaryLeaderboardPageSizeMax <= 0)
        {
            throw new InvalidOperationException($"{BaseballMcpOptions.SectionName}:Limits:SalaryLeaderboardPageSizeMax must be greater than zero.");
        }

        return options;
    }
}
