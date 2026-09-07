using BaseballHistory.Data.Querying;

namespace baseball_history_tests.Data;

public class LeaderboardStatCatalogTests
{
    [Fact]
    public void GetBattingStat_WithAlias_IsCaseInsensitiveAndReturnsCanonicalDefinition()
    {
        var byAlias = LeaderboardStatCatalog.GetBattingStat("HOMERUNS");
        var byKey = LeaderboardStatCatalog.GetBattingStat("hr");

        Assert.NotNull(byAlias);
        Assert.Same(byKey, byAlias);
        Assert.Equal("hr", byAlias.Key);
        Assert.Equal("Home runs", byAlias.Label);
        Assert.Equal("descending", byAlias.SortDirection);
        Assert.False(byAlias.IsRateStat);
    }

    [Fact]
    public void GetBattingStat_ForAverage_UsesDescendingRateStatSemantics()
    {
        var stat = LeaderboardStatCatalog.GetBattingStat("battingaverage");

        Assert.NotNull(stat);
        Assert.Equal("avg", stat.Key);
        Assert.Equal("descending", stat.SortDirection);
        Assert.True(stat.IsRateStat);
    }

    [Fact]
    public void GetPitchingStat_ForEra_UsesAscendingRateStatSemantics()
    {
        var stat = LeaderboardStatCatalog.GetPitchingStat("ERA");

        Assert.NotNull(stat);
        Assert.Equal("era", stat.Key);
        Assert.Equal("ascending", stat.SortDirection);
        Assert.True(stat.IsRateStat);
    }

    [Fact]
    public void GetPitchingStat_WithAlias_ReturnsCanonicalDefinition()
    {
        var byAlias = LeaderboardStatCatalog.GetPitchingStat("wins");
        var byKey = LeaderboardStatCatalog.GetPitchingStat("w");

        Assert.NotNull(byAlias);
        Assert.Same(byKey, byAlias);
        Assert.Equal("Wins", byAlias.Label);
    }

    [Fact]
    public void GetBattingStat_WithUnknownKey_ReturnsNull()
    {
        Assert.Null(LeaderboardStatCatalog.GetBattingStat("sluggingplus"));
    }
}
