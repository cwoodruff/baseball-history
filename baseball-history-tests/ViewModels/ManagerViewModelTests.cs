using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class ManagerViewModelTests
{
    private static ManagerSeasonRow Season(short year, string teamId, short? wins, short? losses,
        bool pennant = false, bool ws = false, bool playerMgr = false) => new()
    {
        Year = year,
        TeamId = teamId,
        LgId = "AL",
        Wins = wins,
        Losses = losses,
        WonPennant = pennant,
        WonWorldSeries = ws,
        IsPlayerManager = playerMgr
    };

    [Fact]
    public void Detail_CareerTotals_SumAcrossSeasons()
    {
        var vm = new ManagerDetailViewModel
        {
            PlayerId = "test01",
            FullName = "Test Manager",
            Seasons =
            {
                Season(1930, "PHA", 102, 52, pennant: true, ws: true),
                Season(1929, "PHA", 104, 46, pennant: true, ws: true),
                Season(1928, "PHA", 98, 55)
            }
        };

        Assert.Equal(304, vm.Wins);
        Assert.Equal(153, vm.Losses);
        Assert.Equal("304-153", vm.Record);
        Assert.Equal(3, vm.SeasonCount);
        Assert.Equal(1, vm.TeamCount);
        Assert.Equal(2, vm.Pennants);
        Assert.Equal(2, vm.WorldSeriesTitles);
        Assert.Equal("1928–1930", vm.YearsLabel);
        Assert.Equal(".665", vm.FormattedWinPct);
        Assert.False(vm.WasPlayerManager);
    }

    [Fact]
    public void Detail_MultipleStintsInOneYear_CountOnceForSeasons()
    {
        var vm = new ManagerDetailViewModel
        {
            PlayerId = "test01",
            FullName = "Test Manager",
            Seasons =
            {
                Season(1960, "NYA", 40, 20),
                Season(1960, "BOS", 30, 30)
            }
        };

        Assert.Equal(1, vm.SeasonCount);
        Assert.Equal(2, vm.TeamCount);
        Assert.Equal(70, vm.Wins);
    }

    [Fact]
    public void Detail_PlayerManagerFlag_Propagates()
    {
        var vm = new ManagerDetailViewModel
        {
            PlayerId = "test01",
            FullName = "Test Manager",
            Seasons = { Season(1894, "PIT", 53, 55, playerMgr: true) }
        };

        Assert.True(vm.WasPlayerManager);
        Assert.Equal("1894", vm.YearsLabel);
    }

    [Fact]
    public void Detail_EmptySeasons_SafeDefaults()
    {
        var vm = new ManagerDetailViewModel { PlayerId = "test01", FullName = "Test Manager" };

        Assert.Equal("—", vm.YearsLabel);
        Assert.Equal("0-0", vm.Record);
        Assert.Equal(0, vm.WinningPercentage);
    }

    [Fact]
    public void SeasonRow_WinPct_HandlesNulls()
    {
        var row = Season(1901, "PHA", null, null);
        Assert.Equal("0-0", row.Record);
        Assert.Equal(0, row.WinningPercentage);
    }

    [Fact]
    public void Summary_FormatsRecordAndPct()
    {
        var summary = new ManagerSummary
        {
            PlayerId = "mackco01",
            FullName = "Connie Mack",
            FirstYear = 1894,
            LastYear = 1950,
            Wins = 3731,
            Losses = 3948
        };

        Assert.Equal("1894–1950", summary.YearsLabel);
        Assert.Equal("3731-3948", summary.Record);
        Assert.Equal(".486", summary.FormattedWinPct);
    }

    [Fact]
    public void ListViewModel_FilterQueryParams_OmitDefaults()
    {
        var vm = new ManagerListViewModel { SearchQuery = "mack", Sort = "wins" };
        var queryParams = vm.FilterQueryParams();

        Assert.Equal("mack", queryParams["q"]);
        Assert.False(queryParams.ContainsKey("sort"));

        vm.Sort = "name";
        Assert.Equal("name", vm.FilterQueryParams()["sort"]);
    }
}
