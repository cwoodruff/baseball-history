using baseball_history_web.ViewModels;

namespace baseball_history_tests.ViewModels;

public class EmptyStateModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var model = new EmptyStateModel();

        Assert.Equal("No Results Found", model.Title);
        Assert.Null(model.Message);
        Assert.Null(model.Icon);
        Assert.Null(model.ActionUrl);
        Assert.Null(model.ActionText);
    }

    [Fact]
    public void NoPlayers_WithLetter_SetsCorrectMessage()
    {
        var model = EmptyStateModel.NoPlayers("A");

        Assert.Equal("No Players Found", model.Title);
        Assert.Contains("'A'", model.Message!);
        Assert.Contains("last names", model.Message!);
        Assert.NotNull(model.Icon);
    }

    [Fact]
    public void NoPlayers_WithoutLetter_SetsGenericMessage()
    {
        var model = EmptyStateModel.NoPlayers();

        Assert.Equal("No Players Found", model.Title);
        Assert.Equal("Try adjusting your search criteria.", model.Message);
    }

    [Fact]
    public void NoTeams_SetsCorrectProperties()
    {
        var model = EmptyStateModel.NoTeams();

        Assert.Equal("No Teams Found", model.Title);
        Assert.Equal("Try adjusting your search criteria.", model.Message);
        Assert.NotNull(model.Icon);
    }

    [Fact]
    public void NoSearchResults_IncludesQuery()
    {
        var model = EmptyStateModel.NoSearchResults("test query");

        Assert.Equal("No Results", model.Title);
        Assert.Contains("test query", model.Message!);
        Assert.NotNull(model.Icon);
    }

    [Fact]
    public void NoStats_SetsCorrectProperties()
    {
        var model = EmptyStateModel.NoStats();

        Assert.Equal("No Statistics Available", model.Title);
        Assert.Contains("criteria", model.Message!);
        Assert.NotNull(model.Icon);
    }

    [Fact]
    public void CanSetCustomProperties()
    {
        var model = new EmptyStateModel
        {
            Title = "Custom Title",
            Message = "Custom message",
            Icon = "&#9889;",
            ActionUrl = "/custom",
            ActionText = "Take Action"
        };

        Assert.Equal("Custom Title", model.Title);
        Assert.Equal("Custom message", model.Message);
        Assert.Equal("&#9889;", model.Icon);
        Assert.Equal("/custom", model.ActionUrl);
        Assert.Equal("Take Action", model.ActionText);
    }
}

public class AlphabetNavModelTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var model = new AlphabetNavModel();

        Assert.Null(model.CurrentLetter);
        Assert.NotNull(model.AvailableLetters);
        Assert.Empty(model.AvailableLetters);
        Assert.Equal("#content", model.Target);
        Assert.NotNull(model.QueryParams);
        Assert.Empty(model.QueryParams);
    }

    [Fact]
    public void GetLetterUrl_ReturnsCorrectUrl()
    {
        var model = new AlphabetNavModel
        {
            BaseUrl = "/Players"
        };

        var url = model.GetLetterUrl("B");

        Assert.Contains("letter=B", url);
        Assert.Contains("page=1", url);
        Assert.StartsWith("/Players?", url);
    }

    [Fact]
    public void GetLetterUrl_PreservesExistingQueryParams()
    {
        var model = new AlphabetNavModel
        {
            BaseUrl = "/Players",
            QueryParams = new Dictionary<string, string> { { "sort", "name" } }
        };

        var url = model.GetLetterUrl("C");

        Assert.Contains("letter=C", url);
        Assert.Contains("sort=name", url);
        Assert.Contains("page=1", url);
    }

    [Fact]
    public void GetLetterUrl_OverridesExistingPage()
    {
        var model = new AlphabetNavModel
        {
            BaseUrl = "/Players",
            QueryParams = new Dictionary<string, string> { { "page", "5" } }
        };

        var url = model.GetLetterUrl("D");

        // Should reset to page 1, not keep page 5
        Assert.Contains("page=1", url);
    }

    [Fact]
    public void ForPlayers_CreatesCorrectModel()
    {
        var letters = new List<char> { 'A', 'B', 'C' };
        var model = AlphabetNavModel.ForPlayers('B', letters);

        Assert.Equal('B', model.CurrentLetter);
        Assert.Equal(letters, model.AvailableLetters);
        Assert.Equal("/Players", model.BaseUrl);
        Assert.Equal("#player-list", model.Target);
    }

    [Fact]
    public void ForPlayers_WithNullCurrentLetter()
    {
        var letters = new List<char> { 'A', 'B', 'C' };
        var model = AlphabetNavModel.ForPlayers(null, letters);

        Assert.Null(model.CurrentLetter);
        Assert.Equal(letters, model.AvailableLetters);
    }

    [Fact]
    public void GetLetterUrl_EscapesSpecialCharacters()
    {
        var model = new AlphabetNavModel
        {
            BaseUrl = "/Test",
            QueryParams = new Dictionary<string, string> { { "filter", "a&b" } }
        };

        var url = model.GetLetterUrl("A");

        Assert.Contains("a%26b", url);
    }
}