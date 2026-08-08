using BaseballHistory.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_tests;

public class DatabaseTest
{
    [Fact]
    public void CanConnectAndGetTeam()
    {
        using var context = TestDatabaseFactory.CreateContext();

        var team = context.Teams.FirstOrDefault();

        Assert.NotNull(team);
        Assert.NotNull(team.TeamId);
    }
}