# Baseball History Web App

A web application for exploring Major League Baseball history using the Lahman
Baseball Database. Baseball data and statistics from 1871 to 2025. Built with ASP.NET Core Razor Pages, Entity Framework Core,
htmxRazor, htmx, and Bootstrap.

![Home Screenshot](./docs/home-screenshot.png)

## Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Documentation](#documentation)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)

## Overview

Baseball History is an interactive web application that allows users to explore
over 150 years of Major League Baseball statistics, including:

- **Player Browser**: Browse players alphabetically with career statistics
- **Team Browser**: Explore franchises and their season-by-season history
- **Statistical Leaders**: View batting and pitching leaderboards with filters
- **Hall of Fame**: Browse Hall of Fame inductees by year and category
- **Search**: Global search across players and teams

## Technology Stack

| Component | Technology                           |
|-----------|--------------------------------------|
| Backend   | ASP.NET Core 10.0, Razor Pages       |
| Database  | SQLite with Lahman Baseball Database |
| ORM       | Entity Framework Core 10.0           |
| Frontend  | htmxRazor 2.0.1, htmx 2.0.4, Bootstrap 5 |

## Documentation

- [Architecture Overview](./docs/ARCHITECTURE.md) - System architecture and design
  patterns
- [Database Design](./docs/DATABASE.md) - Database schema and Entity Framework
  configuration
- [Frontend Design](./docs/FRONTEND.md) - htmx patterns, Bootstrap theming, and CSS
  architecture
- [Features](./docs/FEATURES.md) - Detailed feature documentation
- [API Reference](./docs/API.md) - Page models and data flow

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQLite (included with .NET)
- Aspire CLI (`aspire`) for the orchestrated local development workflow

### Running the Application

#### Preferred: Aspire AppHost orchestration

```bash
# Clone the repository
git clone <repository-url>
cd baseball-history

# Build the solution
dotnet build baseball-history.sln

# Start the Aspire AppHost
aspire start --apphost baseball-history-aspire/baseball-history-aspire.csproj

# Run the regression suite
dotnet test baseball-history-tests
```

Use `aspire describe --apphost baseball-history-aspire/baseball-history-aspire.csproj`
to see the dashboard URL and the dynamically assigned web endpoint for the `web`
resource.

#### Backward-compatible standalone web startup

```bash
# Run the web application without Aspire
dotnet run --project baseball-history-web
```

The Aspire AppHost is additive and does not replace the direct `dotnet run`
workflow for the existing web project.

### Migration Runtime Notes

- htmxRazor serves its foundation assets from `/_rhx/`; component CSS imports stay centralized in `baseball-history-web/Pages/Shared/_Layout.cshtml`.
- `rhx-button.css` and `rhx-badge.css` are intentionally retained because `Pages/About.cshtml`, team pages, and leaderboard partials still render those components.
- The app uses two cache layers during navigation: 24-hour `IMemoryCache` entries for shared lookup data and a 3600-second response cache that varies by `HX-Request` so boosted/full-page and non-boosted partial responses do not collide.
- When replacing `lahman.db`, restart each app instance after the file swap so in-memory lookup caches and warmed player-page data rebuild against the new database.
- The Aspire AppHost only orchestrates `baseball-history-web` for local
  development; it does not change the web app's runtime contracts or require
  Aspire-specific code in the web project.

### Database

The application uses
the [Lahman Baseball Database](https://www.seanlahman.com/baseball-archive/statistics/),
a comprehensive database of Major League Baseball statistics from 1871 to
present. The SQLite database file (`lahman.db`) should be placed in the
`baseball-history-web` directory.

## Project Structure

```
baseball-history/
├── baseball-history-aspire/       # Aspire AppHost for local orchestration
├── baseball-history-web/          # Main web application
│   ├── Models/                    # Entity models and DbContext
│   ├── ViewModels/                # View models for pages
│   ├── Pages/                     # Razor Pages
│   │   ├── Players/               # Player browser and modals
│   │   ├── Teams/                 # Team/franchise browser
│   │   ├── Stats/                 # Statistical leaderboards
│   │   ├── HallOfFame/            # Hall of Fame browser
│   │   └── Shared/                # Layouts and components
│   ├── Extensions/                # Helper extensions (htmx, etc.)
│   ├── wwwroot/                   # Static files (CSS, JS)
│   └── Program.cs                 # Application entry point
└── docs/                          # Documentation
```

## Key Features

- **No JavaScript Required**: Uses htmx for dynamic content loading
- **MLB Theming**: Official MLB color scheme with team-specific colors
- **Responsive Design**: Mobile-friendly Bootstrap layout
- **Fast Navigation**: htmx boost for SPA-like navigation
- **Player Modals**: Quick view of player details without page navigation
- **Advanced Filtering**: Filter leaderboards by year, league, and minimums

## License

This project uses the Lahman Baseball Database, which is available under the
Creative Commons Attribution-ShareAlike 3.0 Unported License.
