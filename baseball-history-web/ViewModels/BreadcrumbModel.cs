namespace baseball_history_web.ViewModels;

/// <summary>
/// Breadcrumb trail for drill-down pages. The last item is rendered as the
/// current page (no link).
/// </summary>
public class BreadcrumbModel
{
    public List<BreadcrumbItem> Items { get; set; } = new();

    public static BreadcrumbModel Build(params BreadcrumbItem[] items) =>
        new() { Items = items.ToList() };
}

public record BreadcrumbItem(string Label, string? Url = null);
