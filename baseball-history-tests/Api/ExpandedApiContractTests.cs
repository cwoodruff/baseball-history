using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

/// <summary>
/// Contracts for the endpoints that expose web-only data: player list filters,
/// franchise filters, managers, All-Star games, the Negro Leagues hub, This Day
/// in Baseball, and the player Managing/All-Star/advanced batting tabs.
/// </summary>
public class ExpandedApiContractTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Players_NameSearch_SpansAllLetters()
    {
        using var document = await GetJsonAsync("/api/players?letter=A&q=griffey");
        var players = document.RootElement.GetProperty("data");

        Assert.True(players.GetArrayLength() > 0);
        Assert.All(players.EnumerateArray(), p => Assert.Equal("Griffey", p.GetProperty("lastName").GetString()));
    }

    [Fact]
    public async Task Players_PositionEraAndSort_FilterAndOrderRows()
    {
        using var document = await GetJsonAsync("/api/players?letter=R&pos=SS&era=1925&sort=hr&pageSize=10");
        var players = document.RootElement.GetProperty("data");

        Assert.True(players.GetArrayLength() > 1);
        var homeRuns = players.EnumerateArray()
            .Select(p => p.GetProperty("totalHomeRuns").ValueKind == JsonValueKind.Null ? 0 : p.GetProperty("totalHomeRuns").GetInt32())
            .ToList();
        Assert.Equal(homeRuns.OrderByDescending(h => h), homeRuns);
        Assert.All(players.EnumerateArray(), p =>
        {
            Assert.StartsWith("R", p.GetProperty("lastName").GetString());
            Assert.True(p.GetProperty("debutYear").GetInt32() <= 1929);
            Assert.True(p.GetProperty("finalYear").GetInt32() >= 1920);
        });
    }

    [Theory]
    [InlineData("/api/players?pos=XX")]
    [InlineData("/api/players?sort=salary")]
    [InlineData("/api/players?era=1700")]
    [InlineData("/api/teams/franchises?era=3000")]
    [InlineData("/api/managers?sort=losses")]
    [InlineData("/api/this-day?date=02-30")]
    public async Task InvalidFilterValues_ReturnBadRequest(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Franchises_NameAndEraFilters_NarrowTheList()
    {
        using var byName = await GetJsonAsync("/api/teams/franchises?q=yankees");
        Assert.Contains(byName.RootElement.EnumerateArray(), f => f.GetProperty("franchiseId").GetString() == "NYY");
        Assert.All(byName.RootElement.EnumerateArray(), f =>
            Assert.Contains("yankees", f.GetProperty("franchiseName").GetString()!, StringComparison.OrdinalIgnoreCase));

        using var byEra = await GetJsonAsync("/api/teams/franchises?era=1875");
        Assert.True(byEra.RootElement.GetArrayLength() > 0);
        Assert.All(byEra.RootElement.EnumerateArray(), f =>
        {
            Assert.True(f.GetProperty("firstYear").GetInt32() <= 1879);
            Assert.True(f.GetProperty("lastYear").GetInt32() >= 1870);
        });
    }

    [Fact]
    public async Task Managers_DefaultSort_LeadsWithConnieMack()
    {
        using var document = await GetJsonAsync("/api/managers?pageSize=5");
        var root = document.RootElement;

        Assert.Equal(5, root.GetProperty("pageSize").GetInt32());
        Assert.True(root.GetProperty("totalCount").GetInt32() > 500);
        var first = root.GetProperty("data")[0];
        Assert.Equal("mackco01", first.GetProperty("playerId").GetString());
        Assert.True(first.GetProperty("isHallOfFamer").GetBoolean());
    }

    [Fact]
    public async Task ManagerDetail_ReturnsSeasonsAndTotals()
    {
        using var document = await GetJsonAsync("/api/managers/mackco01");
        var root = document.RootElement;

        Assert.Equal("Connie Mack", root.GetProperty("fullName").GetString());
        Assert.Equal(3731, root.GetProperty("wins").GetInt32());
        Assert.True(root.GetProperty("seasonRecords").GetArrayLength() >= 53);
        Assert.True(root.GetProperty("worldSeriesTitles").GetInt32() >= 5);
    }

    [Fact]
    public async Task ManagerDetail_ForNonManager_Returns404()
    {
        var response = await Client.GetAsync("/api/managers/ruthba01");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AllStarYears_IncludeEastWestEra()
    {
        using var document = await GetJsonAsync("/api/all-star");
        var years = document.RootElement;

        Assert.True(years.GetArrayLength() > 80);
        Assert.Contains(years.EnumerateArray(), y =>
            y.GetProperty("year").GetInt32() == 1943 && y.GetProperty("hasEastWestGame").GetBoolean());
        Assert.Contains(years.EnumerateArray(), y =>
            y.GetProperty("year").GetInt32() == 1960 && y.GetProperty("gameCount").GetInt32() == 2);
    }

    [Fact]
    public async Task AllStarYear_ReturnsGamesWithRosters()
    {
        using var document = await GetJsonAsync("/api/all-star/1943");
        var games = document.RootElement.GetProperty("games");

        Assert.Equal("AllStar", games[0].GetProperty("gameType").GetString());
        Assert.Contains(games.EnumerateArray(), g => g.GetProperty("gameType").GetString() == "EastWest");
        var alRoster = games[0].GetProperty("rosters")[0];
        Assert.Equal("AL", alRoster.GetProperty("lgId").GetString());
        Assert.True(alRoster.GetProperty("players")[0].GetProperty("isStarter").GetBoolean());
    }

    [Fact]
    public async Task AllStarYear_WithoutGames_Returns404()
    {
        var response = await Client.GetAsync("/api/all-star/1800");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NegroLeagues_ListsSevenLeagues()
    {
        using var document = await GetJsonAsync("/api/negro-leagues");
        var leagues = document.RootElement;

        Assert.Equal(7, leagues.GetArrayLength());
        Assert.All(leagues.EnumerateArray(), l => Assert.True(l.GetProperty("teamSeasons").GetInt32() > 0));
    }

    [Fact]
    public async Task NegroLeague_IdIsCaseInsensitive()
    {
        using var document = await GetJsonAsync("/api/negro-leagues/nn2");
        var root = document.RootElement;

        Assert.Equal("NN2", root.GetProperty("lgId").GetString());
        Assert.True(root.GetProperty("seasons").GetArrayLength() > 10);
        Assert.Contains(root.GetProperty("clubs").EnumerateArray(), c => c.GetProperty("teamId").GetString() == "HG");
    }

    [Fact]
    public async Task NegroLeagueSeason_ReturnsStandingsAndNumericLeaders()
    {
        using var document = await GetJsonAsync("/api/negro-leagues/NN2/1943");
        var root = document.RootElement;

        var leader = root.GetProperty("standings")[0];
        Assert.Equal("HG", leader.GetProperty("teamId").GetString());
        Assert.Equal(0, leader.GetProperty("gamesBehind").GetDouble());

        var avgLeaders = root.GetProperty("leaders").GetProperty("battingAverage");
        Assert.True(avgLeaders.GetArrayLength() > 0);
        Assert.InRange(avgLeaders[0].GetProperty("value").GetDouble(), 0.250, 0.600);
    }

    [Theory]
    [InlineData("/api/negro-leagues/XYZ")]
    [InlineData("/api/negro-leagues/NN2/1960")]
    public async Task NegroLeague_UnknownLeagueOrSeason_Returns404(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ThisDay_ForGivenDate_RanksHallOfFamersFirst()
    {
        using var document = await GetJsonAsync("/api/this-day?date=02-06&limit=3");
        var root = document.RootElement;

        Assert.Equal("February 6", root.GetProperty("label").GetString());
        var birthdays = root.GetProperty("birthdays");
        Assert.Equal(3, birthdays.GetArrayLength());
        Assert.Equal("ruthba01", birthdays[0].GetProperty("playerId").GetString());
    }

    [Fact]
    public async Task PlayerDetail_IncludesCareerOpsIndexAndAllStarCount()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01");
        var root = document.RootElement;

        Assert.True(root.GetProperty("careerOpsIndex").GetInt32() > 200);
        Assert.Equal(2, root.GetProperty("allStarSelections").GetInt32());
        Assert.False(root.GetProperty("hasManagerialCareer").GetBoolean());
    }

    [Fact]
    public async Task PlayerAdvancedBatting_ReturnsLeagueIndexedSeasons()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01/batting/advanced");
        var seasons = document.RootElement;

        var season1927 = seasons.EnumerateArray().Single(s => s.GetProperty("year").GetInt32() == 1927);
        Assert.True(season1927.GetProperty("plateAppearances").GetInt32() > 600);
        Assert.True(season1927.GetProperty("opsIndex").GetDouble() > 200);
        Assert.True(season1927.GetProperty("qualified").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, season1927.GetProperty("babip").ValueKind);
    }

    [Fact]
    public async Task PlayerAdvancedPitching_ReturnsSeasonRates()
    {
        using var document = await GetJsonAsync("/api/players/johnswa01/pitching/advanced");
        var seasons = document.RootElement;

        var season1913 = seasons.EnumerateArray().Single(s => s.GetProperty("year").GetInt32() == 1913);
        Assert.True(season1913.GetProperty("inningsPitched").GetDouble() > 300);
        Assert.InRange(season1913.GetProperty("era").GetDouble(), 1.0, 1.3);
        Assert.True(season1913.GetProperty("qualified").GetBoolean());
        Assert.True(season1913.GetProperty("k9").GetDouble() > 0);
    }

    [Fact]
    public async Task PlayerAllStar_ReturnsSelections()
    {
        using var document = await GetJsonAsync("/api/players/ruthba01/all-star");
        var selections = document.RootElement;

        Assert.Equal(2, selections.GetArrayLength());
        Assert.Equal(1934, selections[0].GetProperty("year").GetInt32());
        Assert.True(selections[0].GetProperty("isStarter").GetBoolean());
    }

    [Fact]
    public async Task PlayerManaging_ReturnsStintsOrEmpty()
    {
        using var manager = await GetJsonAsync("/api/players/torrejo01/managing");
        Assert.True(manager.RootElement.GetArrayLength() > 10);
        Assert.Equal(2010, manager.RootElement[0].GetProperty("year").GetInt32());

        using var nonManager = await GetJsonAsync("/api/players/ruthba01/managing");
        Assert.Equal(0, nonManager.RootElement.GetArrayLength());
    }

    [Theory]
    [InlineData("/api/players/INVALIDXXX/batting/advanced")]
    [InlineData("/api/players/INVALIDXXX/pitching/advanced")]
    [InlineData("/api/players/INVALIDXXX/all-star")]
    [InlineData("/api/players/INVALIDXXX/managing")]
    public async Task PlayerSubresources_InvalidPlayerId_Returns404(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
