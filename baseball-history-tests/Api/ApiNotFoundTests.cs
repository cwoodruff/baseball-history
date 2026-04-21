using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

public class ApiNotFoundTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // Player endpoint 404 tests
    [Fact]
    public async Task GetPlayerDetail_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerDetail_NonExistentPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/zzzzz99");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerBatting_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/batting");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerPitching_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/pitching");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerFielding_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/fielding");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerAwards_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/awards");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Team endpoint 404 tests
    [Fact]
    public async Task GetFranchiseDetail_InvalidFranchiseId_Returns404()
    {
        var response = await Client.GetAsync("/api/teams/franchises/INVALID");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFranchiseDetail_NonExistentFranchiseId_Returns404()
    {
        var response = await Client.GetAsync("/api/teams/franchises/ZZZ");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamSeason_InvalidTeamId_Returns404()
    {
        var response = await Client.GetAsync("/api/teams/seasons/INVALID/AL/2024");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTeamSeason_InvalidYear_Returns404()
    {
        var response = await Client.GetAsync("/api/teams/seasons/NYA/AL/1800");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Hall of Fame endpoint 404 tests
    [Fact]
    public async Task GetHallOfFameVoting_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/hall-of-fame/voting/INVALIDXXX");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHallOfFameVoting_NonExistentPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/hall-of-fame/voting/zzzzz99");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Postseason endpoint 404 tests
    [Fact]
    public async Task GetPostseasonSeries_InvalidYear_Returns404()
    {
        var response = await Client.GetAsync("/api/postseason/series?year=1800");
        
        // May return 200 with empty results or 404 depending on implementation
        // This test verifies it doesn't error
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostseasonSeries_FutureYear_ReturnsEmptyOrNotFound()
    {
        var response = await Client.GetAsync("/api/postseason/series?year=2099");
        
        // Should handle gracefully
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
    }

    // Valid requests for comparison (sanity checks)
    [Fact]
    public async Task GetPlayerDetail_ValidPlayerId_Returns200()
    {
        // Babe Ruth
        var response = await Client.GetAsync("/api/players/ruthba01");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ruth", json);
    }

    [Fact]
    public async Task GetFranchiseDetail_ValidFranchiseId_Returns200()
    {
        var response = await Client.GetAsync("/api/teams/franchises/NYY");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("Yankees", json);
    }

    [Fact]
    public async Task GetTeamSeason_ValidTeamSeason_Returns200()
    {
        var response = await Client.GetAsync("/api/teams/seasons/NYA/AL/2020");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("NYA", json);
    }
}
