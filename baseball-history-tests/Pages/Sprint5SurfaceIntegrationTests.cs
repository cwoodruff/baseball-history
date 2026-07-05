using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class Sprint5SurfaceIntegrationTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Home_FullPage_RendersShellAndHomepageContracts()
    {
        var html = await GetStringAsync("/");

        AssertFullPageShell(html);
        Assert.Contains("Baseball History", html);
        Assert.Contains("Browse Players", html);
        Assert.Contains("View Teams", html);
        Assert.Contains("Career Home Run Leaders", html);
        Assert.Contains("Career Wins Leaders", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
        Assert.Contains("href=\"/Stats/Batting?stat=hr\"", html);
        Assert.Contains("href=\"/Stats/Pitching?stat=w\"", html);
    }

    [Fact]
    public async Task Search_Query_ReturnsDropdownPartialWithPlayerContracts()
    {
        var html = await GetStringAsync("/Search?q=Ruth");

        AssertPartialResponse(html);
        Assert.Contains("PLAYERS", html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("hx-get=\"/Players/Modal/ruthba01\"", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
        Assert.Contains("View all results for", html);
        Assert.Contains("Ruth", html);
        Assert.DoesNotContain("id=\"searchAllResultsModal\"", html);
    }

    [Fact]
    public async Task Search_Query_WithHtmxHeaders_StillReturnsDropdownPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Search?q=Ruth", boosted: true);

        AssertPartialResponse(html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("hx-get=\"/Players/Modal/ruthba01\"", html);
        Assert.DoesNotContain("class=\"search-container\"", html);
        Assert.DoesNotContain("id=\"modal-container\"", html);
    }

    [Fact]
    public async Task Search_TeamQuery_ReturnsFranchiseNavigationContracts()
    {
        var html = await GetStringAsync("/Search?q=Yankees");

        AssertPartialResponse(html);
        Assert.Contains("TEAMS", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
        Assert.Contains("data-team=\"NYA\"", html);
        Assert.DoesNotContain("id=\"searchAllResultsModal\"", html);
    }

    [Fact]
    public async Task Search_AllResultsHandler_ReturnsModalPartialWithPlayerAndTeamContracts()
    {
        var html = await GetStringAsync("/Search?handler=AllResults&q=Yankees");

        AssertPartialResponse(html);
        Assert.Contains("id=\"searchAllResultsModal\"", html);
        Assert.Contains("Search Results for", html);
        Assert.Contains("Yankees", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);
        Assert.Contains("data-bs-dismiss=\"modal\"", html);
        Assert.DoesNotContain("class=\"search-container\"", html);
    }

    [Fact]
    public async Task About_FullPage_RendersShellAndRenderedRhxButton()
    {
        var html = await GetStringAsync("/About");

        AssertFullPageShell(html);
        Assert.Contains(">About</h1>", html);
        Assert.Contains("github.com/cwoodruff/baseball-history", html);
        Assert.Contains("View Source on GitHub", html);
        Assert.DoesNotContain("<rhx-button", html);
    }

    [Fact]
    public async Task ApiDocs_FullPage_RendersShellAndApiReferenceLinks()
    {
        var html = await GetStringAsync("/ApiDocs");

        AssertFullPageShell(html);
        Assert.Contains(">REST API</h1>", html);
        Assert.Contains("href=\"/scalar/v1\"", html);
        Assert.Contains("href=\"/openapi/v1.json\"", html);
        Assert.Contains("/api/players", html);
        Assert.Contains("/api/teams/franchises", html);
    }

    [Fact]
    public async Task Privacy_FullPage_RendersShell()
    {
        var html = await GetStringAsync("/Privacy");

        AssertFullPageShell(html);
        Assert.Contains(">Privacy Policy</h1>", html);
        Assert.Contains("do not collect, store, sell, or share", html);
    }

    [Fact]
    public async Task Health_FullPage_RendersShellAndHealthContracts()
    {
        var html = await GetStringAsync("/Health");

        AssertFullPageShell(html);
        Assert.Contains(">Health Check</h1>", html);
        Assert.Contains("Database Status", html);
        Assert.Contains("PostgreSQL database connection is accessible", html);
        Assert.Contains("Sample Team Record", html);
    }

    [Fact]
    public async Task Error_FullPage_RendersShellAndNoStoreHeaders()
    {
        var response = await Client.GetAsync("/Error");
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        AssertFullPageShell(html);
        Assert.Contains(">Rain Delay</h1>", html);
        Assert.Contains("went wrong on our end", html);
        Assert.True(response.Headers.CacheControl?.NoStore, "Expected /Error to be marked no-store.");
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
