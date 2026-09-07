using BaseballHistory.Data.Models;
using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class TeamListViewModelTests
{
    [Fact]
    public void HasActiveFilters_ReflectsSearchAndEraFilters()
    {
        var model = new TeamListViewModel();
        Assert.False(model.HasActiveFilters);

        model.SearchQuery = "Yankees";
        Assert.True(model.HasActiveFilters);

        model.SearchQuery = null;
        model.Era = 1920;
        Assert.True(model.HasActiveFilters);
    }

    [Fact]
    public void TotalFranchises_SumsActiveAndInactiveLists()
    {
        var model = new TeamListViewModel
        {
            ActiveFranchises = [new FranchiseSummary(), new FranchiseSummary()],
            InactiveFranchises = [new FranchiseSummary()]
        };

        Assert.Equal(3, model.TotalFranchises);
    }

    [Fact]
    public void FromFranchise_AggregatesTeamHistoryAndUsesLatestTeamIdentity()
    {
        var franchise = new TeamsFranchises
        {
            FranchId = "WSN",
            FranchName = "Washington Nationals",
            Active = "Y"
        };

        var teams = new[]
        {
            new Teams { FranchId = "MON", TeamId = "MON", Name = "Montreal Expos", YearId = 2004, W = 67, L = 95, LgId = "NL", DivId = "E" },
            new Teams { FranchId = "WSN", TeamId = "WSN", Name = "Washington Nationals", YearId = 2019, W = 93, L = 69, LgId = "NL", DivId = "E", Wswin = "Y", LgWin = "Y" }
        };

        var summary = FranchiseSummary.FromFranchise(franchise, teams);

        Assert.Equal("WSN", summary.FranchiseId);
        Assert.Equal("Washington Nationals", summary.FranchiseName);
        Assert.True(summary.IsActive);
        Assert.Equal((short)2004, summary.FirstYear);
        Assert.Equal((short)2019, summary.LastYear);
        Assert.Equal(2, summary.TotalSeasons);
        Assert.Equal(160, summary.TotalWins);
        Assert.Equal(164, summary.TotalLosses);
        Assert.Equal(1, summary.WorldSeriesWins);
        Assert.Equal(1, summary.PennantWins);
        Assert.Equal("WSN", summary.CurrentTeamId);
        Assert.Equal("NL", summary.CurrentLeague);
        Assert.Equal("E", summary.CurrentDivision);
        Assert.Equal(".494", summary.FormattedWinPct);
    }

    [Fact]
    public void EraOptions_DelegatesToPlayerListEraOptions()
    {
        Assert.Equal(PlayerListViewModel.EraOptions, TeamListViewModel.EraOptions);
    }
}
