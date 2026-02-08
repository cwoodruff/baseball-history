using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class CareerBattingStatsTests
{
    [Fact]
    public void BattingAverage_WithHits_CalculatesCorrectly()
    {
        var stats = new CareerBattingStats { Hits = 200, AtBats = 500 };
        Assert.Equal(0.4, stats.BattingAverage, 3);
    }

    [Fact]
    public void BattingAverage_WithZeroAtBats_ReturnsZero()
    {
        var stats = new CareerBattingStats { Hits = 0, AtBats = 0 };
        Assert.Equal(0, stats.BattingAverage);
    }

    [Fact]
    public void OnBasePercentage_CalculatesCorrectly()
    {
        var stats = new CareerBattingStats { Hits = 150, AtBats = 500, Walks = 50 };
        // OBP = (H + BB) / (AB + BB) = (150 + 50) / (500 + 50) = 200/550
        Assert.Equal(200.0 / 550.0, stats.OnBasePercentage, 4);
    }

    [Fact]
    public void OnBasePercentage_WithZeroDenominator_ReturnsZero()
    {
        var stats = new CareerBattingStats { Hits = 0, AtBats = 0, Walks = 0 };
        Assert.Equal(0, stats.OnBasePercentage);
    }

    [Fact]
    public void SluggingPercentage_CalculatesCorrectly()
    {
        var stats = new CareerBattingStats
        {
            Hits = 100,
            Doubles = 20,
            Triples = 5,
            HomeRuns = 25,
            AtBats = 400
        };
        // TB = H + 2B + 2*3B + 3*HR = 100 + 20 + 10 + 75 = 205
        // SLG = TB / AB = 205 / 400 = 0.5125
        Assert.Equal(205.0 / 400.0, stats.SluggingPercentage, 4);
    }

    [Fact]
    public void SluggingPercentage_WithZeroAtBats_ReturnsZero()
    {
        var stats = new CareerBattingStats { Hits = 0, AtBats = 0 };
        Assert.Equal(0, stats.SluggingPercentage);
    }

    [Fact]
    public void Ops_CalculatesCorrectly()
    {
        var stats = new CareerBattingStats
        {
            Hits = 150,
            Doubles = 30,
            Triples = 5,
            HomeRuns = 20,
            AtBats = 500,
            Walks = 50
        };
        var expectedOps = stats.OnBasePercentage + stats.SluggingPercentage;
        Assert.Equal(expectedOps, stats.Ops, 4);
    }

    [Fact]
    public void FormattedAvg_FormatsCorrectly()
    {
        var stats = new CareerBattingStats { Hits = 300, AtBats = 1000 };
        Assert.Equal(".300", stats.FormattedAvg);
    }

    [Fact]
    public void FormattedAvg_WithZero_ReturnsZeroFormat()
    {
        var stats = new CareerBattingStats { Hits = 0, AtBats = 0 };
        Assert.Equal(".000", stats.FormattedAvg);
    }

    [Fact]
    public void FormattedAvg_RoundsCorrectly()
    {
        var stats = new CareerBattingStats { Hits = 333, AtBats = 1000 };
        Assert.Equal(".333", stats.FormattedAvg);
    }

    [Fact]
    public void FormattedObp_FormatsCorrectly()
    {
        var stats = new CareerBattingStats { Hits = 200, AtBats = 400, Walks = 100 };
        // OBP = 300/500 = 0.600
        Assert.Equal(".600", stats.FormattedObp);
    }

    [Fact]
    public void FormattedSlg_FormatsCorrectly()
    {
        var stats = new CareerBattingStats
        {
            Hits = 200,
            Doubles = 40,
            Triples = 10,
            HomeRuns = 50,
            AtBats = 500
        };
        // TB = 200 + 40 + 20 + 150 = 410, SLG = 410/500 = 0.820
        Assert.Equal(".820", stats.FormattedSlg);
    }

    [Fact]
    public void FormattedOps_FormatsCorrectly()
    {
        var stats = new CareerBattingStats
        {
            Hits = 180,
            Doubles = 30,
            Triples = 5,
            HomeRuns = 35,
            AtBats = 500,
            Walks = 100
        };
        // OPS can exceed 1.000, so it may have a leading digit
        // Should format OPS to 3 decimal places
        Assert.Matches(@"^\d*\.\d{3}$", stats.FormattedOps);
    }
}