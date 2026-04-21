# Frontend Design

This document describes the frontend architecture, including HTMX patterns,
Bootstrap theming, and CSS design.

## Technology Stack

| Technology | Version | Purpose                                      |
|------------|---------|----------------------------------------------|
| htmxRazor  | 2.0.1   | Razor component/tag-helper integration       |
| HTMX       | 2.0.4   | Dynamic content without custom page scripts  |
| Bootstrap  | 5.x     | Responsive UI framework                      |
| Custom CSS | -       | MLB theming and components                   |

## HTMX Patterns

### Overview

HTMX enables dynamic, SPA-like behavior without writing JavaScript. The
application uses HTMX for:

- Partial page updates
- Modal loading
- Form filtering
- Pagination
- Search results

### Boost Mode

The entire application uses `hx-boost="true"` on the body element, which:

- Intercepts all anchor clicks
- Fetches pages via AJAX
- Swaps only the body content
- Updates browser history

```html
<body hx-boost="true">
    <!-- All navigation is boosted -->
</body>
```

### htmxRazor Asset Strategy

- htmxRazor foundation assets are injected automatically by `AddhtmxRazor()` / `UsehtmxRazor()`.
- Component CSS stays centralized in `Pages/Shared/_Layout.cshtml` so styles survive `hx-boost` body swaps.
- Current retained component imports are:
  - `/_rhx/css/components/rhx-button.css` for the About-page proof button
  - `/_rhx/css/components/rhx-badge.css` for migrated team and leaderboard badges
- No additional component JavaScript imports are currently required; shell-owned modal/search/dropdown behavior lives in `_Layout.cshtml` with the Bootstrap bundle.

### Partial Responses

For HTMX requests, page models return partial views instead of full pages:

```csharp
if (Request.IsHtmxNonBoostedRequest())
    return Partial("_PartialView", viewModel);
return Page();
```

Detecting HTMX requests:

```csharp
public static class HtmxExtensions
{
    public static bool IsHtmxRequest(this HttpRequest request)
        => request.Headers.ContainsKey("HX-Request");

    public static bool IsHtmxBoosted(this HttpRequest request)
        => request.Headers.ContainsKey("HX-Boosted");

    public static bool IsHtmxNonBoostedRequest(this HttpRequest request)
        => request.IsHtmxRequest() && !request.IsHtmxBoosted();
}
```

### Common HTMX Attributes

```html
<!-- Basic GET request -->
<a hx-get="/Players/Modal/ruthba01"
   hx-target="#modal-container"
   hx-swap="innerHTML">
    Babe Ruth
</a>

<!-- Form with filters -->
<select hx-get="/Stats/Batting"
        hx-include="#filter-form"
        hx-target="#leaderboard"
        hx-indicator="#loading"
        hx-push-url="true">
    ...
</select>

<!-- Pagination -->
<a hx-get="/Players?page=2&letter=A"
   hx-target="#player-list"
   hx-push-url="true">
    Next
</a>
```

### Modal Pattern

Modals are loaded via HTMX into a container, then initialized with Bootstrap
JavaScript:

```html
<!-- Layout contains the container -->
<div id="modal-container"></div>

<!-- Links trigger modal loading -->
<a href="#"
   hx-get="/Players/Modal/ruthba01"
   hx-target="#modal-container"
   hx-swap="innerHTML"
   hx-boost="false">
    View Player
</a>
```

JavaScript initialization after HTMX swap:

```javascript
document.body.addEventListener('htmx:afterSwap', function(evt) {
    if (evt.detail.target.id === 'modal-container') {
        var modal = evt.detail.target.querySelector('.modal');
        if (modal) {
            var bsModal = new bootstrap.Modal(modal);
            bsModal.show();

            modal.addEventListener('hidden.bs.modal', function() {
                bsModal.dispose();
                evt.detail.target.innerHTML = '';
                // Clean up backdrop
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
                document.body.classList.remove('modal-open');
            });
        }
    }
});
```

### Loading Indicators

HTMX indicators show during requests:

```html
<!-- Indicator element -->
<div id="loading-indicator" class="htmx-indicator loading-baseball">
    <div class="loading-baseball-icon">&#9918;</div>
    <div class="loading-baseball-text">Loading stats...</div>
</div>

<!-- Elements reference the indicator -->
<select hx-get="/Stats/Batting"
        hx-indicator="#loading-indicator">
```

CSS for indicators:

```css
.htmx-indicator {
    opacity: 0;
    transition: opacity 200ms ease-in;
}

.htmx-request .htmx-indicator {
    opacity: 1;
}
```

### Cache Behavior During HTMX Navigation

Pages that support both full-page and non-boosted HTMX responses use:

```csharp
[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client, VaryByHeader = "HX-Request")]
```

This keeps full-page shell responses separate from partial-only responses. Shared lookup data (letters, years, leagues, Hall of Fame IDs, and the warmed default Players page) stays in `IMemoryCache` for 24 hours.

**Operational follow-through when refreshing `lahman.db`:**
1. Replace the database file.
2. Restart each app instance so `IMemoryCache` and the hosted `PlayerCacheService` rebuild from the new data.
3. Verify one full-page request and one non-boosted HTMX request on a migrated page (for example `/Players` and `/Stats/Batting`) so both response-cache variants are repopulated.

