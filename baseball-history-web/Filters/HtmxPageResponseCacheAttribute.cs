using Microsoft.AspNetCore.Mvc;

namespace baseball_history_web.Filters;

public sealed class HtmxPageResponseCacheAttribute : ResponseCacheAttribute
{
    public HtmxPageResponseCacheAttribute()
    {
        Duration = 3600;
        Location = ResponseCacheLocation.Client;
        VaryByHeader = "HX-Request";
    }
}
