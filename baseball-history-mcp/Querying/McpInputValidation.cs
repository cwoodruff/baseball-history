using ModelContextProtocol;

namespace baseball_history_mcp.Querying;

internal static class McpInputValidation
{
    public static string NormalizeRequiredPlayerId(string? value, string parameterName = "playerId")
    {
        var normalized = NormalizeRequiredText(value, parameterName);
        return normalized.ToLowerInvariant();
    }

    public static string NormalizeRequiredCode(string? value, string parameterName)
    {
        var normalized = NormalizeRequiredText(value, parameterName);
        return normalized.ToUpperInvariant();
    }

    public static string? NormalizeOptionalText(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    public static string? NormalizeOptionalLeague(string? value, string parameterName = "league")
    {
        var normalized = NormalizeOptionalText(value);
        if (normalized is null)
        {
            return null;
        }

        if (normalized.Length is < 2 or > 5)
        {
            ThrowInvalid($"{parameterName} must be between 2 and 5 characters.");
        }

        return normalized.ToUpperInvariant();
    }

    public static void ValidatePage(int page, string parameterName = "page")
    {
        if (page < 1)
        {
            ThrowInvalid($"{parameterName} must be at least 1.");
        }
    }

    public static void ValidatePageSize(int pageSize, string parameterName = "pageSize")
    {
        if (pageSize < 1)
        {
            ThrowInvalid($"{parameterName} must be at least 1.");
        }
    }

    public static void ValidateNonNegative(int value, string parameterName)
    {
        if (value < 0)
        {
            ThrowInvalid($"{parameterName} must be zero or greater.");
        }
    }

    public static short ValidateYear(int year, string parameterName = "year")
    {
        if (year is < 1 or > short.MaxValue)
        {
            ThrowInvalid($"{parameterName} must be between 1 and {short.MaxValue}.");
        }

        return (short)year;
    }

    public static void ValidateOptionalYear(int? year, string parameterName)
    {
        if (year.HasValue)
        {
            ValidateYear(year.Value, parameterName);
        }
    }

    public static void ValidateYearRange(int? fromYear, int? toYear)
    {
        ValidateOptionalYear(fromYear, "fromYear");
        ValidateOptionalYear(toYear, "toYear");

        if (fromYear.HasValue && toYear.HasValue && fromYear.Value > toYear.Value)
        {
            ThrowInvalid("fromYear must be less than or equal to toYear.");
        }
    }

    public static void ThrowInvalid(string message) =>
        throw new McpProtocolException($"Invalid parameters: {message}", McpErrorCode.InvalidParams);

    private static string NormalizeRequiredText(string? value, string parameterName)
    {
        var normalized = NormalizeOptionalText(value);
        if (normalized is null)
        {
            ThrowInvalid($"{parameterName} is required.");
        }

        return normalized!;
    }
}
