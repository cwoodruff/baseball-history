using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

public class ApiSmokeTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Search_WithShortQuery_ReturnsBadRequest()
    {
        var response = await Client.GetAsync("/api/search?q=R");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithValidQuery_ReturnsPlayersAndCounts()
    {
        using var document = await GetJsonAsync("/api/search?q=Ruth");
        var root = document.RootElement;

        Assert.Equal("Ruth", root.GetProperty("query").GetString());
        Assert.True(root.GetProperty("players").GetArrayLength() > 0);
        Assert.True(root.GetProperty("totalPlayerCount").GetInt32() >= root.GetProperty("players").GetArrayLength());
    }

    [Fact]
    public async Task PlayerDetail_WithValidPlayerId_ReturnsCareerData()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01");
        var root = document.RootElement;

        Assert.Equal("ruthba01", root.GetProperty("playerId").GetString());
        Assert.Equal("Babe", root.GetProperty("firstName").GetString());
        Assert.NotEqual(JsonValueKind.Null, root.GetProperty("careerBatting").ValueKind);
        Assert.True(root.GetProperty("teams").GetArrayLength() > 0);
    }

    [Fact]
    public async Task PlayerBatting_WithValidPlayerId_ReturnsSeasonHistory()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01/batting");
        var seasons = document.RootElement;

        Assert.Equal(JsonValueKind.Array, seasons.ValueKind);
        Assert.True(seasons.GetArrayLength() > 0);
        Assert.False(string.IsNullOrWhiteSpace(seasons[0].GetProperty("teamId").GetString()));
        Assert.True(seasons[0].GetProperty("year").GetInt32() >= seasons[seasons.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task PlayerFielding_WithValidPlayerId_ReturnsParsedSeasonStats()
    {
        // Po/A/E/Dp are stored as strings and parsed in memory; this exercises that path
        // end-to-end and guards the GetPlayerFielding projection from regressing.
        using var document = await GetJsonAsync("/api/players/ruthba01/fielding");
        var seasons = document.RootElement;

        Assert.Equal(JsonValueKind.Array, seasons.ValueKind);
        Assert.True(seasons.GetArrayLength() > 0);

        var first = seasons[0];
        Assert.False(string.IsNullOrWhiteSpace(first.GetProperty("position").GetString()));
        Assert.True(first.GetProperty("year").GetInt32() >= seasons[seasons.GetArrayLength() - 1].GetProperty("year").GetInt32());
        Assert.All(
            seasons.EnumerateArray(),
            season =>
            {
                Assert.True(season.GetProperty("putOuts").GetInt32() >= 0);
                Assert.True(season.GetProperty("assists").GetInt32() >= 0);
                Assert.True(season.GetProperty("errors").GetInt32() >= 0);
                Assert.True(season.GetProperty("doublePlays").GetInt32() >= 0);
            });
    }

    [Fact]
    public async Task TeamSeason_WithValidIdentifiers_ReturnsRosterData()
    {
        using var document = await GetJsonAsync("/api/teams/seasons/NYA/AL/2020");
        var root = document.RootElement;

        Assert.Equal("NYA", root.GetProperty("teamId").GetString());
        Assert.Equal(2020, root.GetProperty("year").GetInt32());
        Assert.True(root.GetProperty("batters").GetArrayLength() > 0);
        Assert.True(root.GetProperty("pitchers").GetArrayLength() > 0);
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
