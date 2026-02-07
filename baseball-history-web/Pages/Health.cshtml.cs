using baseball_history_web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_web.Pages;

public class HealthModel : PageModel
{
    private readonly BaseballDbContext _context;

    public HealthModel(BaseballDbContext context)
    {
        _context = context;
    }

    public bool DatabaseHealthy { get; set; }
    public string? ErrorMessage { get; set; }
    public Models.Teams? SampleTeam { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            SampleTeam = await _context.Teams.FirstOrDefaultAsync();
            DatabaseHealthy = SampleTeam != null;
        }
        catch (Exception ex)
        {
            DatabaseHealthy = false;
            ErrorMessage = ex.Message;
        }
    }
}
