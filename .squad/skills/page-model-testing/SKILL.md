# Skill: Page Model Unit Testing in ASP.NET Core Razor Pages

**Domain:** Testing | Razor Pages | Handler coverage | Regression safety  
**Applies to:** baseball-history-web page models, any Razor Pages application  
**Difficulty:** Intermediate  

## Overview

Page model unit testing in ASP.NET Core requires proper initialization of `PageContext`, `ViewDataDictionary`, and `ITempDataDictionary`. Without these, calls to `Partial()` and form binding fail with NullReferenceException. This skill documents the pattern used in `PageModelTestBase` for safe, reusable page model testing.

## Pattern: Enhanced PageModelTestBase

### 1. Infrastructure Setup

**Add required package to test project .csproj:**
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc" Version="10.0.5" />
```

**Create base helper class (or extend existing `PageModelTestBase`):**

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

public abstract class PageModelTestBase
{
    // Existing helpers...
    
    /// <summary>
    /// Creates a properly initialized PageContext for model binding and partial rendering
    /// </summary>
    protected static PageContext CreatePageContext(DefaultHttpContext? httpContext = null)
    {
        httpContext ??= new DefaultHttpContext();
        
        return new PageContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor())
        )
        {
            ViewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(), 
                new ModelStateDictionary()
            )
        };
    }
    
    /// <summary>
    /// Initializes a PageModel with ViewData and TempData for partial rendering
    /// Call this before invoking OnGetAsync/OnPostAsync on the model
    /// </summary>
    protected static void InitializePageModelForPartialRendering(PageModel model, DefaultHttpContext? httpContext = null)
    {
        httpContext ??= new DefaultHttpContext();
        
        model.PageContext = CreatePageContext(httpContext);
        model.ViewData = model.PageContext.ViewData;
        model.TempData = CreateTempData(httpContext);
    }
    
    protected static ITempDataDictionary CreateTempData(HttpContext httpContext) =>
        new TempDataDictionary(httpContext, new TestTempDataProvider());

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) 
            => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
```

### 2. Test Template: Full Page vs. Partial Response

```csharp
[Fact]
public async Task OnGetAsync_WithoutHtmxHeader_ReturnsPageResult()
{
    using var context = CreateContext();
    using var cache = CreateMemoryCache();
    
    var httpContext = new DefaultHttpContext();
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model, httpContext);
    
    var result = await model.OnGetAsync("A", 1);
    
    Assert.IsType<PageResult>(result);
    Assert.NotNull(model.ViewModel);
}

[Fact]
public async Task OnGetAsync_WithHtmxHeader_ReturnsPartialView()
{
    using var context = CreateContext();
    using var cache = CreateMemoryCache();
    
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["HX-Request"] = "true";
    
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model, httpContext);
    
    var result = await model.OnGetAsync("A", 1);
    
    var partial = Assert.IsType<PartialViewResult>(result);
    Assert.Equal("_PlayersContent", partial.ViewName);
    Assert.NotNull(partial.Model);
}
```

### 3. Test Template: Pagination Edge Cases

```csharp
[Fact]
public async Task OnGetAsync_WithPageZero_ClampsToOne()
{
    using var context = CreateContext();
    using var cache = CreateMemoryCache();
    
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model);
    
    await model.OnGetAsync("A", page: 0);
    
    Assert.Equal(1, model.ViewModel.CurrentPage);
}

[Fact]
public async Task OnGetAsync_WithPageAboveMax_ClampsToTotalPages()
{
    using var context = CreateContext();
    using var cache = CreateMemoryCache();
    
    var model = new Players.IndexModel(context, cache);
    InitializePageModelForPartialRendering(model);
    
    await model.OnGetAsync("A", page: int.MaxValue);
    
    Assert.True(model.ViewModel.TotalPages >= 1);
    Assert.Equal(model.ViewModel.TotalPages, model.ViewModel.CurrentPage);
}
```

## Key Patterns

### HTTP Headers in Tests
```csharp
var httpContext = new DefaultHttpContext();
httpContext.Request.Headers["HX-Request"] = "true";  // htmx request
httpContext.Request.Headers["HX-Boosted"] = "true";  // boosted link
```

### Result Type Assertions
```csharp
Assert.IsType<PageResult>(result);              // Full page
Assert.IsType<PartialViewResult>(result);       // Partial view
Assert.IsType<RedirectResult>(result);          // Redirect
Assert.IsType<NotFoundResult>(result);          // 404
```

### PartialViewResult Properties
```csharp
var partial = Assert.IsType<PartialViewResult>(result);
Assert.Equal("_PartialName", partial.ViewName);
Assert.NotNull(partial.Model);  // Verify model passed
Assert.IsType<MyViewModel>(partial.Model);
```

## Common Mistakes to Avoid

| Mistake | Why It Fails | Fix |
|---------|-------------|-----|
| Not initializing `PageContext.ViewData` | `Partial()` needs ViewData to exist | Call `InitializePageModelForPartialRendering()` |
| Sharing `MemoryCache` across tests | Cache keys collide, tests interfere | Use `new MemoryCache()` per test |
| Not setting HTTP context headers | Routing logic depends on headers | Set `httpContext.Request.Headers` before model init |
| Asserting only ViewModel, not Result type | Partial-vs-Page routing untested | Use `Assert.IsType<PartialViewResult>()` |
| Not calling `CreateContext()` for DB queries | Queries fail with connection error | Always use helper; it resolves DB path |

## When to Use This Pattern

✅ **Use for:**
- Testing page handler routing decisions (full page vs partial)
- Validating pagination edge cases
- Verifying filter/sort logic in handlers
- Testing cache behavior
- Regression coverage for form submissions
- Edge case validation (page 0, negative numbers, oversized results)

❌ **Don't use for:**
- Integration tests with full HTTP pipeline (use `WebApplicationFactory` instead)
- View rendering tests (unit test logic in view models instead)
- Database schema validation (use `DatabaseIntegrationTests`)
- Static file serving
- HTTPS/redirect chains

## Related Skills

- **htmx Request Detection:** Verify `Request.IsHtmxNonBoostedRequest()` pattern
- **ViewModel Projection:** Test `.Select()` query patterns before handlers
- **Pagination Boundaries:** `Math.Clamp(page, 1, Math.Max(1, totalPages))`
- **Memory Cache Testing:** Fresh cache per test to avoid key collisions

## References

- Microsoft Docs: [Testing Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/testing)
- Test file: `baseball-history-tests/Pages/PageModelTestBase.cs`
- Example tests: `baseball-history-tests/Pages/PlayersIndexModelTests.cs`
