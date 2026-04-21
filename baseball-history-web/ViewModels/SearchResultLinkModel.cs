namespace baseball_history_web.ViewModels;

public class SearchResultLinkModel
{
    public SearchResult Result { get; set; } = null!;
    public string ItemClass { get; set; } = "search-result-item text-decoration-none";
    public string IconClass { get; set; } = "result-icon";
    public bool ClearSearchResultsOnClick { get; set; }
    public bool DismissModalOnClick { get; set; }
    public bool UsePlayerModal { get; set; } = true;
}
