namespace baseball_history_mcp.Querying;

public sealed record PagedReadResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public int RequestedPage { get; init; } = Page;

    public int RequestedPageSize { get; init; } = PageSize;

    public int MaxPageSize { get; init; } = PageSize;

    public bool WasPageAdjusted { get; init; }

    public bool WasPageSizeClamped { get; init; }
}
