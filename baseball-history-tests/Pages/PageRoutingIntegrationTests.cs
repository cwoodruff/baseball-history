using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PageRoutingIntegrationTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Healthz_ReadyEndpoint_ReturnsOk()
    {
        var response = await Client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Alive_LivenessEndpoint_ReturnsOk()
    {
        var response = await Client.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Players_FullPage_RendersShellAndPlayersHost()
    {
        var html = await GetStringAsync("/Players?letter=A");

        AssertFullPageShell(html);
        Assert.Contains("id=\"players-content\"", html);
        Assert.Contains("id=\"player-list\"", html);
        Assert.Contains(">Players</h1>", html);
        Assert.Contains("class=\"alphabet-nav\"", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
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
        Assert.Contains("class=\"alphabet-nav\"", html);
        Assert.Contains("hx-target=\"#players-content\"", html);
    }

    [Fact]
    public async Task Players_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Players?letter=C", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"players-content\"", html);
    }

    [Fact]
    public async Task Players_FullPage_WiresAlphabetNavToPlayersContent()
    {
        var html = await GetStringAsync("/Players?letter=A");

        AssertFullPageShell(html);
        Assert.Contains("class=\"letter-link active ", html);
        Assert.Contains("hx-get=\"/Players?letter=B&amp;page=1\"", html);
        Assert.Contains("hx-target=\"#players-content\"", html);
        Assert.Contains("hx-push-url=\"true\"", html);
    }

    [Fact]
    public async Task Players_Htmx_PageTwo_PreservesPaginationAndModalContracts()
    {
        var html = await GetHtmxStringAsync("/Players?letter=A&page=2");

        AssertPartialResponse(html);
        Assert.Contains("Page 2 of", html);
        Assert.Contains("hx-get=\"/Players?letter=A&amp;page=1\"", html);
        Assert.Contains("hx-target=\"#players-content\"", html);
        Assert.Contains("hx-boost=\"false\"", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
    }

    [Fact]
    public async Task Players_ModalRoute_ReturnsModalPartial()
    {
        var html = await GetStringAsync("/Players/Modal/ruthba01");

        AssertPartialResponse(html);
        Assert.Contains("id=\"playerModal\"", html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("Player Info", html);
        Assert.Contains("Career Batting", html);
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
        Assert.Contains("hx-target=\"#modal-container\"", html);
        Assert.Contains("search-results", html);
    }

    [Fact]
    public async Task Search_AllResultsHandler_ReturnsModalPartial()
    {
        var html = await GetStringAsync("/Search?handler=AllResults&q=Ruth");

        AssertPartialResponse(html);
        Assert.Contains("id=\"searchAllResultsModal\"", html);
        Assert.Contains("Search Results for", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
    }

    [Fact]
    public async Task Search_TeamQuery_PreservesFranchiseNavigationContracts()
    {
        var html = await GetStringAsync("/Search?q=Yankees");

        AssertPartialResponse(html);
        Assert.Contains("TEAMS", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
        Assert.Contains("data-team=\"NYA\"", html);
    }

    [Fact]
    public async Task Search_AllResultsTeamQuery_DismissesModalOnNavigation()
    {
        var html = await GetStringAsync("/Search?handler=AllResults&q=Yankees");

        AssertPartialResponse(html);
        Assert.Contains("id=\"searchAllResultsModal\"", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
        Assert.Contains("data-bs-dismiss=\"modal\"", html);
    }

    [Fact]
    public async Task Index_FullPage_RendersHomepageLinksAndModalContracts()
    {
        var html = await GetStringAsync("/");

        AssertFullPageShell(html);
        Assert.Contains("Browse Players", html);
        Assert.Contains("View Teams", html);
        Assert.Contains("href=\"/Stats/Batting?stat=hr\"", html);
        Assert.Contains("href=\"/HallOfFame\"", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
    }

    [Theory]
    [InlineData("/About", ">About</h1>", "View Source on GitHub")]
    [InlineData("/ApiDocs", ">REST API</h1>", "/openapi/v1.json")]
    [InlineData("/McpDocs", ">MCP Server</h1>", "search_players")]
    [InlineData("/Privacy", ">Privacy Policy</h1>", "do not collect, store, sell, or share")]
    [InlineData("/Health", ">Health Check</h1>", "Database Status")]
    [InlineData("/Error", ">Error</h1>", "Development Mode")]
    public async Task SupportPages_FullPage_RenderWithinShell(string route, string headingMarker, string contentMarker)
    {
        var html = await GetStringAsync(route);

        AssertFullPageShell(html);
        Assert.Contains(headingMarker, html);
        Assert.Contains(contentMarker, html);
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
        Assert.Contains("id=\"teams-content\"", html);
        Assert.Contains("id=\"team-list\"", html);
        Assert.Contains("Teams &amp; Franchises", html);
    }

    [Fact]
    public async Task Teams_NonBoostedHtmx_ReturnsTeamsContentPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Teams?league=AL");

        AssertPartialResponse(html);
        // the partial swaps into #teams-content, so it contains the list host but not the outer shell
        Assert.DoesNotContain("id=\"teams-content\"", html);
        Assert.Contains("id=\"team-list\"", html);
        Assert.Contains("id=\"team-filter-form\"", html);
        Assert.Contains("Active Franchises", html);
        Assert.DoesNotContain("<rhx-badge", html);
    }

    [Fact]
    public async Task Teams_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Teams?league=NL", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"team-list\"", html);
    }

    [Fact]
    public async Task Teams_Franchise_FullPage_RendersShellAndSeasonRouting()
    {
        var html = await GetStringAsync("/Teams/Franchise/NYY");

        AssertFullPageShell(html);
        Assert.Contains("id=\"franchise-content\"", html);
        Assert.Contains("Season History", html);
        Assert.Contains("href=\"/Teams/Season/NYA/AL/2025\"", html);
        Assert.DoesNotContain("<rhx-badge", html);
    }

    [Fact]
    public async Task Teams_Franchise_NonBoostedHtmx_ReturnsSeasonHistoryPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Teams/Franchise/NYY");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"franchise-content\"", html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
        Assert.Contains("Season History", html);
        Assert.Contains("href=\"/Teams/Season/NYA/AL/2025\"", html);
    }

    [Fact]
    public async Task Teams_Franchise_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Teams/Franchise/NYY", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"franchise-content\"", html);
    }

    [Fact]
    public async Task Teams_Season_FullPage_RendersShellAndPlayerModalContracts()
    {
        var html = await GetStringAsync("/Teams/Season/NYA/AL/2025");

        AssertFullPageShell(html);
        Assert.Contains("New York Yankees", html);
        Assert.Contains("Other Seasons", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
        Assert.DoesNotContain("<rhx-badge", html);
    }

    [Fact]
    public async Task Teams_Season_NonBoostedHtmx_ReturnsSeasonPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Teams/Season/NYA/AL/2025");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("New York Yankees", html);
        Assert.Contains("Other Seasons", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
    }

    [Fact]
    public async Task Teams_Season_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Teams/Season/NYA/AL/2025", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("Other Seasons", html);
        Assert.Contains("id=\"modal-container\"", html);
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

    [Fact]
    public async Task Compare_NonBoostedHtmx_ReturnsCompareMainPartial()
    {
        var html = await GetHtmxStringAsync("/Compare?player1=ruthba01&player2=cobbty01");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"compare-content\"", html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
        Assert.Contains("id=\"compare-tables\"", html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("Ty Cobb", html);
    }

    [Fact]
    public async Task Compare_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Compare?player1=ruthba01&player2=cobbty01", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"compare-content\"", html);
        Assert.Contains("id=\"compare-tables\"", html);
    }

    [Fact]
    public async Task Compare_NonBoostedHtmx_WiresPlayerModalContracts()
    {
        var html = await GetHtmxStringAsync("/Compare?player1=ruthba01&player2=cobbty01");

        AssertPartialResponse(html);
        Assert.Contains("hx-get=\"/Players/Modal/ruthba01\"", html);
        Assert.Contains("hx-get=\"/Players/Modal/cobbty01\"", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
    }

    [Fact]
    public async Task Compare_NonBoostedHtmx_WiresDualSearchContracts()
    {
        var html = await GetHtmxStringAsync("/Compare");

        AssertPartialResponse(html);
        Assert.Contains("id=\"search-results-1\"", html);
        Assert.Contains("id=\"search-results-2\"", html);
        Assert.Contains("handler=Search&amp;side=1", html);
        Assert.Contains("handler=Search&amp;side=2", html);
    }

    [Fact]
    public async Task Salaries_FullPage_RendersShellAndDollarFormattedAmounts()
    {
        var html = await GetStringAsync("/Salaries?year=2016&team=NYA");

        AssertFullPageShell(html);
        Assert.Contains(">Salary Explorer</h1>", html);
        Assert.Contains("id=\"salary-list\"", html);
        Assert.Contains("$222,997,792", html);
        Assert.Contains("$25,000,000", html);
    }

    [Fact]
    public async Task Salaries_NonBoostedHtmx_ReturnsSalaryListPartialWithDollarFormattedAmounts()
    {
        var html = await GetHtmxStringAsync("/Salaries?year=2016&team=NYA");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"filter-form\"", html);
        Assert.Contains("Player Salaries", html);
        Assert.Contains("$222,997,792", html);
        Assert.Contains("$25,000,000", html);
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
