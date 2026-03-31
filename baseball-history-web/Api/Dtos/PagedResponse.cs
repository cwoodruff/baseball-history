namespace baseball_history_web.Api.Dtos;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public static class PagedResponse
{
    public static PagedResponse<T> Create<T>(IReadOnlyList<T> data, int page, int pageSize, int totalCount) =>
        new(data, page, pageSize, totalCount, (int)Math.Ceiling((double)totalCount / Math.Max(1, pageSize)));
}
