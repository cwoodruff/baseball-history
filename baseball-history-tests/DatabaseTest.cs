using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_tests;

public class DatabaseTest
{
    private static string GetDatabasePath()
    {
        var testDir = AppContext.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
        return Path.Combine(solutionDir, "baseball-history-web", "lahman.db");
    }

    [Fact]
    public void CanConnectAndGetTeam()
    {
        var dbPath = GetDatabasePath();
        var options = new DbContextOptionsBuilder<BaseballDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var context = new BaseballDbContext(options);

        var team = context.Teams.FirstOrDefault();

        Assert.NotNull(team);
        Assert.NotNull(team.TeamId);
    }
}