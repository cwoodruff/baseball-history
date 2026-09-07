using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

public class PlayerApiContractTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PlayerPitching_WithTwoWayPlayer_ReturnsPitchingSeasons()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01/pitching");
        var seasons = document.RootElement;

        Assert.Equal(JsonValueKind.Array, seasons.ValueKind);
        Assert.True(seasons.GetArrayLength() >= 10);
        Assert.Equal(1933, seasons[0].GetProperty("year").GetInt32());
        Assert.Equal("NYA", seasons[0].GetProperty("teamId").GetString());
        Assert.True(seasons[0].GetProperty("year").GetInt32() > seasons[seasons.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task PlayerAwards_WithAwardWinner_ReturnsDescendingAwardHistory()
    {
        using var document = await GetJsonAsync("/api/players/troutmi01/awards");
        var awards = document.RootElement;

        Assert.Equal(JsonValueKind.Array, awards.ValueKind);
        Assert.True(awards.GetArrayLength() > 10);
        Assert.Equal(2022, awards[0].GetProperty("year").GetInt32());
        Assert.Contains(awards.EnumerateArray(), award =>
            award.GetProperty("awardId").GetString() == "Most Valuable Player" &&
            award.GetProperty("year").GetInt32() == 2016);
    }

    [Fact]
    public async Task PlayerPostseasonBatting_WithOctoberRegular_ReturnsSeriesHistory()
    {
        using var document = await GetJsonAsync("/api/players/berrayo01/postseason/batting");
        var series = document.RootElement;

        Assert.Equal(JsonValueKind.Array, series.ValueKind);
        Assert.True(series.GetArrayLength() > 10);
        Assert.Equal("WS", series[0].GetProperty("round").GetString());
        Assert.Equal(1963, series[0].GetProperty("year").GetInt32());
        Assert.True(series[0].GetProperty("year").GetInt32() > series[series.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task PlayerPostseasonPitching_WithOctoberCloser_ReturnsSeriesHistory()
    {
        using var document = await GetJsonAsync("/api/players/riverma01/postseason/pitching");
        var series = document.RootElement;

        Assert.Equal(JsonValueKind.Array, series.ValueKind);
        Assert.True(series.GetArrayLength() > 20);
        Assert.Equal(2011, series[0].GetProperty("year").GetInt32());
        Assert.Equal("ALDS1", series[0].GetProperty("round").GetString());
        Assert.True(series[0].GetProperty("year").GetInt32() > series[series.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task PlayerPostseasonBatting_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/postseason/batting");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlayerPostseasonPitching_InvalidPlayerId_Returns404()
    {
        var response = await Client.GetAsync("/api/players/INVALIDXXX/postseason/pitching");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
