using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class BattingLeaderEntryTests
{
    [Fact]
    public void TotalBases_CalculatesCorrectly()
    {
        var entry = new BattingLeaderEntry
        {
            Hits = 100,
            Doubles = 25,
            Triples = 5,
            HomeRuns = 30
        };
        // TB = H + 2B + 2*3B + 3*HR = 100 + 25 + 10 + 90 = 225
        Assert.Equal(225, entry.TotalBases);
    }

    [Fact]
    public void ExtraBaseHits_CalculatesCorrectly()
    {
        var entry = new BattingLeaderEntry
        {
            Doubles = 25,
            Triples = 5,
            HomeRuns = 30
        };
        Assert.Equal(60, entry.ExtraBaseHits);
    }

    [Theory]
    [InlineData("hr", 30)]
    [InlineData("HR", 30)]
    [InlineData("homeruns", 30)]
    [InlineData("h", 150)]
    [InlineData("hits", 150)]
    [InlineData("r", 80)]
    [InlineData("runs", 80)]
    [InlineData("rbi", 100)]
    [InlineData("sb", 20)]
    [InlineData("stolenbases", 20)]
    [InlineData("2b", 25)]
    [InlineData("doubles", 25)]
    [InlineData("3b", 5)]
    [InlineData("triples", 5)]
    [InlineData("bb", 60)]
    [InlineData("walks", 60)]
    [InlineData("g", 162)]
    [InlineData("games", 162)]
    [InlineData("ab", 550)]
    [InlineData("atbats", 550)]
    public void GetStatValue_ReturnsCorrectValue(string statColumn, int expectedValue)
    {
        var entry = new BattingLeaderEntry
        {
            Games = 162,
            AtBats = 550,
            Runs = 80,
            Hits = 150,
            Doubles = 25,
            Triples = 5,
            HomeRuns = 30,
            Rbi = 100,
            StolenBases = 20,
            Walks = 60
        };

        var result = entry.GetStatValue(statColumn);
        Assert.Equal(expectedValue, Convert.ToInt32(result));
    }

    [Fact]
    public void GetStatValue_WithAverageStats_ReturnsDouble()
    {
        var entry = new BattingLeaderEntry
        {
            Hits = 150,
            AtBats = 500,
            Walks = 50,
            Doubles = 30,
            Triples = 5,
            HomeRuns = 20
        };

        var avg = (double)entry.GetStatValue("avg");
        Assert.Equal(0.3, avg, 3);

        var ops = (double)entry.GetStatValue("ops");
        Assert.True(ops > 0);
    }

    [Fact]
    public void GetStatValue_TotalBases()
    {
        var entry = new BattingLeaderEntry
        {
            Hits = 100,
            Doubles = 20,
            Triples = 5,
            HomeRuns = 25
        };

        // TB = H + 2B + 2*3B + 3*HR = 100 + 20 + 10 + 75 = 205
        var tb = entry.GetStatValue("tb");
        Assert.Equal(205, Convert.ToInt32(tb));
    }

    [Fact]
    public void GetStatValue_UnknownStat_ReturnsHits()
    {
        var entry = new BattingLeaderEntry { Hits = 175 };
        var result = entry.GetStatValue("unknown");
        Assert.Equal(175, Convert.ToInt32(result));
    }

    [Fact]
    public void FormattedStats_FormatCorrectly()
    {
        var entry = new BattingLeaderEntry
        {
            Hits = 300,
            AtBats = 1000,
            Walks = 100,
            Doubles = 50,
            Triples = 10,
            HomeRuns = 40
        };

        Assert.Equal(".300", entry.FormattedAvg);
        Assert.Matches(@"^\.\d{3}$", entry.FormattedObp);
        Assert.Matches(@"^\.\d{3}$", entry.FormattedSlg);
        Assert.Matches(@"^\.\d{3}$", entry.FormattedOps);
    }
}