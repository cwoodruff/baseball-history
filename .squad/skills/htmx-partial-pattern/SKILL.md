# HTMX Partial Response Pattern

## Overview

This project uses a consistent pattern for detecting HTMX requests and returning partial views for AJAX interactions while full pages for standard navigation.

## The Pattern

### 1. Detect Request Type

```csharp
public bool IsHtmxNonBoostedRequest(this HttpRequest request)
{
    return request.Headers.ContainsKey("HX-Request")
        && !request.Headers.ContainsKey("HX-Boosted");
}
```

**Key distinction:**
- **`HX-Request`** alone = boosted navigation (SPA-like), return full page
- **`HX-Request` + `!HX-Boosted`** = targeted AJAX request, return partial
- **No `HX-Request` header** = standard browser navigation, return full page

### 2. Return Appropriate Response

```csharp
public async Task<IActionResult> OnGetAsync(string? letter, [FromQuery] int page = 1)
{
    // Load data (shared between full page and partial)
    var viewModel = await LoadDataAsync(letter, page);
    
    // Choose response type
    if (Request.IsHtmxNonBoostedRequest())
        return Partial("_PlayerList", viewModel);
    
    return Page();  // Full page for standard navigation
}
```

### 3. Leverage Response Caching

```csharp
[ResponseCache(
    Duration = 3600, 
    Location = ResponseCacheLocation.Client, 
    VaryByHeader = "HX-Request"  // Separate caches for AJAX vs full page
)]
public class IndexModel : PageModel
{
    // ...
}
```

The `VaryByHeader = "HX-Request"` ensures:
- Full page requests are cached separately from AJAX requests
- No stale partial views served to full-page requests
- Maximum cache reuse

## Where This Pattern Lives

- **Detection:** `Extensions/HtmxExtensions.cs`
- **Usage:** All page models (`Pages/**/*.cshtml.cs`)
- **Tests:** `baseball-history-tests/Extensions/HtmxExtensionsTests.cs`

## When to Use

✅ **Use this pattern for:**
- Pagination (page load should return partial)
- Filtering (dropdown change → new filtered list)
- Modal triggers (click → fetch player detail)
- Search (type → fetch results)

❌ **Don't use for:**
- Full page navigation (let hx-boost return full page)
- Initial page load (browser doesn't send HX-Request header)

## Common Mistakes to Avoid

1. **Checking only `HX-Request`** — will return partials for boosted navigation (breaks experience)
   ```csharp
   // ❌ WRONG
   if (Request.Headers.ContainsKey("HX-Request"))
       return Partial(...);
   
   // ✅ RIGHT
   if (Request.IsHtmxNonBoostedRequest())
       return Partial(...);
   ```

2. **Not setting `VaryByHeader` on ResponseCache** — partials will be cached as full pages
   ```csharp
   // ❌ WRONG
   [ResponseCache(Duration = 3600)]
   
   // ✅ RIGHT
   [ResponseCache(Duration = 3600, VaryByHeader = "HX-Request")]
   ```

3. **Different data loading for partial vs full page** — leads to inconsistency
   ```csharp
   // ❌ WRONG
   var data = Request.IsHtmxNonBoostedRequest() 
       ? await LoadPartialData()
       : await LoadFullPageData();
   
   // ✅ RIGHT
   var data = await LoadData();  // Same for both
   if (Request.IsHtmxNonBoostedRequest())
       return Partial("_Partial", data);
   return Page();
   ```

## Testing This Pattern

```csharp
[Fact]
public async Task OnGetAsync_WithHtmxRequest_ReturnsPartial()
{
    // Arrange
    var model = new PlayerIndexModel(context, cache);
    model.HttpContext.Request.Headers["HX-Request"] = "true";
    // Ensure NO HX-Boosted header
    
    // Act
    var result = await model.OnGetAsync("A", 1);
    
    // Assert
    Assert.IsType<PartialViewResult>(result);
}

[Fact]
public async Task OnGetAsync_WithHtmxBoostedRequest_ReturnsFullPage()
{
    // Arrange
    var model = new PlayerIndexModel(context, cache);
    model.HttpContext.Request.Headers["HX-Request"] = "true";
    model.HttpContext.Request.Headers["HX-Boosted"] = "true";  // Both headers
    
    // Act
    var result = await model.OnGetAsync("A", 1);
    
    // Assert
    Assert.IsType<PageResult>(result);
}
```

## htmxRazor Migration Note

When migrating partials to htmxRazor components:
1. This detection pattern **remains unchanged**
2. Return a component instance instead of `Partial()`:
   ```csharp
   if (Request.IsHtmxNonBoostedRequest())
       return ViewComponent("PlayerList", viewModel);
   ```
3. Or later, return component-rendered HTML via a service

The HTMX interaction logic is orthogonal to the component framework choice.
