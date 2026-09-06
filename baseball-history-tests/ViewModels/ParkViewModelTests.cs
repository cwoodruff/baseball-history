using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class ParkViewModelTests
{
    private static ParkSeasonRow Season(short year, string teamId, string lgId, short? games = null,
        int? attendance = null) => new()
    {
        Year = year,
        TeamId = teamId,
        LgId = lgId,
        Games = games,
        Attendance = attendance
    };

    [Fact]
    public void ParkSummary_YearsActive_FormatsRange()
    {
        var park = new ParkSummary { FirstYear = 1912, LastYear = 2023 };
        Assert.Equal("1912–2023", park.YearsActive);
    }

    [Fact]
    public void ParkSummary_YearsActive_SingleYear()
    {
        var park = new ParkSummary { FirstYear = 1884, LastYear = 1884 };
        Assert.Equal("1884", park.YearsActive);
    }

    [Fact]
    public void ParkSummary_YearsActive_NoData()
    {
        var park = new ParkSummary();
        Assert.Equal("—", park.YearsActive);
    }

    [Fact]
    public void ParkSummary_Location_OmitsUsCountry()
    {
        var park = new ParkSummary { City = "Boston", State = "MA", Country = "US" };
        Assert.Equal("Boston, MA", park.Location);
    }

    [Fact]
    public void ParkSummary_Location_IncludesForeignCountry()
    {
        var park = new ParkSummary { City = "Tokyo", Country = "Japan" };
        Assert.Equal("Tokyo, Japan", park.Location);
    }

    [Fact]
    public void ParkDetail_AliasNames_SplitsOnSemicolon()
    {
        var park = new ParkDetailViewModel { ParkKey = "CHI11", Alias = "Weeghman Park; Cubs Park" };
        Assert.Equal(new[] { "Weeghman Park", "Cubs Park" }, park.AliasNames);
    }

    [Fact]
    public void ParkDetail_AliasNames_EmptyWhenNoAlias()
    {
        var park = new ParkDetailViewModel { ParkKey = "BOS07", Alias = "" };
        Assert.Empty(park.AliasNames);
    }

    [Fact]
    public void ParkDetail_AttendanceByYear_SumsAcrossTenantsAndSortsAscending()
    {
        var park = new ParkDetailViewModel
        {
            ParkKey = "BOS07",
            Seasons =
            {
                Season(1913, "BOS", "AL", attendance: 490600),
                Season(1913, "BSN", "NL", attendance: 45570),
                Season(1912, "BOS", "AL", attendance: 708050)
            }
        };

        var byYear = park.AttendanceByYear;

        Assert.Equal(2, byYear.Count);
        Assert.Equal(1912, byYear[0].Year);
        Assert.Equal(708050, byYear[0].Attendance);
        Assert.Equal(1913, byYear[1].Year);
        Assert.Equal(490600 + 45570, byYear[1].Attendance);
    }

    [Fact]
    public void ParkDetail_AttendanceByYear_SkipsYearsWithoutAttendance()
    {
        var park = new ParkDetailViewModel
        {
            ParkKey = "SFP01",
            Seasons =
            {
                Season(1900, "X", "NL"),
                Season(1901, "X", "NL", attendance: 1000)
            }
        };

        Assert.Single(park.AttendanceByYear);
        Assert.False(park.Seasons[0].Attendance.HasValue);
        Assert.True(park.HasAttendanceData);
    }

    [Fact]
    public void ParkDetail_PeakAttendanceYear_FindsMaximum()
    {
        var park = new ParkDetailViewModel
        {
            ParkKey = "BOS07",
            Seasons =
            {
                Season(1912, "BOS", "AL", attendance: 708050),
                Season(1975, "BOS", "AL", attendance: 1748518),
                Season(1913, "BOS", "AL", attendance: 490600)
            }
        };

        Assert.Equal((short?)1975, park.PeakAttendanceYear?.Year);
        Assert.Equal("1,748,518", park.PeakAttendanceYear?.FormattedAttendance);
    }

    [Fact]
    public void ParkDetail_SeasonCount_CountsDistinctYears()
    {
        var park = new ParkDetailViewModel
        {
            ParkKey = "BOS07",
            Seasons =
            {
                Season(1913, "BOS", "AL", games: 75),
                Season(1913, "BSN", "NL", games: 4),
                Season(1914, "BOS", "AL", games: 77)
            }
        };

        Assert.Equal(2, park.SeasonCount);
        Assert.Equal(75 + 4 + 77, park.TotalGames);
        Assert.Equal("1913–1914", park.YearsActive);
    }

    [Fact]
    public void ParkSeasonRow_FormattedAttendance_DashWhenMissing()
    {
        Assert.Equal("—", Season(1900, "X", "NL").FormattedAttendance);
        Assert.Equal("1,748,518", Season(1975, "BOS", "AL", attendance: 1748518).FormattedAttendance);
    }

    [Fact]
    public void ParkListViewModel_FilterQueryParams_IncludesActiveFilters()
    {
        var vm = new ParkListViewModel { SearchQuery = "fenway", SelectedState = "MA" };

        var queryParams = vm.FilterQueryParams();

        Assert.Equal("fenway", queryParams["q"]);
        Assert.Equal("MA", queryParams["state"]);
        Assert.True(vm.HasActiveFilters);
    }

    [Fact]
    public void ParkListViewModel_TotalPages_RoundsUp()
    {
        var vm = new ParkListViewModel { TotalParks = 345, PageSize = 30 };
        Assert.Equal(12, vm.TotalPages);
    }
}
