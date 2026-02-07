# Architecture Overview

This document describes the system architecture of the Baseball History web
application.

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Browser                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │  Bootstrap  │  │    HTMX     │  │   Team Colors CSS       │  │
│  │     5.x     │  │    2.0.4    │  │   (30 team themes)      │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/HTMX Requests
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core 10.0                             │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                     Razor Pages                              ││
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐   ││
│  │  │ Players  │ │  Teams   │ │  Stats   │ │  HallOfFame  │   ││
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────────┘   ││
│  └─────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                    View Models                               ││
│  │  PlayerDetailVM, TeamSeasonVM, LeaderboardVM, etc.          ││
│  └─────────────────────────────────────────────────────────────┘│
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                Entity Framework Core 10.0                    ││
│  │  BaseballDbContext with 25+ entity configurations           ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ SQL Queries
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SQLite Database                              │
│                    (lahman.db ~60MB)                             │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────────┐   │
│  │  People  │ │ Batting  │ │ Pitching │ │ Teams/Franchises │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Design Patterns

### 1. Razor Pages Pattern

The application uses ASP.NET Core Razor Pages instead of MVC controllers. Each
page consists of:

- **Page Model** (`.cshtml.cs`): Handles HTTP requests and data loading
- **View** (`.cshtml`): Razor markup for rendering HTML
- **Partial Views** (`_*.cshtml`): Reusable components for HTMX responses

```csharp
public class IndexModel : PageModel
{
    public ViewModel ViewModel { get; set; }

    public async Task<IActionResult> OnGetAsync(...)
    {
        // Load data
        ViewModel = await LoadDataAsync();

        // Return partial for HTMX, full page otherwise
        if (Request.IsHtmxNonBoostedRequest())
            return Partial("_PartialView", ViewModel);

        return Page();
    }
}
```

### 2. HTMX Integration Pattern

HTMX enables dynamic content loading without custom JavaScript:

```html
<!-- Trigger: Select change loads new content -->
<select hx-get="/Stats/Batting"
        hx-include="#filter-form"
        hx-target="#leaderboard"
        hx-indicator="#loading"
        hx-push-url="true">
    ...
</select>

<!-- Target: Content area updated by HTMX -->
<div id="leaderboard">
    @await Html.PartialAsync("_Leaders", Model)
</div>
```

### 3. Partial View Response Pattern

Page models detect HTMX requests and return appropriate responses:

```csharp
// Extension method for detecting HTMX requests
public static bool IsHtmxNonBoostedRequest(this HttpRequest request)
{
    return request.Headers.ContainsKey("HX-Request")
        && !request.Headers.ContainsKey("HX-Boosted");
}

// Usage in page model
if (Request.IsHtmxNonBoostedRequest())
    return Partial("_PartialView", viewModel);
return Page();
```

### 4. View Model Pattern

View models transform entity data for presentation:

```csharp
public class PlayerDetailViewModel
{
    public string PlayerId { get; set; }
    public string FullName { get; set; }
    public CareerBattingStats BattingStats { get; set; }
    public List<SeasonBattingRecord> BattingSeasons { get; set; }

    public static PlayerDetailViewModel FromPeople(People person)
    {
        // Transform entity to view model
    }
}
```

## Request Flow

### Standard Page Request

```
1. Browser → GET /Players
2. Razor Page → Query database via EF Core
3. EF Core → Execute SQL on SQLite
4. Page Model → Build ViewModel
5. Razor View → Render full HTML page
6. Browser ← Complete HTML document
```

### HTMX Partial Request

```
1. User action → HTMX intercepts
2. HTMX → GET /Players?letter=B (with HX-Request header)
3. Page Model → Detects HTMX request
4. Page Model → Returns Partial("_PlayerList", viewModel)
5. HTMX ← Receives HTML fragment
6. HTMX → Swaps content into target element
```

### Modal Loading

```
1. Click player name → HTMX triggers
2. HTMX → GET /Players/Modal/{id}
3. Modal Page → Load player data
4. Return → Partial("_PlayerModal", player)
5. HTMX → Insert into #modal-container
6. JavaScript → Initialize Bootstrap modal
7. Bootstrap → Show modal with backdrop
```

## Component Architecture

### Layout Structure

```
_Layout.cshtml
├── <head> (CSS, meta)
├── <header>
│   └── navbar-mlb (navigation, search)
├── <main>
│   └── @RenderBody() (page content)
├── <footer>
│   └── footer-mlb
├── #modal-container (for HTMX modals)
└── <scripts> (HTMX, Bootstrap, custom)
```

### Shared Components

| Component         | Location             | Purpose                        |
|-------------------|----------------------|--------------------------------|
| `_Pagination`     | `Shared/Components/` | Reusable pagination with HTMX  |
| `_EmptyState`     | `Shared/Components/` | Consistent empty state display |
| `_LoadingSpinner` | `Shared/Components/` | Loading indicators             |
| `_PlayerCard`     | `Shared/Components/` | Player card for grids          |
| `_PlayerModal`    | `Players/`           | Full player detail modal       |

## Performance Considerations

### Database Optimization

1. **Eager Loading**: Use `.Include()` for related entities
2. **Projection**: Select only needed columns
3. **Pagination**: Database-level pagination where possible
4. **Indexing**: Leverage existing Lahman database indexes

### Frontend Optimization

1. **HTMX Boost**: SPA-like navigation without full reloads
2. **Partial Updates**: Only update changed content areas
3. **Loading Indicators**: Visual feedback during data fetch
4. **CSS Variables**: Efficient theming without duplication

## Error Handling

### Page-Level Errors

```csharp
public async Task<IActionResult> OnGetAsync(string id)
{
    if (string.IsNullOrEmpty(id))
        return NotFound();

    var data = await _context.Entity.FindAsync(id);
    if (data == null)
        return NotFound();

    // Continue processing...
}
```

### Global Error Page

The application includes an Error page (`/Error`) that displays user-friendly
error messages while logging details for debugging.

## Security Considerations

1. **Input Validation**: All query parameters validated
2. **SQL Injection**: EF Core parameterized queries
3. **XSS Prevention**: Razor automatic HTML encoding
4. **HTTPS**: Enforced in production
