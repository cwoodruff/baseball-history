using baseball_history_web.Services;
using baseball_history_web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace baseball_history_web.Pages.Players;

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class DetailsModel(PlayerDetailService playerDetailService) : PageModel
{
    public PlayerDetailViewModel Player { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var player = await playerDetailService.GetPlayerDetailAsync(id);

        if (player == null)
        {
            return NotFound();
        }

        Player = player;
        return Page();
    }
}