## CSS Architecture

### Color System

MLB official colors are defined as CSS custom properties:

```css
:root {
    /* MLB Official Colors */
    --mlb-navy: #002D72;
    --mlb-red: #D50032;
    --mlb-white: #FFFFFF;

    /* Derived colors */
    --mlb-navy-light: #1a4a8f;
    --mlb-navy-dark: #001a4d;
    --mlb-red-light: #e6334d;

    /* Team color placeholders */
    --team-primary: var(--mlb-navy);
    --team-secondary: var(--mlb-red);
    --team-accent: var(--mlb-white);
}
```

### Team Colors

Each of the 30 MLB teams has custom colors in `team-colors.css`:

```css
/* New York Yankees */
[data-team="NYA"], [data-franchise="NYY"] {
    --team-primary: #003087;
    --team-secondary: #003087;
    --team-accent: #FFFFFF;
}

/* Boston Red Sox */
[data-team="BOS"], [data-franchise="BOS"] {
    --team-primary: #BD3039;
    --team-secondary: #0C2340;
    --team-accent: #FFFFFF;
}
```

Usage in HTML:

```html
<div data-team="NYA">
    <!-- This element and children use Yankees colors -->
    <div class="card-header-team">...</div>
</div>
```

### Component Classes

#### Navigation

```css
.navbar-mlb {
    background: linear-gradient(135deg, var(--mlb-navy) 0%, var(--mlb-navy-dark) 100%);
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.15);
}

.navbar-mlb .nav-link:hover {
    color: var(--mlb-white);
    background-color: rgba(255, 255, 255, 0.1);
}
```

#### Cards

```css
.card {
    border: 1px solid var(--card-border);
    border-radius: 0.5rem;
    box-shadow: 0 2px 8px var(--card-shadow);
    transition: transform 0.2s ease;
}

.card:hover {
    transform: translateY(-2px);
}

.card-header-mlb {
    background: linear-gradient(135deg, var(--mlb-navy) 0%, var(--mlb-navy-light) 100%);
    color: var(--mlb-white);
    border-bottom: 3px solid var(--mlb-red);
}
```

#### Tables

```css
.table-baseball thead {
    background-color: var(--mlb-navy);
    color: var(--mlb-white);
}

.table-baseball tbody tr:hover {
    background-color: rgba(0, 45, 114, 0.05);
}

.table-baseball .stat-cell {
    font-family: 'SF Mono', 'Monaco', 'Consolas', monospace;
    text-align: right;
}
```

#### Badges

```css
.hof-badge {
    background: linear-gradient(135deg, #d4af37 0%, #f4d03f 50%, #d4af37 100%);
    color: #1a1a1a;
    font-weight: 700;
    text-transform: uppercase;
}

.stat-badge-primary {
    background-color: var(--mlb-navy);
    color: var(--mlb-white);
}
```

### Responsive Design

Mobile-first breakpoints:

```css
/* Base: Mobile */
html { font-size: 14px; }

/* Tablet and up */
@media (min-width: 768px) {
    html { font-size: 16px; }
}

/* Mobile adjustments */
@media (max-width: 576px) {
    .alphabet-nav .letter-link {
        width: 2rem;
        height: 2rem;
    }
}
```

### Animation

Loading spinner animation:

```css
@keyframes baseball-spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

.loading-baseball-icon {
    animation: baseball-spin 1s linear infinite;
}
```

Skeleton loading:

```css
@keyframes skeleton-loading {
    0% { background-position: 200% 0; }
    100% { background-position: -200% 0; }
}

.skeleton {
    background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
    animation: skeleton-loading 1.5s ease-in-out infinite;
}
```

## File Structure

```
wwwroot/
├── css/
│   ├── site.css           # Main stylesheet
│   └── team-colors.css    # 30 team color schemes
└── lib/
    └── bootstrap/         # Bootstrap 5
```

Shell lifecycle JavaScript is intentionally inline in `Pages/Shared/_Layout.cshtml` so modal cleanup, dropdown re-init, and search dismissal survive boosted body swaps without depending on a separate static asset.

## Bootstrap Components Used

| Component   | Usage                         |
|-------------|-------------------------------|
| Navbar      | Main navigation with dropdown |
| Cards       | Content containers            |
| Tables      | Statistical data display      |
| Modals      | Player detail popups          |
| Forms       | Filter controls               |
| Pagination  | Page navigation               |
| Badges      | Labels and status indicators  |
| List Groups | Team rosters, awards          |
| Grid System | Responsive layouts            |

## Accessibility

- Semantic HTML elements
- ARIA labels on interactive elements
- Keyboard navigation support
- Sufficient color contrast
- Focus indicators

```html
<button type="button"
        class="btn-close"
        data-bs-dismiss="modal"
        aria-label="Close">
</button>
```

## Browser Support

The application supports modern browsers:

- Chrome (last 2 versions)
- Firefox (last 2 versions)
- Safari (last 2 versions)
- Edge (last 2 versions)

HTMX provides graceful degradation for older browsers.
