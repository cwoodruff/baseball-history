using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class AllStarViewModelTests
{
    [Fact]
    public void GroupKey_ClassifiesGames()
    {
        Assert.Equal("MLB|NLS195907070", AllStarGames.GroupKey("NL", "NLS195907070"));
        Assert.Equal("MLB|NLS195907070", AllStarGames.GroupKey("AL", "NLS195907070"));
        Assert.Equal(AllStarGames.EastWestKey, AllStarGames.GroupKey("EAS", ""));
        Assert.Equal(AllStarGames.EastWestKey, AllStarGames.GroupKey("WES", ""));
        Assert.Equal(AllStarGames.NorthSouthKey, AllStarGames.GroupKey("NOS", ""));
        Assert.Equal(AllStarGames.NorthSouthKey, AllStarGames.GroupKey("SAS", ""));
        Assert.Equal("OTHER|NNN", AllStarGames.GroupKey("NNN", ""));
    }

    [Fact]
    public void TitleFor_NamesKnownGames()
    {
        Assert.Equal("All-Star Game", AllStarGames.TitleFor("MLB|NLS195907070", ["AL", "NL"]));
        Assert.Equal("East-West Game", AllStarGames.TitleFor(AllStarGames.EastWestKey, ["EAS", "WES"]));
        Assert.Equal("North-South Game", AllStarGames.TitleFor(AllStarGames.NorthSouthKey, ["NOS", "SAS"]));
        Assert.Equal("NNN vs. NNS", AllStarGames.TitleFor("OTHER|NNN", ["NNN", "NNS"]));
    }

    [Fact]
    public void TypeOrder_SortsMlbFirst()
    {
        Assert.True(AllStarGames.TypeOrder("MLB|X") < AllStarGames.TypeOrder(AllStarGames.EastWestKey));
        Assert.True(AllStarGames.TypeOrder(AllStarGames.EastWestKey) <
                    AllStarGames.TypeOrder(AllStarGames.NorthSouthKey));
    }

    [Fact]
    public void ParseGameDate_ReadsRetrosheetId()
    {
        Assert.Equal(new DateOnly(1959, 7, 7), AllStarGameViewModel.ParseGameDate("NLS195907070"));
        Assert.Null(AllStarGameViewModel.ParseGameDate(""));
        Assert.Null(AllStarGameViewModel.ParseGameDate(null));
        Assert.Null(AllStarGameViewModel.ParseGameDate("SHORT"));
    }

    [Fact]
    public void PositionName_MapsStartingPositions()
    {
        Assert.Equal("P", new AllStarRosterRow { PlayerId = "x", FullName = "x", TeamId = "x", StartingPos = 1 }.PositionName);
        Assert.Equal("RF", new AllStarRosterRow { PlayerId = "x", FullName = "x", TeamId = "x", StartingPos = 9 }.PositionName);
        Assert.Equal("DH", new AllStarRosterRow { PlayerId = "x", FullName = "x", TeamId = "x", StartingPos = 10 }.PositionName);
        var reserve = new AllStarRosterRow { PlayerId = "x", FullName = "x", TeamId = "x", StartingPos = null };
        Assert.Equal("—", reserve.PositionName);
        Assert.False(reserve.IsStarter);
    }

    [Fact]
    public void SquadName_MapsShowcaseSquads()
    {
        Assert.Equal("East", AllStarGames.SquadName("EAS"));
        Assert.Equal("West", AllStarGames.SquadName("WES"));
        Assert.Equal("North", AllStarGames.SquadName("NOS"));
        Assert.Equal("South", AllStarGames.SquadName("SAS"));
        Assert.Equal("AL", AllStarGames.SquadName("AL"));
    }

    [Fact]
    public void YearViewModel_MlbGameCount_IgnoresShowcaseGames()
    {
        var vm = new AllStarYearViewModel
        {
            Year = 1943,
            Games =
            {
                new AllStarGameViewModel { GroupKey = "MLB|ALS194307130", Title = "All-Star Game" },
                new AllStarGameViewModel { GroupKey = AllStarGames.EastWestKey, Title = "East-West Game" }
            }
        };

        Assert.Equal(1, vm.MlbGameCount);
        Assert.True(vm.HasMultipleGames);
    }

    [Fact]
    public void YearViewModel_PreviousAndNextYear()
    {
        var vm = new AllStarYearViewModel { Year = 1959, AvailableYears = [1958, 1959, 1960] };
        Assert.Equal((short)1958, vm.PreviousYear);
        Assert.Equal((short)1960, vm.NextYear);
    }
}
