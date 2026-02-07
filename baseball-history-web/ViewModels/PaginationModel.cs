namespace baseball_history_web.ViewModels;

/// <summary>
/// Model for the pagination component
/// </summary>
public class PaginationModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 50;
    public string BaseUrl { get; set; } = null!;
    public string Target { get; set; } = "#content";
    public bool PushUrl { get; set; } = true;
    public Dictionary<string, string> QueryParams { get; set; } = new();
    public int VisiblePageCount { get; set; } = 5;

    /// <summary>
    /// Gets the URL for a specific page
    /// </summary>
    public string GetPageUrl(int page)
    {
        var queryParams = new Dictionary<string, string>(QueryParams)
        {
            ["page"] = page.ToString()
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{BaseUrl}?{queryString}";
    }

    /// <summary>
    /// Gets the visible page numbers for the pagination control
    /// Returns -1 for ellipsis markers
    /// </summary>
    public IEnumerable<int> GetVisiblePages()
    {
        if (TotalPages <= VisiblePageCount + 2)
        {
            // Show all pages
            for (int i = 1; i <= TotalPages; i++)
                yield return i;
            yield break;
        }

        // Always show first page
        yield return 1;

        int half = VisiblePageCount / 2;
        int start = Math.Max(2, CurrentPage - half);
        int end = Math.Min(TotalPages - 1, CurrentPage + half);

        // Adjust if we're near the start or end
        if (CurrentPage <= half + 1)
        {
            end = VisiblePageCount;
        }
        else if (CurrentPage >= TotalPages - half)
        {
            start = TotalPages - VisiblePageCount + 1;
        }

        // Ellipsis before middle section
        if (start > 2)
            yield return -1;

        // Middle pages
        for (int i = start; i <= end; i++)
            yield return i;

        // Ellipsis after middle section
        if (end < TotalPages - 1)
            yield return -1;

        // Always show last page
        yield return TotalPages;
    }

    public static PaginationModel Create(int currentPage, int totalItems, int pageSize, string baseUrl, Dictionary<string, string>? queryParams = null)
    {
        return new PaginationModel
        {
            CurrentPage = currentPage,
            TotalItems = totalItems,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            BaseUrl = baseUrl,
            QueryParams = queryParams ?? new Dictionary<string, string>()
        };
    }
}
