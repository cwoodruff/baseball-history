# Development Guide

This document provides guidelines for developing and extending the Baseball
History application.

## Development Setup

### Prerequisites

- .NET 10.0 SDK
- IDE: Visual Studio 2022, VS Code with C# extension, or JetBrains Rider
- Git
- Access to a PostgreSQL database loaded with Lahman data

### Getting Started

```bash
# Clone repository
git clone <repository-url>
cd baseball-history

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Preferred: start the Aspire AppHost
aspire start --apphost baseball-history-aspire/baseball-history-aspire.csproj
```

Use `aspire describe --apphost baseball-history-aspire/baseball-history-aspire.csproj`
to inspect the dashboard URL and the current `web` endpoint that Aspire
assigned.

### Standalone Startup Still Supported

```bash
# Set the PostgreSQL connection string locally
dotnet user-secrets set --project baseball-history-web \
  "ConnectionStrings:Lahman" \
  "Host=localhost;Port=5432;Database=lahman;Username=postgres;Password=YOUR_LOCAL_PASSWORD;SSL Mode=Disable"

# Run the application without Aspire
dotnet run --project baseball-history-web
```

Issue #19 adds an orchestration layer; it does not replace the existing web
project startup flow.

### Optional: Run the MCP server locally

The repository also ships an MCP host in `baseball-history-mcp` that serves
streamable HTTP on port 5190.

```bash
# Configure the shared PostgreSQL connection string
dotnet user-secrets set --project baseball-history-mcp \
  "ConnectionStrings:Lahman" \
  "Host=<server>;Port=5432;Database=lahman;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true"

# Serve streamable HTTP on http://localhost:5190 (health check at /healthz)
dotnet run --project baseball-history-mcp
```

The Aspire AppHost also hosts the MCP server as the `mcp` resource, so
`aspire run` starts it alongside the web app.

Use [MCP-SERVER-PLAN.md](./MCP-SERVER-PLAN.md) for the full shipped surface,
sample client configuration, non-goals, and adoption guidance.

### Database Setup

This repo now expects PostgreSQL at runtime. See [POSTGRES-MIGRATION.md](./POSTGRES-MIGRATION.md) for the full migration and Azure rollout guidance.

1. Provision or obtain a PostgreSQL database loaded with the Lahman schema/data
2. Store the connection string outside git with `dotnet user-secrets`:
   ```bash
   dotnet user-secrets set --project baseball-history-web \
     "ConnectionStrings:Lahman" \
     "Host=<server>;Port=5432;Database=lahman;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true"
   ```
3. The app reads the same `ConnectionStrings:Lahman` key from user-secrets,
   environment variables (`ConnectionStrings__Lahman`), Azure App Service
   settings, or Key Vault-backed configuration
4. If running through Aspire, restart the AppHost after updating the connection
   string so the `web` resource reconnects cleanly
5. Treat the old `lahman.db` file as migration input only; it is no longer the
   runtime database for new setups

### Configuration

**appsettings.json** (safe sample only):

```json
{
  "ConnectionStrings": {
    "Lahman": "Host=<set-in-user-secrets-or-app-service>;Port=5432;Database=lahman;Username=<set-in-user-secrets-or-app-service>;Password=<set-in-user-secrets-or-app-service>;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

Use user-secrets for local work. In Azure, prefer an App Service setting named
`ConnectionStrings__Lahman` whose value is a Key Vault reference. Direct App
Service storage is acceptable only as a short-term fallback.

---

## Project Structure

```
baseball-history/
├── baseball-history-aspire/
│   ├── AppHost.cs            # Aspire resource graph for local orchestration
│   ├── appsettings.json
│   └── Properties/
├── baseball-history-web/
│   ├── Extensions/           # Extension methods
│   │   └── HtmxExtensions.cs
│   ├── Models/               # Entity models
│   │   ├── BaseballDbContext.cs
│   │   ├── People.cs
│   │   ├── Batting.cs
│   │   └── ...
│   ├── ViewModels/           # View models
│   │   ├── PlayerDetailViewModel.cs
│   │   ├── LeaderboardViewModel.cs
│   │   └── ...
│   ├── Pages/                # Razor Pages
│   │   ├── Index.cshtml
│   │   ├── Players/
│   │   ├── Teams/
│   │   ├── Stats/
│   │   ├── HallOfFame/
│   │   └── Shared/
│   ├── wwwroot/              # Static files
│   │   ├── css/
│   │   ├── js/
│   │   └── lib/
│   └── Program.cs            # Entry point
└── docs/                     # Documentation
```

---

## Coding Guidelines

### Razor Pages

Each page consists of:

- `PageName.cshtml` - View markup
- `PageName.cshtml.cs` - Page model with handlers

```csharp
public class ExampleModel : PageModel
{
    private readonly BaseballDbContext _context;

    public ExampleModel(BaseballDbContext context)
    {
        _context = context;
    }

