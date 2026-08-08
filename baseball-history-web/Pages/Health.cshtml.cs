using BaseballHistory.Data.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages;

public class HealthModel(BaseballDbContext context) : PageModel
{
    public bool DatabaseHealthy { get; set; }
    public string? ErrorMessage { get; set; }
    public BaseballHistory.Data.Models.Teams? SampleTeam { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            SampleTeam = await context.Teams.FirstOrDefaultAsync();
            DatabaseHealthy = SampleTeam != null;
        }
        catch (Exception ex)
        {
            DatabaseHealthy = false;
            ErrorMessage = ex.Message;
        }
    }
}