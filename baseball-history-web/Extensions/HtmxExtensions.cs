namespace baseball_history_web.Extensions;

/// <summary>
/// Extension methods for detecting and handling HTMX requests
/// </summary>
public static class HtmxExtensions
{
    private const string HxRequestHeader = "HX-Request";
    private const string HxBoostedHeader = "HX-Boosted";
    private const string HxTargetHeader = "HX-Target";
    private const string HxTriggerHeader = "HX-Trigger";
    private const string HxTriggerNameHeader = "HX-Trigger-Name";
    private const string HxCurrentUrlHeader = "HX-Current-URL";
    private const string HxPromptHeader = "HX-Prompt";

    /// <summary>
    /// Checks if the current request is an HTMX request
    /// </summary>
    public static bool IsHtmxRequest(this HttpRequest request)
    {
        return request.Headers.ContainsKey(HxRequestHeader);
    }

    /// <summary>
    /// Checks if the current request is a boosted navigation (hx-boost)
    /// </summary>
    public static bool IsHtmxBoosted(this HttpRequest request)
    {
        return request.Headers.ContainsKey(HxBoostedHeader);
    }

    /// <summary>
    /// Checks if this is a non-boosted HTMX request (targeted partial request).
    /// Use this to determine if you should return a partial view instead of full page.
    /// Boosted navigations should return full pages, targeted requests should return partials.
    /// </summary>
    public static bool IsHtmxNonBoostedRequest(this HttpRequest request)
    {
        return request.IsHtmxRequest() && !request.IsHtmxBoosted();
    }

    /// <summary>
    /// Gets the target element ID for the HTMX request
    /// </summary>
    public static string? GetHtmxTarget(this HttpRequest request)
    {
        return request.Headers.TryGetValue(HxTargetHeader, out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// Gets the trigger element ID for the HTMX request
    /// </summary>
    public static string? GetHtmxTrigger(this HttpRequest request)
    {
        return request.Headers.TryGetValue(HxTriggerHeader, out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// Gets the trigger element name for the HTMX request
    /// </summary>
    public static string? GetHtmxTriggerName(this HttpRequest request)
    {
        return request.Headers.TryGetValue(HxTriggerNameHeader, out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// Gets the current URL from which the HTMX request was made
    /// </summary>
    public static string? GetHtmxCurrentUrl(this HttpRequest request)
    {
        return request.Headers.TryGetValue(HxCurrentUrlHeader, out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// Gets the prompt response if hx-prompt was used
    /// </summary>
    public static string? GetHtmxPrompt(this HttpRequest request)
    {
        return request.Headers.TryGetValue(HxPromptHeader, out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// Sets the HX-Redirect header to redirect the client
    /// </summary>
    public static void HtmxRedirect(this HttpResponse response, string url)
    {
        response.Headers["HX-Redirect"] = url;
    }

    /// <summary>
    /// Sets the HX-Refresh header to refresh the page
    /// </summary>
    public static void HtmxRefresh(this HttpResponse response)
    {
        response.Headers["HX-Refresh"] = "true";
    }

    /// <summary>
    /// Sets the HX-Push-Url header to push a new URL to history
    /// </summary>
    public static void HtmxPushUrl(this HttpResponse response, string url)
    {
        response.Headers["HX-Push-Url"] = url;
    }

    /// <summary>
    /// Sets the HX-Replace-Url header to replace the current URL in history
    /// </summary>
    public static void HtmxReplaceUrl(this HttpResponse response, string url)
    {
        response.Headers["HX-Replace-Url"] = url;
    }

    /// <summary>
    /// Sets the HX-Reswap header to change the swap behavior
    /// </summary>
    public static void HtmxReswap(this HttpResponse response, string swapStrategy)
    {
        response.Headers["HX-Reswap"] = swapStrategy;
    }

    /// <summary>
    /// Sets the HX-Retarget header to change the target element
    /// </summary>
    public static void HtmxRetarget(this HttpResponse response, string cssSelector)
    {
        response.Headers["HX-Retarget"] = cssSelector;
    }

    /// <summary>
    /// Triggers a client-side event using HX-Trigger header
    /// </summary>
    public static void HtmxTrigger(this HttpResponse response, string eventName)
    {
        response.Headers["HX-Trigger"] = eventName;
    }

    /// <summary>
    /// Triggers a client-side event after the swap completes
    /// </summary>
    public static void HtmxTriggerAfterSwap(this HttpResponse response, string eventName)
    {
        response.Headers["HX-Trigger-After-Swap"] = eventName;
    }

    /// <summary>
    /// Triggers a client-side event after the settle step
    /// </summary>
    public static void HtmxTriggerAfterSettle(this HttpResponse response, string eventName)
    {
        response.Headers["HX-Trigger-After-Settle"] = eventName;
    }
}