using baseball_history_web.Services;
using Microsoft.EntityFrameworkCore;

namespace baseball_history_tests.Services;

public class PlayerDetailServiceTests
{
    [Fact]
    public async Task GetPlayerDetailAsync_ForWellKnownPlayerWithFullCareerData_ReturnsRichDetail()
    {
        using var context = TestDatabaseFactory.CreateContext();
        var service = new PlayerDetailService(context);

        var player = await service.GetPlayerDetailAsync("jeterde01");

        Assert.NotNull(player);
        Assert.Equal("jeterde01", player.PlayerId);
        Assert.Equal("Derek Jeter", player.FullName);
        Assert.True(player.IsInHallOfFame);
        Assert.NotNull(player.BattingStats);
        Assert.Null(player.PitchingStats);
        Assert.True(player.CareerOpsIndex.HasValue);
        Assert.NotEmpty(player.AdvancedBattingSeasons);
        Assert.NotEmpty(player.BattingSeasons);
        Assert.NotEmpty(player.FieldingSeasons);
        Assert.NotEmpty(player.Teams);
        Assert.NotEmpty(player.Awards);
        Assert.NotEmpty(player.AllStarAppearances);
        Assert.NotEmpty(player.PostseasonBattingSeasons);
        Assert.True(player.HasPostseason);
        Assert.Equal(player.Awards.OrderByDescending(a => a.Year).Select(a => a.Year), player.Awards.Select(a => a.Year));
        Assert.All(player.PostseasonBattingSeasons, season => Assert.False(string.IsNullOrWhiteSpace(season.RoundName)));
    }

    [Fact]
    public async Task GetPlayerDetailAsync_ForPartialRecord_PreservesPartialFlagAndIdentity()
    {
        await using var lookupContext = TestDatabaseFactory.CreateContext();
        var partialPlayer = await lookupContext.People
            .Where(p => p.NameLast != null &&
                        (p.NameFirst == null ||
                         p.NameFirst == "" ||
                         EF.Functions.Like(p.NameFirst, "_") ||
                         EF.Functions.Like(p.NameFirst, "_.")))
            .OrderBy(p => p.NameLast)
            .Select(p => new { p.PlayerId, p.NameFirst, p.NameLast })
            .FirstAsync();

        using var context = TestDatabaseFactory.CreateContext();
        var service = new PlayerDetailService(context);

        var player = await service.GetPlayerDetailAsync(partialPlayer.PlayerId);

        Assert.NotNull(player);
        Assert.Equal(partialPlayer.PlayerId, player.PlayerId);
        Assert.True(player.IsPartialRecord);
        Assert.Equal($"{partialPlayer.NameFirst} {partialPlayer.NameLast}".Trim(), player.FullName);
        Assert.Equal(partialPlayer.NameLast, player.LastName);
    }

    [Fact]
    public async Task GetPlayerDetailAsync_AggregatesCareerTotalsFromBattingAndPitchingRecords()
    {
        await using var expectedContext = TestDatabaseFactory.CreateContext();
        var expectedBatting = await expectedContext.Batting
            .Where(b => b.PlayerId == "ruthba01")
            .GroupBy(b => b.PlayerId)
            .Select(g => new
            {
                Games = g.Sum(b => b.G ?? 0),
                AtBats = g.Sum(b => b.Ab ?? 0),
                Hits = g.Sum(b => b.H ?? 0),
                HomeRuns = g.Sum(b => b.Hr ?? 0),
                Walks = g.Sum(b => b.Bb ?? 0)
            })
            .SingleAsync();

        var expectedPitching = await expectedContext.Pitching
            .Where(p => p.PlayerId == "ruthba01")
            .GroupBy(p => p.PlayerId)
            .Select(g => new
            {
                Games = g.Sum(p => p.G ?? 0),
                Wins = g.Sum(p => p.W ?? 0),
                Losses = g.Sum(p => p.L ?? 0),
                Saves = g.Sum(p => p.Sv ?? 0),
                InningsPitched = g.Sum(p => p.Ipouts ?? 0) / 3.0
            })
            .SingleAsync();

        using var context = TestDatabaseFactory.CreateContext();
        var service = new PlayerDetailService(context);

        var player = await service.GetPlayerDetailAsync("ruthba01");

        Assert.NotNull(player);
        Assert.NotNull(player.BattingStats);
        Assert.NotNull(player.PitchingStats);
        Assert.Equal(714, player.BattingStats.HomeRuns);
        Assert.Equal(expectedBatting.Games, player.BattingStats.Games);
        Assert.Equal(expectedBatting.AtBats, player.BattingStats.AtBats);
        Assert.Equal(expectedBatting.Hits, player.BattingStats.Hits);
        Assert.Equal(expectedBatting.HomeRuns, player.BattingStats.HomeRuns);
        Assert.Equal(expectedBatting.Walks, player.BattingStats.Walks);
        Assert.Equal(expectedPitching.Games, player.PitchingStats.Games);
        Assert.Equal(expectedPitching.Wins, player.PitchingStats.Wins);
        Assert.Equal(expectedPitching.Losses, player.PitchingStats.Losses);
        Assert.Equal(expectedPitching.Saves, player.PitchingStats.Saves);
        Assert.Equal(expectedPitching.InningsPitched, player.PitchingStats.InningsPitched);
        Assert.True(player.IsTwoWayPlayer);
        Assert.Contains(player.Teams, team => team.TeamId == "BOS");
        Assert.Contains(player.Teams, team => team.TeamId == "NYA");
    }
}
