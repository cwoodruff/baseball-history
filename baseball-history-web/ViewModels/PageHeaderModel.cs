namespace baseball_history_web.ViewModels;

public class PageHeaderModel
{
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? Eyebrow { get; set; }
    public string? BadgeText { get; set; }
    public string BadgeVariant { get; set; } = "neutral";
}
