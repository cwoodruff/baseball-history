using baseball_history_web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace baseball_history_web.Pages.Shared;

public abstract class HtmxPageModel : PageModel
{
    protected IActionResult HtmxOrPage(string partialName, object model)
    {
        return Request.IsHtmxNonBoostedRequest()
            ? Partial(partialName, model)
            : Page();
    }
}
