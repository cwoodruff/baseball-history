using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class PostseasonRecordTests
{
    [Fact]
    public void BattingRecord_ComputesAverage()
    {
        var record = new PostseasonBattingRecord
        {
            Year = 1956,
            Round = "WS",
            TeamId = "NYA",
            LgId = "AL",
            AtBats = 25,
            Hits = 9
        };

        Assert.Equal(0.360, record.BattingAverage, 3);
        Assert.Equal(".360", record.FormattedAvg);
    }

    [Fact]
    public void BattingRecord_WithNoAtBats_AverageIsZero()
    {
        var record = new PostseasonBattingRecord
        {
            Year = 1956,
            Round = "WS",
            TeamId = "NYA",
            LgId = "AL"
        };

        Assert.Equal(0, record.BattingAverage);
    }

    [Fact]
    public void PitchingRecord_ComputesEra()
    {
        var record = new PostseasonPitchingRecord
        {
            Year = 1999,
            Round = "WS",
            TeamId = "NYA",
            LgId = "AL",
            InningsPitched = 12,
            EarnedRuns = 1
        };

        Assert.Equal(0.75, record.Era, 2);
        Assert.Equal("0.75", record.FormattedEra);
    }

    [Fact]
    public void PitchingRecord_WithNoInnings_EraIsZeroNotNaN()
    {
        var record = new PostseasonPitchingRecord
        {
            Year = 1999,
            Round = "WS",
            TeamId = "NYA",
            LgId = "AL",
            EarnedRuns = 3
        };

        Assert.Equal(0, record.Era);
    }

    [Theory]
    [InlineData("WS", "World Series")]
    [InlineData("ALCS", "AL Championship Series")]
    [InlineData("ALDS1", "AL Division Series")]
    [InlineData("NLDS2", "NL Division Series")]
    [InlineData("ALWC", "AL Wild Card")]
    [InlineData("NLWC2", "NL Wild Card")]
    [InlineData("AWDIV", "AL Division Series")]
    [InlineData("CS", "CS")] // 19th-century round with no friendly name falls through
    public void RoundDisplayName_MapsNumberedAndPlainRounds(string round, string expected)
    {
        Assert.Equal(expected, PostseasonViewModel.RoundDisplayName(round));
    }

    [Fact]
    public void RoundChronologicalRank_OrdersWildCardBeforeDivisionBeforeChampionshipBeforeWorldSeries()
    {
        var rounds = new[] { "WS", "ALDS1", "ALWC", "ALCS" };

        var sorted = rounds.OrderBy(PostseasonViewModel.RoundChronologicalRank).ToArray();

        Assert.Equal(new[] { "ALWC", "ALDS1", "ALCS", "WS" }, sorted);
    }

    [Fact]
    public void RoundChronologicalRank_UnknownRoundSortsLast()
    {
        Assert.True(PostseasonViewModel.RoundChronologicalRank("XX") >
                    PostseasonViewModel.RoundChronologicalRank("WS"));
    }

    [Fact]
    public void PostseasonBattingTotals_AggregatesAcrossRounds()
    {
        var vm = new PlayerDetailViewModel
        {
            PlayerId = "test",
            PostseasonBattingSeasons =
            [
                new PostseasonBattingRecord { Year = 1996, Round = "ALDS1", TeamId = "NYA", LgId = "AL", Games = 4, AtBats = 15, Hits = 6, HomeRuns = 1, Rbi = 3 },
                new PostseasonBattingRecord { Year = 1996, Round = "WS", TeamId = "NYA", LgId = "AL", Games = 6, AtBats = 20, Hits = 5, HomeRuns = 0, Rbi = 2 }
            ]
        };

        var totals = vm.PostseasonBattingTotals;

        Assert.NotNull(totals);
        Assert.Equal(10, totals.Games);
        Assert.Equal(35, totals.AtBats);
        Assert.Equal(11, totals.Hits);
        Assert.Equal(1, totals.HomeRuns);
        Assert.Equal(5, totals.Rbi);
        Assert.Equal(".314", totals.FormattedAvg);
    }

    [Fact]
    public void PostseasonTotals_WithNoData_AreNull()
    {
        var vm = new PlayerDetailViewModel { PlayerId = "test" };

        Assert.Null(vm.PostseasonBattingTotals);
        Assert.Null(vm.PostseasonPitchingTotals);
        Assert.False(vm.HasPostseason);
    }

    [Fact]
    public void PostseasonPitchingTotals_AggregatesInningsAndEra()
    {
        var vm = new PlayerDetailViewModel
        {
            PlayerId = "test",
            PostseasonPitchingSeasons =
            [
                new PostseasonPitchingRecord { Year = 1999, Round = "WS", TeamId = "NYA", LgId = "AL", Games = 3, Wins = 1, Saves = 2, InningsPitched = 4.2 + 0.1, EarnedRuns = 0, Strikeouts = 3 },
                new PostseasonPitchingRecord { Year = 2000, Round = "WS", TeamId = "NYA", LgId = "AL", Games = 4, Wins = 0, Saves = 2, InningsPitched = 6, EarnedRuns = 2, Strikeouts = 4 }
            ]
        };

        var totals = vm.PostseasonPitchingTotals;

        Assert.NotNull(totals);
        Assert.Equal(7, totals.Games);
        Assert.Equal(4, totals.Saves);
        Assert.Equal(7, totals.Strikeouts);
        Assert.True(vm.HasPostseason);
    }
}
