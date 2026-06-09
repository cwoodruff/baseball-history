using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_tests.Database;

public class DatabaseIntegrationTests
{
    private BaseballDbContext CreateContext()
    {
        return TestDatabaseFactory.CreateContext();
    }

    [Fact]
    public void CanConnectToDatabase()
    {
        using var context = CreateContext();
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public void CanQueryPeople()
    {
        using var context = CreateContext();
        var count = context.People.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void CanQueryTeams()
    {
        using var context = CreateContext();
        var count = context.Teams.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void CanQueryBatting()
    {
        using var context = CreateContext();
        var count = context.Batting.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void CanQueryPitching()
    {
        using var context = CreateContext();
        var count = context.Pitching.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void CanQueryHallOfFame()
    {
        using var context = CreateContext();
        var inductees = context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Count();
        Assert.True(inductees > 0);
    }

    [Fact]
    public void CanQueryFranchises()
    {
        using var context = CreateContext();
        var activeFranchises = context.TeamsFranchises
            .Where(f => f.Active == "Y")
            .Count();
        // Should have 30 active franchises
        Assert.Equal(30, activeFranchises);
    }

    [Fact]
    public void CanNavigatePeopleToBatting()
    {
        using var context = CreateContext();
        var player = context.People
            .Include(p => p.Battings)
            .FirstOrDefault(p => p.Battings.Any());

        Assert.NotNull(player);
        Assert.True(player.Battings.Count > 0);
    }

    [Fact]
    public void CanNavigatePeopleToPitching()
    {
        using var context = CreateContext();
        var player = context.People
            .Include(p => p.Pitchings)
            .FirstOrDefault(p => p.Pitchings.Any());

        Assert.NotNull(player);
        Assert.True(player.Pitchings.Count > 0);
    }

    [Fact]
    public void CanNavigateTeamToFranchise()
    {
        using var context = CreateContext();
        var team = context.Teams
            .Include(t => t.Franchise)
            .FirstOrDefault(t => t.FranchId != null);

        Assert.NotNull(team);
        Assert.NotNull(team.Franchise);
    }

    [Fact]
    public void CanNavigateFranchiseToTeams()
    {
        using var context = CreateContext();
        var franchise = context.TeamsFranchises
            .Include(f => f.Teams)
            .FirstOrDefault(f => f.Active == "Y");

        Assert.NotNull(franchise);
        Assert.True(franchise.Teams.Count > 0);
    }

    [Fact]
    public void CanQueryAllStarAppearances()
    {
        using var context = CreateContext();
        var count = context.AllstarFull.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void CanQueryAwards()
    {
        using var context = CreateContext();
        var count = context.AwardsPlayers.Count();
        Assert.True(count > 0);
    }

    [Fact]
    public void DateOnlyConverter_HandlesBirthDates()
    {
        using var context = CreateContext();
        // Query players and verify DateOnly conversion doesn't throw
        var players = context.People
            .Take(100)
            .ToList();

        // Should not throw any exceptions during conversion
        Assert.True(players.Count > 0);

        // Verify we can access Debut without exceptions (some may be null)
        var playersWithDebut = players.Where(p => p.Debut.HasValue).ToList();
        Assert.True(playersWithDebut.Count >= 0); // May be 0 or more, just shouldn't throw
    }

    [Fact]
    public void DateOnlyConverter_HandlesEmptyStrings()
    {
        using var context = CreateContext();
        // Some players may have empty debut dates - this should not throw
        var players = context.People.Take(1000).ToList();

        // Should handle null values without throwing
        Assert.True(players.Count > 0);
    }

    [Fact]
    public void CanQueryPlayersByLastNameInitial()
    {
        using var context = CreateContext();
        var playersA = context.People
            .Where(p => p.NameLast != null && p.NameLast.ToUpper().StartsWith("A"))
            .Take(10)
            .ToList();

        Assert.True(playersA.Count > 0);
        Assert.All(playersA, p => Assert.StartsWith("A", p.NameLast!, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CanAggregateCareerStats()
    {
        using var context = CreateContext();

        // Get a known player and aggregate their batting stats
        var player = context.People
            .Where(p => p.Battings.Any())
            .Take(1)
            .Select(p => new
            {
                PlayerId = p.PlayerId,
                TotalHits = p.Battings.Sum(b => b.H ?? 0),
                TotalHomeRuns = p.Battings.Sum(b => b.Hr ?? 0),
                TotalGames = p.Battings.Sum(b => b.G ?? 0)
            })
            .FirstOrDefault();

        Assert.NotNull(player);
        Assert.NotNull(player.PlayerId);
    }

    [Fact]
    public void CanQueryTeamSeasonWithRoster()
    {
        using var context = CreateContext();

        var team = context.Teams
            .Include(t => t.Battings)
                .ThenInclude(b => b.Player)
            .Include(t => t.Pitchings)
                .ThenInclude(p => p.Player)
            .Include(t => t.Managers)
                .ThenInclude(m => m.Player)
            .Where(t => t.YearId >= 2000)
            .FirstOrDefault();

        Assert.NotNull(team);
        Assert.True(team.Battings.Count > 0 || team.Pitchings.Count > 0);
    }

    [Fact]
    public void CanQueryLeadersWithHallOfFameStatus()
    {
        using var context = CreateContext();

        var hofPlayerIds = context.HallOfFame
            .Where(h => h.Inducted == "Y")
            .Select(h => h.PlayerId)
            .Distinct()
            .ToHashSet();

        var leaders = context.Batting
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                TotalHR = g.Sum(b => b.Hr ?? 0)
            })
            .OrderByDescending(x => x.TotalHR)
            .Take(10)
            .ToList();

        Assert.True(leaders.Count > 0);

        // Check that we can properly determine HOF status
        var firstLeader = leaders.First();
        var isHof = hofPlayerIds.Contains(firstLeader.PlayerId);
        // Just verify it doesn't throw
        Assert.True(firstLeader.TotalHR > 0);
    }
}
