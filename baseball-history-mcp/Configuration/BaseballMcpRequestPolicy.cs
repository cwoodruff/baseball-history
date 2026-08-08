using baseball_history_mcp.Querying;
using BaseballHistory.Data.Querying;
using Microsoft.Extensions.Options;

namespace baseball_history_mcp.Configuration;

public sealed class BaseballMcpRequestPolicy(IOptions<BaseballMcpOptions> options)
{
    public PlayerLookupRequest Normalize(PlayerLookupRequest request) =>
        request with
        {
            Query = NormalizeOptionalText(request.Query),
            LastNameStartsWith = NormalizePrefix(request.LastNameStartsWith)
        };

    public FranchiseLookupRequest Normalize(FranchiseLookupRequest request) =>
        request with
        {
            League = NormalizeLeague(request.League)
        };

    public BattingLeaderboardQuery Normalize(BattingLeaderboardQuery request)
    {
        ValidateYearRange(request.FromYear, request.ToYear);

        if (request.MinAtBats < 0)
        {
            throw new BaseballMcpUsageException("minAtBats must be zero or greater.");
        }

        var statDef = LeaderboardStatCatalog.GetBattingStat(request.Stat);
        if (statDef == null)
        {
            throw new BaseballMcpUsageException($"Unsupported batting stat '{request.Stat}'.");
        }

        return request with
        {
            Stat = statDef.Key,
            League = NormalizeLeague(request.League),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, options.Value.Limits.LeaderboardPageSizeMax)
        };
    }

    public PitchingLeaderboardQuery Normalize(PitchingLeaderboardQuery request)
    {
        ValidateYearRange(request.FromYear, request.ToYear);

        if (request.MinInningsPitched < 0)
        {
            throw new BaseballMcpUsageException("minInningsPitched must be zero or greater.");
        }

        var statDef = LeaderboardStatCatalog.GetPitchingStat(request.Stat);
        if (statDef == null)
        {
            throw new BaseballMcpUsageException($"Unsupported pitching stat '{request.Stat}'.");
        }

        return request with
        {
            Stat = statDef.Key,
            League = NormalizeLeague(request.League),
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, options.Value.Limits.LeaderboardPageSizeMax)
        };
    }

    public HallOfFameLookupRequest Normalize(HallOfFameLookupRequest request) =>
        request with
        {
            Category = NormalizeOptionalText(request.Category)
        };

    public string NormalizeRequiredId(string value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new BaseballMcpUsageException($"{parameterName} is required.");
        }

        return normalized;
    }

    public PageWindow CreatePlayerLookupWindow(PlayerLookupRequest request, int totalCount) =>
        PageWindow.Create(request.Page, request.PageSize, options.Value.Limits.PlayerSearchPageSizeMax, totalCount);

    public PageWindow CreateFranchiseLookupWindow(FranchiseLookupRequest request, int totalCount) =>
        PageWindow.Create(request.Page, request.PageSize, options.Value.Limits.FranchiseListPageSizeMax, totalCount);

    public PageWindow CreateHallOfFameLookupWindow(HallOfFameLookupRequest request, int totalCount) =>
        PageWindow.Create(request.Page, request.PageSize, options.Value.Limits.HallOfFamePageSizeMax, totalCount);

    public PageWindow CreateLeaderboardWindow(int requestedPage, int requestedPageSize, int totalCount) =>
        PageWindow.Create(requestedPage, requestedPageSize, options.Value.Limits.LeaderboardPageSizeMax, totalCount);

    private static void ValidateYearRange(int? fromYear, int? toYear)
    {
        if (fromYear.HasValue && toYear.HasValue && fromYear.Value > toYear.Value)
        {
            throw new BaseballMcpUsageException("fromYear must be less than or equal to toYear.");
        }
    }

    private static string? NormalizeLeague(string? league) =>
        NormalizeOptionalText(league)?.ToUpperInvariant();

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizePrefix(string? value)
    {
        var normalized = NormalizeOptionalText(value);
        return normalized is null ? null : normalized[..1];
    }
}

public sealed record PageWindow(
    int RequestedPage,
    int RequestedPageSize,
    int Page,
    int PageSize,
    int TotalPages,
    int MaxPageSize)
{
    public bool WasPageAdjusted => Page != RequestedPage;

    public bool WasPageSizeClamped => PageSize != RequestedPageSize;

    public PagedReadResult<T> CreateResult<T>(IReadOnlyList<T> items, int totalCount) =>
        new(items, Page, PageSize, totalCount, TotalPages)
        {
            RequestedPage = RequestedPage,
            RequestedPageSize = RequestedPageSize,
            MaxPageSize = MaxPageSize,
            WasPageAdjusted = WasPageAdjusted,
            WasPageSizeClamped = WasPageSizeClamped
        };

    public static PageWindow Create(int requestedPage, int requestedPageSize, int maxPageSize, int totalCount)
    {
        var pageSize = Math.Clamp(requestedPageSize, 1, maxPageSize);
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize));
        var page = Math.Clamp(requestedPage, 1, totalPages);
        return new PageWindow(requestedPage, requestedPageSize, page, pageSize, totalPages, maxPageSize);
    }
}
