using baseball_history_web.Services;
using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class CareerFieldingRecordTests
{
    [Fact]
    public void FieldingPercentage_ComputesFromChances()
    {
        var record = new CareerFieldingRecord
        {
            Position = "SS",
            Putouts = 4249,
            Assists = 8375,
            Errors = 281
        };

        Assert.Equal(0.978, record.FieldingPercentage, 3);
        Assert.Equal(".978", record.FormattedPct);
    }

    [Fact]
    public void FieldingPercentage_WithNoChances_IsZeroNotNaN()
    {
        var record = new CareerFieldingRecord { Position = "DH" };

        Assert.Equal(0, record.FieldingPercentage);
        Assert.Equal(".000", record.FormattedPct);
    }

    [Fact]
    public void SeasonRecord_FieldingPercentage_WithNoChances_IsZeroNotNaN()
    {
        var record = new SeasonFieldingRecord
        {
            Year = 1901,
            TeamId = "BOS",
            LgId = "AL",
            Position = "OF"
        };

        Assert.Equal(0, record.FieldingPercentage);
        Assert.Equal(".000", record.FormattedPct);
    }

    [Fact]
    public void FieldingByPosition_AggregatesAndOrdersByGames()
    {
        var vm = new PlayerDetailViewModel
        {
            PlayerId = "test",
            FieldingSeasons =
            [
                new SeasonFieldingRecord { Year = 1980, TeamId = "SLN", LgId = "NL", Position = "SS", Games = 150, Putouts = 250, Assists = 500, Errors = 20, DoublePlays = 100 },
                new SeasonFieldingRecord { Year = 1981, TeamId = "SLN", LgId = "NL", Position = "SS", Games = 100, Putouts = 180, Assists = 340, Errors = 10, DoublePlays = 60 },
                new SeasonFieldingRecord { Year = 1981, TeamId = "SLN", LgId = "NL", Position = "2B", Games = 5, Putouts = 10, Assists = 12, Errors = 1, DoublePlays = 3 }
            ]
        };

        var byPosition = vm.FieldingByPosition;

        Assert.Equal(2, byPosition.Count);
        Assert.Equal("SS", byPosition[0].Position);
        Assert.Equal(250, byPosition[0].Games);
        Assert.Equal(430, byPosition[0].Putouts);
        Assert.Equal(840, byPosition[0].Assists);
        Assert.Equal(30, byPosition[0].Errors);
        Assert.Equal(160, byPosition[0].DoublePlays);
        Assert.Equal("2B", byPosition[1].Position);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    [InlineData("abc", 0)]
    [InlineData("0", 0)]
    public void ParseIntOrZero_HandlesLahmanStringColumns(string? value, int expected)
    {
        Assert.Equal(expected, LahmanNumbers.ParseIntOrZero(value));
    }
}
