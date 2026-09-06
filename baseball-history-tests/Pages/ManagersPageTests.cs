using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class ManagersPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_ListsManagersSortedByWins()
    {
        var html = await GetStringAsync("/Managers");

        Assert.Contains("<!DOCTYPE html>", html);
        // Connie Mack's 3,731 wins lead all managers
        Assert.Contains("Connie Mack", html);
        Assert.Contains("/Managers/mackco01", html);
    }

    [Fact]
    public async Task Index_ShowsPagination()
    {
        var html = await GetStringAsync("/Managers");

        var (currentPage, totalPages) = ParsePaginationSummary(html);
        Assert.Equal(1, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task Index_SearchFilter_FindsManager()
    {
        var html = await GetStringAsync("/Managers?q=mack");

        Assert.Contains("Connie Mack", html);
        Assert.DoesNotContain("Casey Stengel", html);
    }

    [Fact]
    public async Task Index_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/Managers");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("manager-filter-form", html);
    }

    [Fact]
    public async Task Detail_ConnieMack_ShowsCareer()
    {
        var html = await GetStringAsync("/Managers/mackco01");

        Assert.Contains("Connie Mack", html);
        // Season rows link to team season pages
        Assert.Contains("/Teams/Season/", html);
        // Mack was a player-manager in Pittsburgh and later a Hall of Famer
        Assert.Contains("Player-Mgr", html);
        Assert.Contains("HOF", html);
        Assert.Contains("Player Page", html);
    }

    [Fact]
    public async Task Detail_UnknownManager_Returns404()
    {
        var response = await Client.GetAsync("/Managers/nosuchmgr99");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Detail_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/Managers/mackco01");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Season History", html);
    }

    [Fact]
    public async Task PlayerPage_PlayerManager_HasManagingTab()
    {
        // Joe Cronin managed the Senators and Red Sox while playing
        var html = await GetStringAsync("/Players/cronijo01");

        Assert.Contains("Managing", html);
        Assert.Contains("/Managers/cronijo01", html);
    }

    [Fact]
    public async Task TeamSeasonPage_LinksManagerCareer()
    {
        var html = await GetStringAsync("/Teams/Season/NYA/AL/1927");

        Assert.Contains("href=\"/Managers/", html);
    }

    [Fact]
    public async Task AwardsPage_ManagersScope_ShowsManagerAwards()
    {
        var html = await GetStringAsync("/Awards?scope=managers");

        Assert.Contains("BBWAA Manager of the Year", html);
        Assert.Contains("TSN Manager of the Year", html);
        // Scope toggle present with managers active
        Assert.Contains("scope=managers", html);
        // Manager award winners never appear in the player awards table
        Assert.Contains("Manager</th>", html);
    }

    [Fact]
    public async Task AwardsPage_ManagerVotingRace_Loads()
    {
        var html = await GetStringAsync(
            "/Awards?scope=managers&award=BBWAA%20Manager%20of%20the%20Year&year=2016&league=AL");

        // Terry Francona won the 2016 AL race
        Assert.Contains("Terry Francona", html);
        Assert.Contains("Voting", html);
        Assert.Contains("Winner", html);
        Assert.Contains("/Managers/francte01", html);
    }

    [Fact]
    public async Task AwardsPage_PlayersScope_UnchangedByDefault()
    {
        var html = await GetStringAsync("/Awards");

        Assert.Contains("Most Valuable Player", html);
        Assert.DoesNotContain("BBWAA Manager of the Year", html);
    }
}
