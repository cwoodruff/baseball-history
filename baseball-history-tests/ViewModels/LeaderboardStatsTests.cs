using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class LeaderboardStatsTests
{
    [Fact]
    public void BattingStats_ContainsAllExpectedStats()
    {
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("hr"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("h"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("r"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("rbi"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("sb"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("avg"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("obp"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("slg"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("ops"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("2b"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("3b"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("tb"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("bb"));
        Assert.True(LeaderboardStats.BattingStats.ContainsKey("g"));
    }

    [Fact]
    public void BattingStats_HasCorrectLabels()
    {
        Assert.Equal("Home Runs", LeaderboardStats.BattingStats["hr"]);
        Assert.Equal("Hits", LeaderboardStats.BattingStats["h"]);
        Assert.Equal("RBI", LeaderboardStats.BattingStats["rbi"]);
        Assert.Equal("Batting Average", LeaderboardStats.BattingStats["avg"]);
        Assert.Equal("OPS", LeaderboardStats.BattingStats["ops"]);
    }

    [Fact]
    public void PitchingStats_ContainsAllExpectedStats()
    {
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("w"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("era"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("so"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("sv"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("cg"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("sho"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("ip"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("whip"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("k9"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("wpct"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("l"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("g"));
        Assert.True(LeaderboardStats.PitchingStats.ContainsKey("gs"));
    }

    [Fact]
    public void PitchingStats_HasCorrectLabels()
    {
        Assert.Equal("Wins", LeaderboardStats.PitchingStats["w"]);
        Assert.Equal("ERA", LeaderboardStats.PitchingStats["era"]);
        Assert.Equal("Strikeouts", LeaderboardStats.PitchingStats["so"]);
        Assert.Equal("Saves", LeaderboardStats.PitchingStats["sv"]);
        Assert.Equal("WHIP", LeaderboardStats.PitchingStats["whip"]);
    }

    [Fact]
    public void BattingStats_CountIsCorrect()
    {
        Assert.Equal(14, LeaderboardStats.BattingStats.Count);
    }

    [Fact]
    public void PitchingStats_CountIsCorrect()
    {
        Assert.Equal(13, LeaderboardStats.PitchingStats.Count);
    }
}

public class LeaderboardViewModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var vm = new LeaderboardViewModel();

        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(100, vm.PageSize);
        Assert.Equal(0, vm.MinimumAtBats);
        Assert.Equal(0, vm.MinimumInningsPitched);
        Assert.False(vm.SingleSeason);
        Assert.NotNull(vm.BattingLeaders);
        Assert.Empty(vm.BattingLeaders);
        Assert.NotNull(vm.PitchingLeaders);
        Assert.Empty(vm.PitchingLeaders);
        Assert.NotNull(vm.AvailableYears);
        Assert.NotNull(vm.AvailableLeagues);
        Assert.NotNull(vm.AvailableStats);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var vm = new LeaderboardViewModel
        {
            Title = "Career Home Run Leaders",
            Type = LeaderboardType.Batting,
            StatColumn = "hr",
            StatLabel = "Home Runs",
            FromYear = 1950,
            ToYear = 2000,
            League = "AL",
            MinimumAtBats = 3000,
            SingleSeason = false,
            CurrentPage = 1,
            TotalPages = 5,
            TotalEntries = 500,
            PageSize = 100
        };

        Assert.Equal("Career Home Run Leaders", vm.Title);
        Assert.Equal(LeaderboardType.Batting, vm.Type);
        Assert.Equal("hr", vm.StatColumn);
        Assert.Equal(1950, vm.FromYear);
        Assert.Equal(2000, vm.ToYear);
        Assert.Equal("AL", vm.League);
        Assert.Equal(3000, vm.MinimumAtBats);
    }
}
