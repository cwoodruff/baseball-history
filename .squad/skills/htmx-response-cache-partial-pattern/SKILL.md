---
name: "htmx-response-cache-partial-pattern"
description: "Dual-mode response caching that separates full-page and htmx partial responses with VaryByHeader. Essential for htmxRazor migration to avoid stale content."
domain: "caching, htmx-integration, response-handling"
confidence: "high"
source: "observed in Sprint 1; proven in 294+ regression tests; applied to all Sprint 2 feature pages"
tools:
  - name: "response-cache-attribute"
    description: "[ResponseCache] attribute with VaryByHeader for dual-mode caching"
    when: "Every page handler that supports both full-page and htmx partial responses"
---

## Context

When migrating Razor Pages to htmxRazor, pages render in two modes:
1. **Full-page request** (normal navigation): Returns complete HTML with shell (header, nav, footer, modals)
2. **htmx non-boosted request** (AJAX): Returns only content partial (no shell, no nav, no modals)

Without careful caching, htmx partials can be served stale from a full-page cache entry, breaking the UI. Similarly, full-page responses must not be served from partial caches.

ASP.NET Core's HTTP response cache uses `VaryByHeader` to generate separate cache keys. By varying on `HX-Request` header, we ensure htmx and non-htmx responses are cached separately.

## Patterns

### Pattern 1: Handler with Dual-Mode Response Cache

```csharp
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
public class MyPageModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // Load data once (same query for both modes)
        var data = await LoadData();

        // Check if request is htmx non-boosted
        if (Request.IsHtmxNonBoostedRequest())
        {
            // Return partial only (no shell)
            return Partial("_Content", data);
        }

        // Return full page (with shell)
        return Page();
    }
}
```

## Examples

### Baseball History Players Page

Live example: `Pages/Players/Index.cshtml.cs` — dual-mode Players page with shared cache

### Baseball History Teams Page

Live example: `Pages/Teams/Index.cshtml.cs` — dual-mode Teams page with league filtering

## Anti-Patterns

### ❌ Anti-Pattern 1: No Response Cache (Performance Regression)
Every request hits database. Add `[ResponseCache(...)]` attribute.

### ❌ Anti-Pattern 2: Forgetting VaryByHeader
First request returns full-page. Second htmx request returns cached full-page (with shell!). UI breaks.
Add `VaryByHeader = "HX-Request"`.

### ❌ Anti-Pattern 3: Lazy-Loading IQueryable in Partial View
Partial receives `IQueryable`, not materialized list. Component renders; query executes AGAIN.
Fix: Call `.ToListAsync()` in handler before returning partial.

## References

- `Extensions/HtmxExtensions.cs` — IsHtmxNonBoostedRequest() implementation
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs` — Regression tests
