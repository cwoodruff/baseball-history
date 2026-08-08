using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_tests.Database;

public class PostgreSqlModelTests
{
    private const string PlaceholderConnectionString =
        "Host=localhost;Port=5432;Database=lahman;Username=postgres;Password=placeholder;SSL Mode=Disable";

    private static BaseballDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BaseballDbContext>()
            .UseNpgsql(PlaceholderConnectionString)
            .Options;

        return new BaseballDbContext(options);
    }

    [Fact]
    public void Model_NormalizesLegacySqliteMetadata_ForPostgreSql()
    {
        using var context = CreateContext();

        var people = context.Model.FindEntityType(typeof(People))!;
        var teams = context.Model.FindEntityType(typeof(Teams))!;
        var collegePlaying = context.Model.FindEntityType(typeof(CollegePlaying))!;

        var birthYear = people.FindProperty(nameof(People.BirthYear))!;
        var era = teams.FindProperty(nameof(Teams.Era))!;
        var yearId = collegePlaying.FindProperty(nameof(CollegePlaying.YearId))!;

        Assert.Null(birthYear.FindAnnotation("Relational:ColumnType"));
        Assert.Null(birthYear.FindAnnotation("Relational:Collation"));
        Assert.NotNull(birthYear.GetValueConverter());

        Assert.Null(era.FindAnnotation("Relational:ColumnType"));
        Assert.Null(era.FindAnnotation("Relational:Collation"));
        Assert.NotNull(era.GetValueConverter());

        Assert.NotNull(yearId.GetValueConverter());
    }

    [Fact]
    public void RepresentativeQueries_Translate_ForNpgsql()
    {
        using var context = CreateContext();

        var playerSql = context.People
            .Where(p => p.BirthYear == "1935")
            .Select(p => new { p.PlayerId, p.BirthYear, p.BirthMonth, p.BirthDay })
            .Take(5)
            .ToQueryString();

        var teamSql = context.Teams
            .Where(t => t.Attendance != null && t.Era != null)
            .Select(t => new { t.TeamId, t.Era, t.Attendance })
            .Take(5)
            .ToQueryString();

        var fieldingSql = context.Fielding
            .Where(f => f.A != null && f.Zr != null)
            .Select(f => new { f.PlayerId, f.A, f.Zr })
            .Take(5)
            .ToQueryString();

        var collegeSql = context.CollegePlaying
            .Where(c => c.YearId == "1950")
            .Select(c => new { c.PlayerId, c.SchoolId, c.YearId })
            .Take(5)
            .ToQueryString();

        Assert.Contains("\"People\"", playerSql);
        Assert.Contains("\"Teams\"", teamSql);
        Assert.Contains("\"Fielding\"", fieldingSql);
        Assert.Contains("\"CollegePlaying\"", collegeSql);
    }
}
