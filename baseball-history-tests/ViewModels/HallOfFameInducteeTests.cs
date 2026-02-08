using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class HallOfFameInducteeTests
{
    [Fact]
    public void IsPitcher_WithPPosition_ReturnsTrue()
    {
        var inductee = new HallOfFameInductee { PrimaryPosition = "P" };
        Assert.True(inductee.IsPitcher);
    }

    [Fact]
    public void IsPitcher_WithNonPPosition_ReturnsFalse()
    {
        var inductee = new HallOfFameInductee { PrimaryPosition = "SS" };
        Assert.False(inductee.IsPitcher);
    }

    [Fact]
    public void IsPitcher_WithNullPosition_ReturnsFalse()
    {
        var inductee = new HallOfFameInductee { PrimaryPosition = null };
        Assert.False(inductee.IsPitcher);
    }

    [Fact]
    public void FormattedVotePct_WithValue_FormatsCorrectly()
    {
        var inductee = new HallOfFameInductee { VotePercentage = 95.5 };
        Assert.Equal("95.5%", inductee.FormattedVotePct);
    }

    [Fact]
    public void FormattedVotePct_WithNull_ReturnsDash()
    {
        var inductee = new HallOfFameInductee { VotePercentage = null };
        Assert.Equal("-", inductee.FormattedVotePct);
    }

    [Fact]
    public void FormattedVotePct_With100Percent()
    {
        var inductee = new HallOfFameInductee { VotePercentage = 100.0 };
        Assert.Equal("100.0%", inductee.FormattedVotePct);
    }

    [Fact]
    public void FormattedCareerAvg_WithValue_FormatsCorrectly()
    {
        var inductee = new HallOfFameInductee { CareerAvg = 0.305 };
        Assert.Equal(".305", inductee.FormattedCareerAvg);
    }

    [Fact]
    public void FormattedCareerAvg_WithNull_ReturnsDash()
    {
        var inductee = new HallOfFameInductee { CareerAvg = null };
        Assert.Equal("-", inductee.FormattedCareerAvg);
    }

    [Fact]
    public void FormattedCareerAvg_WithHighAverage()
    {
        var inductee = new HallOfFameInductee { CareerAvg = 0.366 };
        Assert.Equal(".366", inductee.FormattedCareerAvg);
    }

    [Fact]
    public void FormattedCareerEra_WithValue_FormatsCorrectly()
    {
        var inductee = new HallOfFameInductee { CareerEra = 2.89 };
        Assert.Equal("2.89", inductee.FormattedCareerEra);
    }

    [Fact]
    public void FormattedCareerEra_WithNull_ReturnsDash()
    {
        var inductee = new HallOfFameInductee { CareerEra = null };
        Assert.Equal("-", inductee.FormattedCareerEra);
    }

    [Fact]
    public void FormattedCareerEra_WithLowEra()
    {
        var inductee = new HallOfFameInductee { CareerEra = 1.82 };
        Assert.Equal("1.82", inductee.FormattedCareerEra);
    }

    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var inductee = new HallOfFameInductee
        {
            PlayerId = "ruthba01",
            FullName = "Babe Ruth",
            FirstName = "Babe",
            LastName = "Ruth",
            InductionYear = 1936,
            Category = "Player",
            VotedBy = "BBWAA",
            VotePercentage = 95.1,
            DebutYear = "1914",
            FinalYear = "1935",
            CareerYears = 22,
            TotalHits = 2873,
            TotalHomeRuns = 714,
            CareerAvg = 0.342
        };

        Assert.Equal("ruthba01", inductee.PlayerId);
        Assert.Equal("Babe Ruth", inductee.FullName);
        Assert.Equal(1936, inductee.InductionYear);
        Assert.Equal("Player", inductee.Category);
        Assert.Equal(714, inductee.TotalHomeRuns);
    }
}