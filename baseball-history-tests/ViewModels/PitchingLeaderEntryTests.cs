using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class PitchingLeaderEntryTests
{
    [Fact]
    public void Era_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { EarnedRuns = 81, InningsPitched = 200 };
        // ERA = (81 * 9) / 200 = 729 / 200 = 3.645
        Assert.Equal(3.645, entry.Era, 3);
    }

    [Fact]
    public void Whip_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { Walks = 45, Hits = 180, InningsPitched = 225 };
        // WHIP = (45 + 180) / 225 = 225 / 225 = 1.0
        Assert.Equal(1.0, entry.Whip, 2);
    }

    [Fact]
    public void StrikeoutsPer9_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { Strikeouts = 225, InningsPitched = 200 };
        // K/9 = (225 * 9) / 200 = 2025 / 200 = 10.125
        Assert.Equal(10.125, entry.StrikeoutsPer9, 3);
    }

    [Fact]
    public void WalksPer9_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { Walks = 50, InningsPitched = 200 };
        // BB/9 = (50 * 9) / 200 = 450 / 200 = 2.25
        Assert.Equal(2.25, entry.WalksPer9, 2);
    }

    [Fact]
    public void StrikeoutToWalkRatio_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { Strikeouts = 200, Walks = 50 };
        Assert.Equal(4.0, entry.StrikeoutToWalkRatio);
    }

    [Fact]
    public void StrikeoutToWalkRatio_WithZeroWalks_ReturnsStrikeouts()
    {
        var entry = new PitchingLeaderEntry { Strikeouts = 150, Walks = 0 };
        Assert.Equal(150, entry.StrikeoutToWalkRatio);
    }

    [Fact]
    public void WinningPercentage_CalculatesCorrectly()
    {
        var entry = new PitchingLeaderEntry { Wins = 18, Losses = 6 };
        Assert.Equal(0.75, entry.WinningPercentage, 2);
    }

    [Fact]
    public void WinningPercentage_WithZeroDecisions_ReturnsZero()
    {
        var entry = new PitchingLeaderEntry { Wins = 0, Losses = 0 };
        Assert.Equal(0, entry.WinningPercentage);
    }

    [Theory]
    [InlineData("w", 18)]
    [InlineData("wins", 18)]
    [InlineData("l", 8)]
    [InlineData("losses", 8)]
    [InlineData("so", 225)]
    [InlineData("strikeouts", 225)]
    [InlineData("sv", 0)]
    [InlineData("saves", 0)]
    [InlineData("cg", 5)]
    [InlineData("completegames", 5)]
    [InlineData("sho", 2)]
    [InlineData("shutouts", 2)]
    [InlineData("g", 35)]
    [InlineData("games", 35)]
    [InlineData("gs", 33)]
    [InlineData("gamesstarted", 33)]
    public void GetStatValue_ReturnsCorrectValue(string statColumn, int expectedValue)
    {
        var entry = new PitchingLeaderEntry
        {
            Games = 35,
            GamesStarted = 33,
            Wins = 18,
            Losses = 8,
            Saves = 0,
            CompleteGames = 5,
            Shutouts = 2,
            Strikeouts = 225,
            InningsPitched = 220
        };

        var result = entry.GetStatValue(statColumn);
        Assert.Equal(expectedValue, Convert.ToInt32(result));
    }

    [Fact]
    public void GetStatValue_WithRateStats_ReturnsDouble()
    {
        var entry = new PitchingLeaderEntry
        {
            EarnedRuns = 72,
            InningsPitched = 200,
            Walks = 50,
            Hits = 180,
            Strikeouts = 200
        };

        var era = (double)entry.GetStatValue("era");
        Assert.Equal(3.24, era, 2);

        var whip = (double)entry.GetStatValue("whip");
        Assert.Equal(1.15, whip, 2);
    }

    [Fact]
    public void GetStatValue_InningsPitched()
    {
        var entry = new PitchingLeaderEntry { InningsPitched = 215.2 };
        var ip = entry.GetStatValue("ip");
        Assert.Equal(215.2, ip);
    }

    [Fact]
    public void GetStatValue_WinningPercentage()
    {
        var entry = new PitchingLeaderEntry { Wins = 15, Losses = 5 };
        var wpct = (double)entry.GetStatValue("wpct");
        Assert.Equal(0.75, wpct, 2);
    }

    [Fact]
    public void GetStatValue_UnknownStat_ReturnsWins()
    {
        var entry = new PitchingLeaderEntry { Wins = 20 };
        var result = entry.GetStatValue("unknown");
        Assert.Equal(20, Convert.ToInt32(result));
    }

    [Fact]
    public void FormattedStats_FormatCorrectly()
    {
        var entry = new PitchingLeaderEntry
        {
            Wins = 18,
            Losses = 8,
            EarnedRuns = 72,
            InningsPitched = 200,
            Walks = 50,
            Hits = 180,
            Strikeouts = 200
        };

        Assert.Equal("3.24", entry.FormattedEra);
        Assert.Equal("1.15", entry.FormattedWhip);
        Assert.Equal("200.0", entry.FormattedIp);
        Assert.Equal("18-8", entry.WinLossRecord);
    }

    [Fact]
    public void FormattedK9_FormatsCorrectly()
    {
        var entry = new PitchingLeaderEntry { Strikeouts = 225, InningsPitched = 200 };
        Assert.Equal("10.1", entry.FormattedK9);
    }

    [Fact]
    public void FormattedWinPct_FormatsCorrectly()
    {
        var entry = new PitchingLeaderEntry { Wins = 20, Losses = 5 };
        Assert.Equal(".800", entry.FormattedWinPct);
    }
}
