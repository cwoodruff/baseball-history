using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class AdvancedSeasonRecordTests
{
    [Fact]
    public void AdvancedBattingSeason_FormatsRates()
    {
        var season = new AdvancedBattingSeason
        {
            Year = 1921,
            Pa = 693,
            Iso = 0.468m,
            Babip = 0.372m,
            BbPct = 20.9m,
            KPct = 11.7m,
            OpsIndex = 224m,
            HrPer162 = 61.8m,
            Qualified = true
        };

        Assert.Equal(".468", season.FormattedIso);
        Assert.Equal(".372", season.FormattedBabip);
        Assert.Equal("20.9", season.FormattedBbPct);
        Assert.Equal("11.7", season.FormattedKPct);
        Assert.Equal("224", season.FormattedOpsIndex);
        Assert.Equal("61.8", season.FormattedHrPer162);
    }

    [Fact]
    public void AdvancedBattingSeason_NullStats_RenderAsDash()
    {
        // Unrecorded-era SO (pre-1913) and missing league context must show a
        // dash, never a fabricated zero.
        var season = new AdvancedBattingSeason { Year = 1884, Pa = 400 };

        Assert.Equal("—", season.FormattedBabip);
        Assert.Equal("—", season.FormattedKPct);
        Assert.Equal("—", season.FormattedOpsIndex);
        Assert.Equal("—", season.FormattedHrPer162);
    }

    [Fact]
    public void AdvancedPitchingSeason_FormatsRates()
    {
        var season = new AdvancedPitchingSeason
        {
            Year = 1999,
            Ip = 213.1m,
            K9 = 13.20m,
            Bb9 = 1.57m,
            Whip = 0.923m,
            Qualified = true
        };

        Assert.Equal("213.1", season.FormattedIp);
        Assert.Equal("13.20", season.FormattedK9);
        Assert.Equal("1.57", season.FormattedBb9);
        Assert.Equal("0.923", season.FormattedWhip);
    }

    [Fact]
    public void AdvancedPitchingSeason_NullStats_RenderAsDash()
    {
        var season = new AdvancedPitchingSeason { Year = 1871 };

        Assert.Equal("—", season.FormattedIp);
        Assert.Equal("—", season.FormattedK9);
        Assert.Equal("—", season.FormattedWhip);
    }
}
