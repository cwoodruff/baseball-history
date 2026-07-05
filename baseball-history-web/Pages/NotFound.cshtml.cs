using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace baseball_history_web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class NotFoundModel : PageModel
{
    public int ErrorStatusCode { get; set; } = 404;

    public void OnGet()
    {
        // Reached via UseStatusCodePagesWithReExecute, which preserves the
        // original status code on the response.
        ErrorStatusCode = HttpContext.Response.StatusCode;
    }
}
