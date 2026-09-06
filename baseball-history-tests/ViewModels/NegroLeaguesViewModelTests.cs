using baseball_history_web.Services;
using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class NegroLeaguesViewModelTests
{
    private static NegroLeagueStandingsRow Row(string teamId, short wins, short losses) => new()
    {
        TeamId = teamId,
        Name = teamId,
        Wins = wins,
        Losses = losses
    };

    [Fact]
    public void Registry_ContainsSevenLeagues()
    {
        Assert.Equal(7, NegroLeagues.All.Count);
        Assert.Equal(new[] { "NNL", "ECL", "ANL", "EWL", "NSL", "NN2", "NAL" },
            NegroLeagues.All.Select(l => l.Id).ToArray());
    }

    [Fact]
    public void Registry_Find_IsCaseInsensitiveAndTrims()
    {
        Assert.Equal("NN2", NegroLeagues.Find("nn2")?.Id);
        Assert.Equal("NNL", NegroLeagues.Find(" NNL ")?.Id);
        Assert.Null(NegroLeagues.Find("AL"));
        Assert.Null(NegroLeagues.Find(null));
        Assert.False(NegroLeagues.IsNegroLeague("NL"));
        Assert.True(NegroLeagues.IsNegroLeague("ECL"));
    }

    [Fact]
    public void ComputeGamesBehind_UsesPctLeader()
    {
        var standings = new List<NegroLeagueStandingsRow>
        {
            Row("HG", 26, 7),
            Row("NYC", 17, 9),
            Row("NE", 18, 14)
        };

        NegroLeagueStandingsRow.ComputeGamesBehind(standings);

        Assert.Equal(0, standings[0].GamesBehind);
        Assert.Equal(5.5, standings[1].GamesBehind);
        Assert.Equal(7.5, standings[2].GamesBehind);
    }

    [Fact]
    public void FormattedGamesBehind_FormatsWholeHalfAndLeader()
    {
        Assert.Equal("—", new NegroLeagueStandingsRow { GamesBehind = 0 }.FormattedGamesBehind);
        Assert.Equal("—", new NegroLeagueStandingsRow { GamesBehind = null }.FormattedGamesBehind);
        Assert.Equal("4", new NegroLeagueStandingsRow { GamesBehind = 4.0 }.FormattedGamesBehind);
        Assert.Equal("5.5", new NegroLeagueStandingsRow { GamesBehind = 5.5 }.FormattedGamesBehind);
    }

    [Fact]
    public void StandingsRow_WinningPercentage()
    {
        var row = Row("HG", 26, 7);
        Assert.Equal(26.0 / 33.0, row.WinningPercentage, 5);
        Assert.Equal(".788", row.FormattedWinPct);
        Assert.Equal("26-7", row.Record);
    }

    [Fact]
    public void SeasonViewModel_PreviousAndNextYear()
    {
        var vm = new NegroLeagueSeasonViewModel
        {
            Year = 1943,
            AvailableYears = [1933, 1942, 1943, 1944, 1948]
        };

        Assert.Equal((short)1942, vm.PreviousYear);
        Assert.Equal((short)1944, vm.NextYear);
    }

    [Fact]
    public void SeasonViewModel_EdgeYears_HaveNoNeighbors()
    {
        var first = new NegroLeagueSeasonViewModel { Year = 1933, AvailableYears = [1933, 1934] };
        var last = new NegroLeagueSeasonViewModel { Year = 1948, AvailableYears = [1947, 1948] };

        Assert.Null(first.PreviousYear);
        Assert.Equal((short)1934, first.NextYear);
        Assert.Equal((short)1947, last.PreviousYear);
        Assert.Null(last.NextYear);
    }

    [Fact]
    public void Club_YearsLabelAndRecord()
    {
        var club = new NegroLeagueClub
        {
            TeamId = "HG",
            Name = "Homestead Grays",
            FirstYear = 1935,
            LastYear = 1948,
            Wins = 100,
            Losses = 50
        };

        Assert.Equal("1935–1948", club.YearsLabel);
        Assert.Equal("100-50", club.Record);
        Assert.Equal(".667", club.FormattedWinPct);
    }

    [Fact]
    public void LeagueInfo_YearsLabel_SingleSeasonLeague()
    {
        var anl = NegroLeagues.Find("ANL")!;
        Assert.Equal("1929", anl.YearsLabel);
    }
}
