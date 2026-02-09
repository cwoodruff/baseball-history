using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class TeamSeasonViewModelTests
{
    [Fact]
    public void WinningPercentage_CalculatesCorrectly()
    {
        var vm = new TeamSeasonViewModel { Wins = 100, Losses = 62 };
        Assert.Equal(100.0 / 162.0, vm.WinningPercentage, 4);
    }

    [Fact]
    public void WinningPercentage_WithZeroGames_ReturnsZero()
    {
        var vm = new TeamSeasonViewModel { Wins = 0, Losses = 0 };
        Assert.Equal(0, vm.WinningPercentage);
    }

    [Fact]
    public void FormattedWinPct_FormatsCorrectly()
    {
        var vm = new TeamSeasonViewModel { Wins = 60, Losses = 40 };
        Assert.Equal(".600", vm.FormattedWinPct);
    }

    [Fact]
    public void Record_FormatsCorrectly()
    {
        var vm = new TeamSeasonViewModel { Wins = 95, Losses = 67 };
        Assert.Equal("95-67", vm.Record);
    }

    [Fact]
    public void FormattedAttendance_WithValue_FormatsWithCommas()
    {
        var vm = new TeamSeasonViewModel { Attendance = 3500000 };
        Assert.Equal("3,500,000", vm.FormattedAttendance);
    }

    [Fact]
    public void FormattedAttendance_WithNull_ReturnsNA()
    {
        var vm = new TeamSeasonViewModel { Attendance = null };
        Assert.Equal("N/A", vm.FormattedAttendance);
    }

    [Fact]
    public void AllPostseasonFlags_WorkCorrectly()
    {
        var vm = new TeamSeasonViewModel
        {
            WonDivision = true,
            WonWildCard = false,
            WonPennant = true,
            WonWorldSeries = true
        };

        Assert.True(vm.WonDivision);
        Assert.False(vm.WonWildCard);
        Assert.True(vm.WonPennant);
        Assert.True(vm.WonWorldSeries);
    }
}

public class TeamBattingStatsTests
{
    [Fact]
    public void BattingAverage_CalculatesCorrectly()
    {
        var stats = new TeamBattingStats { Hits = 1500, AtBats = 5500 };
        Assert.Equal(1500.0 / 5500.0, stats.BattingAverage, 4);
    }

    [Fact]
    public void BattingAverage_WithZeroAtBats_ReturnsZero()
    {
        var stats = new TeamBattingStats { Hits = 0, AtBats = 0 };
        Assert.Equal(0, stats.BattingAverage);
    }

    [Fact]
    public void FormattedAvg_FormatsCorrectly()
    {
        var stats = new TeamBattingStats { Hits = 1450, AtBats = 5500 };
        Assert.Matches(@"^\.\d{3}$", stats.FormattedAvg);
    }
}

public class TeamPitchingStatsTests
{
    [Fact]
    public void FormattedEra_FormatsToTwoDecimals()
    {
        var stats = new TeamPitchingStats { Era = 3.85 };
        Assert.Equal("3.85", stats.FormattedEra);
    }

    [Fact]
    public void FormattedEra_WithLowEra()
    {
        var stats = new TeamPitchingStats { Era = 2.75 };
        Assert.Equal("2.75", stats.FormattedEra);
    }
}

public class RosterPlayerTests
{
    [Fact]
    public void FormattedAvg_WithValue_FormatsCorrectly()
    {
        var player = new RosterPlayer { BattingAverage = 0.285 };
        Assert.Equal(".285", player.FormattedAvg);
    }

    [Fact]
    public void FormattedAvg_WithNull_ReturnsZero()
    {
        var player = new RosterPlayer { BattingAverage = null };
        Assert.Equal(".000", player.FormattedAvg);
    }

    [Fact]
    public void FormattedEra_WithValue_FormatsCorrectly()
    {
        var player = new RosterPlayer { Era = 3.45 };
        Assert.Equal("3.45", player.FormattedEra);
    }

    [Fact]
    public void FormattedEra_WithNull_ReturnsZero()
    {
        var player = new RosterPlayer { Era = null };
        Assert.Equal("0.00", player.FormattedEra);
    }

    [Fact]
    public void WinLossRecord_WithValues_FormatsCorrectly()
    {
        var player = new RosterPlayer { Wins = 15, Losses = 8 };
        Assert.Equal("15-8", player.WinLossRecord);
    }

    [Fact]
    public void WinLossRecord_WithNullWins_ReturnsNull()
    {
        var player = new RosterPlayer { Wins = null, Losses = null };
        Assert.Null(player.WinLossRecord);
    }
}

public class ManagerInfoTests
{
    [Fact]
    public void Record_FormatsCorrectly()
    {
        var manager = new ManagerInfo { Wins = 85, Losses = 77 };
        Assert.Equal("85-77", manager.Record);
    }

    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var manager = new ManagerInfo
        {
            PlayerId = "larusto01",
            FullName = "Tony La Russa",
            Games = 162,
            Wins = 95,
            Losses = 67,
            Order = 1,
            IsInHallOfFame = true
        };

        Assert.Equal("larusto01", manager.PlayerId);
        Assert.Equal("Tony La Russa", manager.FullName);
        Assert.Equal(162, manager.Games);
        Assert.True(manager.IsInHallOfFame);
        Assert.Equal("95-67", manager.Record);
    }
}
