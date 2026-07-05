using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PlayerDetailsPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    private const string TwoWayPlayerId = "ruthba01";

    [Fact]
    public async Task PlayerDetails_KnownPlayer_RendersFullPage()
    {
        var html = await GetStringAsync($"/Players/{TwoWayPlayerId}");

        Assert.Contains("Babe Ruth", html);
        Assert.Contains("id=\"player-details\"", html);
        Assert.Contains("breadcrumb", html);
        Assert.Contains("href=\"/Players\"", html);
    }

    [Fact]
    public async Task PlayerDetails_TwoWayPlayer_ShowsBattingAndPitchingTabs()
    {
        var html = await GetStringAsync($"/Players/{TwoWayPlayerId}");

        Assert.Contains($"batting-tab-{TwoWayPlayerId}", html);
        Assert.Contains($"pitching-tab-{TwoWayPlayerId}", html);
    }

    [Fact]
    public async Task PlayerDetails_HallOfFamer_LinksHofBadgeToInductionClass()
    {
        var html = await GetStringAsync($"/Players/{TwoWayPlayerId}");

        Assert.Contains("href=\"/HallOfFame?year=1936\"", html);
    }

    [Fact]
    public async Task PlayerDetails_UnknownPlayer_Returns404()
    {
        var response = await Client.GetAsync("/Players/nosuchplayer99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlayerDetails_TeamsCard_LinksToFranchisePages()
    {
        var html = await GetStringAsync($"/Players/{TwoWayPlayerId}");

        Assert.Contains("href=\"/Teams/Franchise/NYY\"", html);

        var linked = await Client.GetAsync("/Teams/Franchise/NYY");
        Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
    }

    [Fact]
    public async Task PlayerDetails_SeasonRows_LinkToTeamSeasonPages()
    {
        var html = await GetStringAsync($"/Players/{TwoWayPlayerId}");

        // Ruth's 1927 Yankees season should link to the team-season page
        Assert.Contains("href=\"/Teams/Season/NYA/AL/1927\"", html);

        // ... and the linked page must actually resolve
        var linked = await Client.GetAsync("/Teams/Season/NYA/AL/1927");
        Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
    }

    [Fact]
    public async Task PlayersIndex_StillRoutesToPlayerBrowser()
    {
        var response = await Client.GetAsync("/Players");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"players-content\"", html);
    }

    [Fact]
    public async Task PlayerModal_ContainsFullPageLink()
    {
        var html = await GetStringAsync($"/Players/Modal/{TwoWayPlayerId}");

        Assert.Contains($"href=\"/Players/{TwoWayPlayerId}\"", html);
        Assert.Contains("View Full Page", html);
    }

    [Fact]
    public async Task PlayerModal_TeamAndSeasonLinksPresent()
    {
        var html = await GetStringAsync($"/Players/Modal/{TwoWayPlayerId}");

        Assert.Contains("href=\"/Teams/Franchise/", html);
        Assert.Contains("href=\"/Teams/Season/", html);
    }
}
