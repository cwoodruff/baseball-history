using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class FranchiseDetailViewModelTests
{
    [Fact]
    public void TeamSeasonSummary_ComputesWinningPercentageAndRecord()
    {
        var summary = new TeamSeasonSummary
        {
            Wins = 98,
            Losses = 64
        };

        Assert.Equal(98d / 162d, summary.WinningPercentage, 6);
        Assert.Equal(".605", summary.FormattedWinPct);
        Assert.Equal("98-64", summary.Record);
    }

    [Fact]
    public void TeamSeasonSummary_WithNoGames_ReturnsZeroWinningPercentage()
    {
        var summary = new TeamSeasonSummary();

        Assert.Equal(0, summary.WinningPercentage);
        Assert.Equal(".000", summary.FormattedWinPct);
    }
}
