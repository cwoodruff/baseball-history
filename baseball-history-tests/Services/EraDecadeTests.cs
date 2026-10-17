using BaseballHistory.Data.Reference;

namespace baseball_history_tests.Services;

public class EraDecadeTests
{
    [Theory]
    [InlineData(1927, 1920)]
    [InlineData(1870, 1870)]
    [InlineData(2030, 2030)]
    public void From_RoundsDownToDecade(int era, int expected) =>
        Assert.Equal(expected, EraDecade.From(era));

    [Theory]
    [InlineData(null)]
    [InlineData(1869)]
    [InlineData(2031)]
    public void From_OutOfRange_ReturnsNull(int? era) =>
        Assert.Null(EraDecade.From(era));

    [Fact]
    public void Overlaps_ChecksSpanAgainstDecade()
    {
        Assert.True(EraDecade.Overlaps(1920, 1915, 1921));
        Assert.True(EraDecade.Overlaps(1920, 1929, 1940));
        Assert.False(EraDecade.Overlaps(1920, 1930, 1940));
        Assert.False(EraDecade.Overlaps(1920, 1900, 1919));
        Assert.False(EraDecade.Overlaps(1920, null, 1925));
    }
}
