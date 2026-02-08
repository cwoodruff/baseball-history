using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class CareerPitchingStatsTests
{
    [Fact]
    public void Era_CalculatesCorrectly()
    {
        var stats = new CareerPitchingStats { EarnedRuns = 90, InningsPitched = 200 };
        // ERA = (ER * 9) / IP = (90 * 9) / 200 = 810 / 200 = 4.05
        Assert.Equal(4.05, stats.Era, 2);
    }

    [Fact]
    public void Era_WithZeroInnings_ReturnsZero()
    {
        var stats = new CareerPitchingStats { EarnedRuns = 10, InningsPitched = 0 };
        Assert.Equal(0, stats.Era);
    }

    [Fact]
    public void Whip_CalculatesCorrectly()
    {
        var stats = new CareerPitchingStats { Walks = 50, Hits = 150, InningsPitched = 200 };
        // WHIP = (BB + H) / IP = (50 + 150) / 200 = 200 / 200 = 1.0
        Assert.Equal(1.0, stats.Whip, 2);
    }

    [Fact]
    public void Whip_WithZeroInnings_ReturnsZero()
    {
        var stats = new CareerPitchingStats { Walks = 10, Hits = 20, InningsPitched = 0 };
        Assert.Equal(0, stats.Whip);
    }

    [Fact]
    public void StrikeoutsPer9_CalculatesCorrectly()
    {
        var stats = new CareerPitchingStats { Strikeouts = 200, InningsPitched = 200 };
        // K/9 = (SO * 9) / IP = (200 * 9) / 200 = 9.0
        Assert.Equal(9.0, stats.StrikeoutsPer9, 1);
    }

    [Fact]
    public void StrikeoutsPer9_WithZeroInnings_ReturnsZero()
    {
        var stats = new CareerPitchingStats { Strikeouts = 100, InningsPitched = 0 };
        Assert.Equal(0, stats.StrikeoutsPer9);
    }

    [Fact]
    public void FormattedEra_FormatsToTwoDecimals()
    {
        var stats = new CareerPitchingStats { EarnedRuns = 90, InningsPitched = 200 };
        Assert.Equal("4.05", stats.FormattedEra);
    }

    [Fact]
    public void FormattedEra_WithZero_ReturnsZeroFormat()
    {
        var stats = new CareerPitchingStats { EarnedRuns = 0, InningsPitched = 100 };
        Assert.Equal("0.00", stats.FormattedEra);
    }

    [Fact]
    public void FormattedWhip_FormatsToTwoDecimals()
    {
        var stats = new CareerPitchingStats { Walks = 60, Hits = 180, InningsPitched = 200 };
        // WHIP = 240/200 = 1.20
        Assert.Equal("1.20", stats.FormattedWhip);
    }

    [Fact]
    public void WinLossRecord_FormatsCorrectly()
    {
        var stats = new CareerPitchingStats { Wins = 20, Losses = 10 };
        Assert.Equal("20-10", stats.WinLossRecord);
    }

    [Fact]
    public void WinLossRecord_WithZeros()
    {
        var stats = new CareerPitchingStats { Wins = 0, Losses = 0 };
        Assert.Equal("0-0", stats.WinLossRecord);
    }

    [Theory]
    [InlineData(100, 200, 4.50)]
    [InlineData(50, 225, 2.00)]
    [InlineData(27, 162, 1.50)]
    [InlineData(0, 100, 0.00)]
    public void Era_VariousScenarios(int earnedRuns, double inningsPitched, double expectedEra)
    {
        var stats = new CareerPitchingStats { EarnedRuns = earnedRuns, InningsPitched = inningsPitched };
        Assert.Equal(expectedEra, stats.Era, 2);
    }
}