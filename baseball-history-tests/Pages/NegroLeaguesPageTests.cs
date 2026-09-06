using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class NegroLeaguesPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Hub_ListsAllSevenLeagues()
    {
        var html = await GetStringAsync("/NegroLeagues");

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Negro National League (I)", html);
        Assert.Contains("Eastern Colored League", html);
        Assert.Contains("American Negro League", html);
        Assert.Contains("East-West League", html);
        Assert.Contains("Negro Southern League", html);
        Assert.Contains("Negro National League (II)", html);
        Assert.Contains("Negro American League", html);
        Assert.Contains("href=\"/NegroLeagues/NNL\"", html);
        Assert.Contains("href=\"/NegroLeagues/NAL\"", html);
        // Transparency framing links
        Assert.Contains("/SurvivingRecords", html);
        Assert.Contains("/About#data-scope", html);
    }

    [Fact]
    public async Task Hub_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/NegroLeagues");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Negro National League", html);
    }

    [Fact]
    public async Task LeagueDetail_NN2_ShowsSeasonsAndClubs()
    {
        var html = await GetStringAsync("/NegroLeagues/NN2");

        Assert.Contains("Negro National League (II)", html);
        Assert.Contains("Homestead Grays", html);
        // Season list spans the league's run and links to season pages
        Assert.Contains("href=\"/NegroLeagues/NN2/1933\"", html);
        Assert.Contains("href=\"/NegroLeagues/NN2/1948\"", html);
        Assert.Contains("/About#data-scope", html);
    }

    [Fact]
    public async Task LeagueDetail_IsCaseInsensitive()
    {
        var html = await GetStringAsync("/NegroLeagues/nn2");

        Assert.Contains("Negro National League (II)", html);
    }

    [Fact]
    public async Task LeagueDetail_UnknownLeague_Returns404()
    {
        var response = await Client.GetAsync("/NegroLeagues/XYZ");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LeagueDetail_WhiteMajorLeague_Returns404()
    {
        // The hub covers the seven recognized Negro Leagues only
        var response = await Client.GetAsync("/NegroLeagues/AL");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LeagueSeason_1943NN2_ShowsStandingsAndLeaders()
    {
        var html = await GetStringAsync("/NegroLeagues/NN2/1943");

        // Standings with the pennant-winning Homestead Grays linked to the team season page
        Assert.Contains("Homestead Grays", html);
        Assert.Contains("Pennant", html);
        Assert.Contains("/Teams/Season/HG/NN2/1943", html);
        // Leaders sections
        Assert.Contains("Batting Average", html);
        Assert.Contains("Home Runs", html);
        Assert.Contains("ERA", html);
        // Josh Gibson's 1943 season leads the league
        Assert.Contains("/Players/gibsojo99", html);
        // Data scope note required alongside leaderboards
        Assert.Contains("/About#data-scope", html);
    }

    [Fact]
    public async Task LeagueSeason_YearNavigation_LinksAdjacentSeasons()
    {
        var html = await GetStringAsync("/NegroLeagues/NN2/1943");

        Assert.Contains("/NegroLeagues/NN2/1942", html);
        Assert.Contains("/NegroLeagues/NN2/1944", html);
    }

    [Fact]
    public async Task LeagueSeason_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/NegroLeagues/NN2/1943");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Homestead Grays", html);
    }

    [Fact]
    public async Task LeagueSeason_YearOutsideLeagueRun_Returns404()
    {
        var response = await Client.GetAsync("/NegroLeagues/NNL/1955");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SurvivingRecords_LinksToHub()
    {
        var html = await GetStringAsync("/SurvivingRecords");

        Assert.Contains("href=\"/NegroLeagues\"", html);
        Assert.Contains("Browse the record", html);
    }

    [Fact]
    public async Task Navigation_IncludesNegroLeaguesLink()
    {
        var html = await GetStringAsync("/NegroLeagues");

        Assert.Contains(">Negro Leagues</a>", html);
    }
}
