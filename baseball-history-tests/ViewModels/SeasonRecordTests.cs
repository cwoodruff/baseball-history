using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class SeasonBattingRecordTests
{
    [Fact]
    public void BattingAverage_CalculatesCorrectly()
    {
        var record = new SeasonBattingRecord { Hits = 185, AtBats = 550 };
        Assert.Equal(185.0 / 550.0, record.BattingAverage, 4);
    }

    [Fact]
    public void BattingAverage_WithZeroAtBats_ReturnsZero()
    {
        var record = new SeasonBattingRecord { Hits = 0, AtBats = 0 };
        Assert.Equal(0, record.BattingAverage);
    }

    [Fact]
    public void FormattedAvg_FormatsCorrectly()
    {
        var record = new SeasonBattingRecord { Hits = 200, AtBats = 600 };
        Assert.Equal(".333", record.FormattedAvg);
    }

    [Fact]
    public void FormattedAvg_WithHighAverage()
    {
        var record = new SeasonBattingRecord { Hits = 240, AtBats = 600 };
        Assert.Equal(".400", record.FormattedAvg);
    }

    [Fact]
    public void FormattedAvg_WithLowAverage()
    {
        var record = new SeasonBattingRecord { Hits = 120, AtBats = 500 };
        Assert.Equal(".240", record.FormattedAvg);
    }

    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var record = new SeasonBattingRecord
        {
            Year = 1927,
            TeamId = "NYA",
            TeamName = "New York Yankees",
            LgId = "AL",
            Games = 151,
            AtBats = 540,
            Runs = 158,
            Hits = 192,
            Doubles = 29,
            Triples = 8,
            HomeRuns = 60,
            Rbi = 164,
            StolenBases = 7,
            Walks = 137,
            Strikeouts = 89
        };

        Assert.Equal(1927, record.Year);
        Assert.Equal("NYA", record.TeamId);
        Assert.Equal("New York Yankees", record.TeamName);
        Assert.Equal("AL", record.LgId);
        Assert.Equal(151, record.Games);
        Assert.Equal(60, record.HomeRuns);
        Assert.Equal(164, record.Rbi);
    }
}

public class SeasonPitchingRecordTests
{
    [Fact]
    public void Era_CalculatesCorrectly()
    {
        var record = new SeasonPitchingRecord { EarnedRuns = 60, InningsPitched = 250 };
        // ERA = (60 * 9) / 250 = 540 / 250 = 2.16
        Assert.Equal(2.16, record.Era, 2);
    }

    [Fact]
    public void Era_WithZeroInnings_ReturnsZero()
    {
        var record = new SeasonPitchingRecord { EarnedRuns = 0, InningsPitched = 0 };
        Assert.Equal(0, record.Era);
    }

    [Fact]
    public void FormattedEra_FormatsToTwoDecimals()
    {
        var record = new SeasonPitchingRecord { EarnedRuns = 54, InningsPitched = 200 };
        // ERA = 486/200 = 2.43
        Assert.Equal("2.43", record.FormattedEra);
    }

    [Fact]
    public void WinLossRecord_FormatsCorrectly()
    {
        var record = new SeasonPitchingRecord { Wins = 20, Losses = 6 };
        Assert.Equal("20-6", record.WinLossRecord);
    }

    [Fact]
    public void WinLossRecord_WithZeros()
    {
        var record = new SeasonPitchingRecord { Wins = 0, Losses = 0 };
        Assert.Equal("0-0", record.WinLossRecord);
    }

    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var record = new SeasonPitchingRecord
        {
            Year = 1968,
            TeamId = "SLN",
            TeamName = "St. Louis Cardinals",
            LgId = "NL",
            Games = 34,
            GamesStarted = 34,
            Wins = 22,
            Losses = 9,
            Saves = 0,
            InningsPitched = 304.7,
            Hits = 198,
            EarnedRuns = 38,
            Strikeouts = 268,
            Walks = 62
        };

        Assert.Equal(1968, record.Year);
        Assert.Equal("SLN", record.TeamId);
        Assert.Equal(22, record.Wins);
        Assert.Equal(268, record.Strikeouts);
        Assert.Equal(304.7, record.InningsPitched);
    }
}

public class AwardRecordTests
{
    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var award = new AwardRecord
        {
            Year = 1927,
            AwardId = "MVP",
            LgId = "AL",
            Notes = "Unanimous"
        };

        Assert.Equal(1927, award.Year);
        Assert.Equal("MVP", award.AwardId);
        Assert.Equal("AL", award.LgId);
        Assert.Equal("Unanimous", award.Notes);
    }
}

public class AllStarRecordTests
{
    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var allStar = new AllStarRecord
        {
            Year = 1933,
            LgId = "AL",
            TeamId = "NYA",
            GameNum = 1
        };

        Assert.Equal(1933, allStar.Year);
        Assert.Equal("AL", allStar.LgId);
        Assert.Equal("NYA", allStar.TeamId);
        Assert.Equal(1, allStar.GameNum);
    }
}

public class TeamRecordTests
{
    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var team = new TeamRecord
        {
            TeamId = "NYA",
            TeamName = "New York Yankees",
            FirstYear = 1920,
            LastYear = 1934,
            Seasons = 15
        };

        Assert.Equal("NYA", team.TeamId);
        Assert.Equal("New York Yankees", team.TeamName);
        Assert.Equal(1920, team.FirstYear);
        Assert.Equal(1934, team.LastYear);
        Assert.Equal(15, team.Seasons);
    }
}