    public ExampleViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, int page = 1)
    {
        // Load data
        ViewModel = await LoadDataAsync(id, page);

        // Return partial for HTMX requests
        if (Request.IsHtmxNonBoostedRequest())
            return Partial("_ExamplePartial", ViewModel);

        return Page();
    }
}
```

### Aspire AppHost Notes

- `baseball-history-aspire` is a dev-only orchestration project. It references
  `baseball-history-web` as a project resource and exposes it through Aspire's
  dashboard and resource model.
- The AppHost uses the existing home page (`/`) as a lightweight HTTP health
  probe so the web project stays unchanged.
- Do not add the Aspire SDK or service-defaults packages to
  `baseball-history-web` unless a future issue explicitly approves runtime
  changes there.

### View Models

- Create dedicated view models for each view
- Include formatting methods for display values
- Use static factory methods for entity-to-viewmodel conversion

```csharp
public class ExampleViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }

    // Formatted display properties
    public string FormattedValue => Value.ToString("N2");

    // Factory method
    public static ExampleViewModel FromEntity(ExampleEntity entity)
    {
        return new ExampleViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Value = entity.Value
        };
    }
}
```

### Database Queries

- Use async methods (`ToListAsync`, `FirstOrDefaultAsync`)
- Include related entities when needed (`.Include()`)
- Project to anonymous types or view models for efficiency
- Parse string fields in memory, not in LINQ expressions

```csharp
// Good: Efficient projection
var data = await _context.Entity
    .Where(e => e.Active)
    .Select(e => new
    {
        e.Id,
        e.Name,
        RelatedName = e.Related.Name
    })
    .ToListAsync();

// Good: Parse strings in memory
var rawData = await _context.Batting.ToListAsync();
var sum = rawData.Sum(b => int.TryParse(b.Rbi, out var r) ? r : 0);

// Bad: Won't translate to SQL
var sum = await _context.Batting.SumAsync(b => Convert.ToInt32(b.Rbi));
```

### HTMX Patterns

- Use `hx-boost="false"` on elements that shouldn't be boosted
- Add `hx-indicator` for loading feedback
- Use `hx-push-url="true"` for browser history updates

```html
<a hx-get="/Example/Action"
   hx-target="#content"
   hx-indicator="#loading"
   hx-push-url="true"
   hx-boost="false">
    Click Me
</a>
```

### CSS

- Use CSS custom properties for colors
- Follow BEM-like naming for custom classes
- Put MLB theme styles in `site.css`
- Put team colors in `team-colors.css`

```css
/* Good: Use variables */
.example-header {
    background-color: var(--mlb-navy);
    color: var(--mlb-white);
}

/* Good: Component class naming */
.player-card { }
.player-card-header { }
.player-card-stats { }
```

---

## Adding New Features

### Adding a New Page

1. Create page files in appropriate folder:
   ```
   Pages/NewFeature/Index.cshtml
   Pages/NewFeature/Index.cshtml.cs
   ```

2. Create view model if needed:
   ```
   ViewModels/NewFeatureViewModel.cs
   ```

3. Create partial view for HTMX:
   ```
   Pages/NewFeature/_Content.cshtml
   ```

4. Add navigation link to `_Layout.cshtml`

### Adding a New Entity

1. Create model class:
   ```csharp
   public class NewEntity
   {
       public int Id { get; set; }
       // Properties...
   }
   ```

2. Add DbSet to context:
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

3. Configure in `OnModelCreating`:
   ```csharp
   modelBuilder.Entity<NewEntity>(entity =>
   {
       entity.HasKey(e => e.Id);
       // Configuration...
   });
   ```

### Adding Team Colors

1. Add to `team-colors.css`:
   ```css
   [data-team="XXX"], [data-franchise="XXX"] {
       --team-primary: #XXXXXX;
       --team-secondary: #XXXXXX;
       --team-accent: #FFFFFF;
   }
   ```

2. Use `data-team` attribute on elements:
   ```html
   <div data-team="NYA">
       <!-- Uses Yankees colors -->
   </div>
   ```

---

## Testing

### Manual Testing

1. Test full page loads (direct URL)
2. Test HTMX navigation (click links)
3. Test HTMX partials (filters, pagination)
4. Test modals (open/close)
5. Test on mobile viewport
6. Test keyboard navigation

### Health Check

The application includes a health check page at `/Health` that verifies:

- Database connectivity
- Record counts for key tables

---

## Debugging

### Common Issues

**HTMX not working:**

- Check browser console for errors
- Verify `hx-boost="false"` on specific elements
- Check for JavaScript errors

**Modal not closing properly:**

- Verify Bootstrap modal disposal in layout script
- Check for leftover `.modal-backdrop` elements

**Database queries failing:**

- Check `ConnectionStrings:Lahman` in user-secrets, environment variables, or App Service configuration
- Verify the PostgreSQL server is reachable and the user has read access
- Check for LINQ translation errors (use `.ToList()` before complex operations)

### Logging

Enable detailed logging in development:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

## Deployment

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

### Environment Variables

| Variable                               | Description         |
|----------------------------------------|---------------------|
| `ASPNETCORE_ENVIRONMENT`               | Set to "Production" |
| `ConnectionStrings__Lahman`            | PostgreSQL connection string |

### Checklist

- [ ] Build in Release mode
- [ ] Verify `ConnectionStrings__Lahman` is configured in the target environment
- [ ] Set environment to Production
- [ ] Enable HTTPS
- [ ] Configure logging
- [ ] Test all features

---

## Performance Tips

1. **Use `.AsNoTracking()`** for read-only queries
2. **Project only needed fields** with `.Select()`
3. **Paginate at database level** when possible
4. **Cache static data** (team colors, available years)
5. **Use loading indicators** for slow operations
6. **Minimize JavaScript** - prefer HTMX patterns
