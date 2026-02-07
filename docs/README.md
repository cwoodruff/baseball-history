# Baseball History Web Application

A web application for exploring Major League Baseball history using the Lahman
Baseball Database. Built with ASP.NET Core Razor Pages, Entity Framework Core,
HTMX, and Bootstrap.

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
| Frontend  | HTMX 2.0.4, Bootstrap 5              |
| Hosting   | .NET Aspire (optional)               |

## Documentation

- [Architecture Overview](ARCHITECTURE.md) - System architecture and design
  patterns
- [Database Design](DATABASE.md) - Database schema and Entity Framework
  configuration
- [Frontend Design](FRONTEND.md) - HTMX patterns, Bootstrap theming, and CSS
  architecture
- [Features](FEATURES.md) - Detailed feature documentation
- [API Reference](API.md) - Page models and data flow

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQLite (included with .NET)

### Running the Application

```bash
# Clone the repository
git clone <repository-url>
cd baseball-history

# Run the web application
dotnet run --project baseball-history-web

# Or run with Aspire (includes dashboard)
dotnet run --project baseball-history-aspire/AppHost.cs
```

The application will be available at `https://localhost:5001` or
`http://localhost:5000`.

### Database

The application uses
the [Lahman Baseball Database](https://www.seanlahman.com/baseball-archive/statistics/),
a comprehensive database of Major League Baseball statistics from 1871 to
present. The SQLite database file (`lahman.db`) should be placed in the
`baseball-history-web` directory.

## Project Structure

```
baseball-history/
├── baseball-history-web/          # Main web application
│   ├── Models/                    # Entity models and DbContext
│   ├── ViewModels/                # View models for pages
│   ├── Pages/                     # Razor Pages
│   │   ├── Players/               # Player browser and modals
│   │   ├── Teams/                 # Team/franchise browser
│   │   ├── Stats/                 # Statistical leaderboards
│   │   ├── HallOfFame/            # Hall of Fame browser
│   │   └── Shared/                # Layouts and components
│   ├── Extensions/                # Helper extensions (HTMX, etc.)
│   ├── wwwroot/                   # Static files (CSS, JS)
│   └── Program.cs                 # Application entry point
├── baseball-history-aspire/       # .NET Aspire orchestration
└── docs/                          # Documentation
```

## Key Features

- **No JavaScript Required**: Uses HTMX for dynamic content loading
- **MLB Theming**: Official MLB color scheme with team-specific colors
- **Responsive Design**: Mobile-friendly Bootstrap layout
- **Fast Navigation**: HTMX boost for SPA-like navigation
- **Player Modals**: Quick view of player details without page navigation
- **Advanced Filtering**: Filter leaderboards by year, league, and minimums

## License

This project uses the Lahman Baseball Database, which is available under the
Creative Commons Attribution-ShareAlike 3.0 Unported License.
