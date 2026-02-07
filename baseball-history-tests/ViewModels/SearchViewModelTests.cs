using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class SearchViewModelTests
{
    [Fact]
    public void TotalResults_WithBothTypes_SumsCorrectly()
    {
        var vm = new SearchViewModel
        {
            Players = new List<SearchResult>
            {
                new() { Id = "1" },
                new() { Id = "2" }
            },
            Teams = new List<SearchResult>
            {
                new() { Id = "1" }
            }
        };

        Assert.Equal(3, vm.TotalResults);
    }

    [Fact]
    public void TotalResults_WithOnlyPlayers()
    {
        var vm = new SearchViewModel
        {
            Players = new List<SearchResult>
            {
                new() { Id = "1" },
                new() { Id = "2" }
            }
        };

        Assert.Equal(2, vm.TotalResults);
    }

    [Fact]
    public void TotalResults_WithEmpty_ReturnsZero()
    {
        var vm = new SearchViewModel();
        Assert.Equal(0, vm.TotalResults);
    }

    [Fact]
    public void HasResults_WithResults_ReturnsTrue()
    {
        var vm = new SearchViewModel
        {
            Players = new List<SearchResult> { new() { Id = "1" } }
        };

        Assert.True(vm.HasResults);
    }

    [Fact]
    public void HasResults_WithNoResults_ReturnsFalse()
    {
        var vm = new SearchViewModel();
        Assert.False(vm.HasResults);
    }

    [Fact]
    public void DefaultLists_AreInitialized()
    {
        var vm = new SearchViewModel();

        Assert.NotNull(vm.Players);
        Assert.Empty(vm.Players);
        Assert.NotNull(vm.Teams);
        Assert.Empty(vm.Teams);
    }
}

public class SearchResultTests
{
    [Fact]
    public void Url_ForPlayer_ReturnsCorrectPath()
    {
        var result = new SearchResult
        {
            Id = "ruthba01",
            Type = SearchResultType.Player
        };

        Assert.Equal("/Players/Details/ruthba01", result.Url);
    }

    [Fact]
    public void Url_ForTeam_ReturnsCorrectPath()
    {
        var result = new SearchResult
        {
            Id = "NYA",
            Type = SearchResultType.Team
        };

        Assert.Equal("/Teams/Season/NYA", result.Url);
    }

    [Fact]
    public void Url_ForFranchise_ReturnsCorrectPath()
    {
        var result = new SearchResult
        {
            Id = "NYY",
            Type = SearchResultType.Franchise
        };

        Assert.Equal("/Teams/Franchise/NYY", result.Url);
    }

    [Fact]
    public void TypeLabel_ForPlayer_ReturnsCorrect()
    {
        var result = new SearchResult { Type = SearchResultType.Player };
        Assert.Equal("Player", result.TypeLabel);
    }

    [Fact]
    public void TypeLabel_ForTeam_ReturnsCorrect()
    {
        var result = new SearchResult { Type = SearchResultType.Team };
        Assert.Equal("Team", result.TypeLabel);
    }

    [Fact]
    public void TypeLabel_ForFranchise_ReturnsCorrect()
    {
        var result = new SearchResult { Type = SearchResultType.Franchise };
        Assert.Equal("Franchise", result.TypeLabel);
    }

    [Fact]
    public void AllProperties_SetCorrectly()
    {
        var result = new SearchResult
        {
            Id = "ruthba01",
            Title = "Babe Ruth",
            Subtitle = "1914-1935",
            Type = SearchResultType.Player,
            Initials = "BR",
            TeamId = "NYA",
            IsInHallOfFame = true
        };

        Assert.Equal("ruthba01", result.Id);
        Assert.Equal("Babe Ruth", result.Title);
        Assert.Equal("1914-1935", result.Subtitle);
        Assert.Equal("BR", result.Initials);
        Assert.Equal("NYA", result.TeamId);
        Assert.True(result.IsInHallOfFame);
    }
}

public class PlayerListViewModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var vm = new PlayerListViewModel();

        Assert.NotNull(vm.Players);
        Assert.Empty(vm.Players);
        Assert.Null(vm.CurrentLetter);
        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(0, vm.TotalPages);
        Assert.Equal(0, vm.TotalPlayers);
        Assert.Equal(50, vm.PageSize);
        Assert.NotNull(vm.AvailableLetters);
        Assert.Empty(vm.AvailableLetters);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var vm = new PlayerListViewModel
        {
            CurrentLetter = "A",
            CurrentPage = 2,
            TotalPages = 5,
            TotalPlayers = 250,
            PageSize = 50,
            AvailableLetters = new List<char> { 'A', 'B', 'C' }
        };

        Assert.Equal("A", vm.CurrentLetter);
        Assert.Equal(2, vm.CurrentPage);
        Assert.Equal(5, vm.TotalPages);
        Assert.Equal(250, vm.TotalPlayers);
        Assert.Equal(3, vm.AvailableLetters.Count);
    }
}

public class PlayerSummaryTests
{
    [Fact]
    public void FromPeople_SetsBasicInfo()
    {
        var person = new baseball_history_web.Models.People
        {
            PlayerId = "ruthba01",
            NameFirst = "Babe",
            NameLast = "Ruth",
            BirthYear = "1895",
            Debut = new DateOnly(1914, 7, 11),
            FinalGame = new DateOnly(1935, 5, 30)
        };

        var summary = PlayerSummary.FromPeople(person, isHof: true, games: 2503, hits: 2873, hrs: 714);

        Assert.Equal("ruthba01", summary.PlayerId);
        Assert.Equal("Babe", summary.FirstName);
        Assert.Equal("Ruth", summary.LastName);
        Assert.Equal("Babe Ruth", summary.FullName);
        Assert.Equal("1895", summary.BirthYear);
        Assert.Equal("1914", summary.DebutYear);
        Assert.Equal("1935", summary.FinalYear);
        Assert.True(summary.IsInHallOfFame);
        Assert.Equal(2503, summary.TotalGames);
        Assert.Equal(2873, summary.TotalHits);
        Assert.Equal(714, summary.TotalHomeRuns);
    }

    [Fact]
    public void FromPeople_WithNullDates_HandlesGracefully()
    {
        var person = new baseball_history_web.Models.People
        {
            PlayerId = "test",
            NameFirst = "Test",
            NameLast = "Player",
            Debut = null,
            FinalGame = null
        };

        var summary = PlayerSummary.FromPeople(person);

        Assert.Null(summary.DebutYear);
        Assert.Null(summary.FinalYear);
    }

    [Fact]
    public void FromPeople_WithNullFirstName_HandlesTrim()
    {
        var person = new baseball_history_web.Models.People
        {
            PlayerId = "test",
            NameFirst = null,
            NameLast = "Smith"
        };

        var summary = PlayerSummary.FromPeople(person);

        Assert.Equal("Smith", summary.FullName);
    }

    [Fact]
    public void FromPeople_WithNullStats()
    {
        var person = new baseball_history_web.Models.People
        {
            PlayerId = "test",
            NameFirst = "Test",
            NameLast = "Player"
        };

        var summary = PlayerSummary.FromPeople(person);

        Assert.Null(summary.TotalGames);
        Assert.Null(summary.TotalHits);
        Assert.Null(summary.TotalHomeRuns);
        Assert.False(summary.IsInHallOfFame);
    }
}
