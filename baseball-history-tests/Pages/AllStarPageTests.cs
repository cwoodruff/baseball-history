using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class AllStarPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_ListsYearsWithBadges()
    {
        var html = await GetStringAsync("/AllStar");

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("href=\"/AllStar/1933\"", html);
        Assert.Contains("href=\"/AllStar/1959\"", html);
        Assert.Contains("Two games", html);
        Assert.Contains("East-West Game", html);
    }

    [Fact]
    public async Task Index_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/AllStar");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("/AllStar/1933", html);
    }

    [Fact]
    public async Task Year1959_ShowsTwoMlbGames()
    {
        var html = await GetStringAsync("/AllStar/1959");

        Assert.Contains("Game 1", html);
        Assert.Contains("Game 2", html);
        Assert.Contains("Two All-Star Games were played each season from 1959 through 1962", html);
        Assert.Contains("Willie Mays", html);
    }

    [Fact]
    public async Task Year1955_ShowsSingleGameRosters()
    {
        var html = await GetStringAsync("/AllStar/1955");

        Assert.DoesNotContain("Game 2", html);
        Assert.Contains("AL Roster", html);
        Assert.Contains("NL Roster", html);
        // Aaron's Milwaukee Braves club links to the team season page
        Assert.Contains("Hank Aaron", html);
        Assert.Contains("/Teams/Season/ML1/NL/1955", html);
    }

    [Fact]
    public async Task Year1943_IncludesEastWestGame()
    {
        var html = await GetStringAsync("/AllStar/1943");

        Assert.Contains("East-West Game", html);
        Assert.Contains("East Roster", html);
        Assert.Contains("West Roster", html);
        // AL-NL game is present too and sorts first
        Assert.Contains("All-Star Game", html);
        // East-West copy links into the Negro Leagues hub
        Assert.Contains("/NegroLeagues", html);
    }

    [Fact]
    public async Task Year_Navigation_LinksAdjacentSeasons()
    {
        var html = await GetStringAsync("/AllStar/1959");

        Assert.Contains("/AllStar/1958", html);
        Assert.Contains("/AllStar/1960", html);
    }

    [Fact]
    public async Task Year_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/AllStar/1959");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Game 2", html);
    }

    [Fact]
    public async Task Year_NoAllStarGame_Returns404()
    {
        var response = await Client.GetAsync("/AllStar/1900");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlayerPage_AllStarYears_LinkToRosters()
    {
        // Ruth was selected in 1933 and 1934
        var html = await GetStringAsync("/Players/ruthba01");

        Assert.Contains("href=\"/AllStar/1933\"", html);
        Assert.Contains("href=\"/AllStar/1934\"", html);
    }

    [Fact]
    public async Task PlayerPage_TwoGameYears_CountSeasonsNotGames()
    {
        // Aaron: 25 games across 21 seasons (1955-1975, incl. 1959-1962 doubles)
        var html = await GetStringAsync("/Players/aaronha01");

        Assert.Contains("21 x All-Star", html);
        Assert.Contains("href=\"/AllStar/1959\"", html);
    }
}
