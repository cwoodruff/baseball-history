# Baseball History Web App

A web application for exploring Major League Baseball history using the Lahman
Baseball Database. Baseball data and statistics from 1871 to 2025. Built with ASP.NET Core Razor Pages, Entity Framework Core,
htmx, and Bootstrap.

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
| Frontend  | htmx 2.0.4, Bootstrap 5              |

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

### Running the Application

```bash
# Clone the repository
git clone <repository-url>
cd baseball-history

# Run the web application
dotnet run --project baseball-history-web
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
