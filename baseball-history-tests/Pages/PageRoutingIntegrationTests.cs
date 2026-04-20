using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PageRoutingIntegrationTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Players_FullPage_RendersShellAndPlayersHost()
    {
        var html = await GetStringAsync("/Players?letter=A");

        AssertFullPageShell(html);
        Assert.Contains("id=\"players-content\"", html);
        Assert.Contains("id=\"player-list\"", html);
        Assert.Contains(">Players</h1>", html);
    }

    [Fact]
    public async Task Players_NonBoostedHtmx_ReturnsPlayersPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Players?letter=B");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"players-content\"", html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
        Assert.Contains("id=\"player-list\"", html);
        Assert.Contains(">Players</h1>", html);
    }

    [Fact]
    public async Task Players_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Players?letter=C", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"players-content\"", html);
    }

    [Fact]
    public async Task Players_ModalRoute_ReturnsModalPartial()
    {
        var html = await GetStringAsync("/Players/Modal/ruthba01");

        AssertPartialResponse(html);
        Assert.Contains("id=\"playerModal\"", html);
        Assert.Contains("Babe Ruth", html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
    }

    [Fact]
    public async Task Search_ShortQuery_ReturnsEmptyDropdownPartial()
    {
        var html = await GetStringAsync("/Search?q=R");

        AssertPartialResponse(html);
        Assert.DoesNotContain("PLAYERS", html);
        Assert.DoesNotContain("TEAMS", html);
        Assert.DoesNotContain("View all results", html);
    }

    [Fact]
    public async Task Search_Query_ReturnsDropdownResultsPartial()
    {
        var html = await GetStringAsync("/Search?q=Ruth");

        AssertPartialResponse(html);
        Assert.Contains("PLAYERS", html);
        Assert.Contains("View all results for", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
    }

    [Fact]
    public async Task Search_AllResultsHandler_ReturnsModalPartial()
    {
        var html = await GetStringAsync("/Search?handler=AllResults&q=Ruth");

        AssertPartialResponse(html);
        Assert.Contains("id=\"searchAllResultsModal\"", html);
        Assert.Contains("Search Results for", html);
    }

    [Fact]
    public async Task StatsBatting_FullPage_RendersShellFilterAndLeaderboardHost()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=hr");

        AssertFullPageShell(html);
        Assert.Contains("id=\"filter-form\"", html);
        Assert.Contains("id=\"leaderboard\"", html);
        Assert.Contains(">Batting Leaders</h1>", html);
    }

    [Fact]
    public async Task StatsBatting_NonBoostedHtmx_ReturnsLeaderboardPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Stats/Batting?stat=avg");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"filter-form\"", html);
        Assert.DoesNotContain("id=\"leaderboard\"", html);
        Assert.Contains("table-baseball", html);
        Assert.Contains("Batting Leaders - Batting Average", html);
    }

    [Fact]
    public async Task StatsBatting_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Stats/Batting?stat=ops", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"filter-form\"", html);
        Assert.Contains("id=\"leaderboard\"", html);
    }

    [Fact]
    public async Task Teams_FullPage_RendersShellAndTeamListHost()
    {
        var html = await GetStringAsync("/Teams");

        AssertFullPageShell(html);
        Assert.Contains("id=\"team-list\"", html);
        Assert.Contains("Teams & Franchises", html);
    }

    [Fact]
    public async Task Teams_NonBoostedHtmx_ReturnsTeamListPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Teams?league=AL");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"team-list\"", html);
        Assert.Contains("Active Franchises", html);
    }

    [Fact]
    public async Task Teams_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Teams?league=NL", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"team-list\"", html);
    }

    [Fact]
    public async Task Compare_FullPage_WithoutPlayers_RendersDualSearchHosts()
    {
        var html = await GetStringAsync("/Compare");

        AssertFullPageShell(html);
        Assert.Contains("id=\"search-results-1\"", html);
        Assert.Contains("id=\"search-results-2\"", html);
        Assert.DoesNotContain("id=\"compare-tables\"", html);
    }

    [Fact]
    public async Task Compare_SearchHandler_ReturnsResultsPartialAndPreservesOtherSelection()
    {
        var html = await GetStringAsync("/Compare?handler=Search&q=Ruth&side=1&player2=cobbty01");

        AssertPartialResponse(html);
        Assert.Contains("list-group-item", html);
        Assert.Contains("player2=cobbty01", html);
        Assert.Contains("Select", html);
    }

    [Fact]
    public async Task Compare_FullPage_WithTwoPlayers_RendersComparisonTables()
    {
        var html = await GetStringAsync("/Compare?player1=ruthba01&player2=cobbty01");

        AssertFullPageShell(html);
        Assert.Contains("id=\"compare-tables\"", html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("Ty Cobb", html);
    }

    private static void AssertFullPageShell(string html)
    {
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("hx-boost=\"true\"", html);
        Assert.Contains("class=\"search-container\"", html);
        Assert.Contains("id=\"search-results\"", html);
        Assert.Contains("id=\"modal-container\"", html);
    }

    private static void AssertPartialResponse(string html)
    {
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        Assert.DoesNotContain("hx-boost=\"true\"", html);
    }
}
