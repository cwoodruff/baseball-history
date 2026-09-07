using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

public class AwardSalaryParkApiContractTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AwardsWinners_FilteredMvpRace_ReturnsPagedWinnerContract()
    {
        using var document = await GetJsonAsync("/api/awards/winners?awardId=Most%20Valuable%20Player&year=2016&lgId=AL&pageSize=3");
        var root = document.RootElement;
        var winners = root.GetProperty("data");

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(3, root.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, winners.GetArrayLength());

        var winner = winners[0];
        Assert.Equal("troutmi01", winner.GetProperty("playerId").GetString());
        Assert.Equal("Mike Trout", winner.GetProperty("playerName").GetString());
        Assert.Equal("Most Valuable Player", winner.GetProperty("awardId").GetString());
        Assert.Equal(2016, winner.GetProperty("year").GetInt32());
        Assert.Equal("AL", winner.GetProperty("lgId").GetString());
    }

    [Fact]
    public async Task AwardsVoting_KnownRace_ReturnsOrderedVoteContract()
    {
        using var document = await GetJsonAsync("/api/awards/voting/Most%20Valuable%20Player/2016/AL");
        var root = document.RootElement;
        var votes = root.GetProperty("votes");

        Assert.Equal("Most Valuable Player", root.GetProperty("awardId").GetString());
        Assert.Equal(2016, root.GetProperty("year").GetInt32());
        Assert.Equal("AL", root.GetProperty("lgId").GetString());
        Assert.True(votes.GetArrayLength() > 5);

        var first = votes[0];
        var second = votes[1];
        Assert.Equal("troutmi01", first.GetProperty("playerId").GetString());
        Assert.Equal("Mike Trout", first.GetProperty("playerName").GetString());
        Assert.True(first.GetProperty("voteShare").GetDouble() > second.GetProperty("voteShare").GetDouble());
    }

    [Fact]
    public async Task AwardsWinners_UnknownAwardFilter_ReturnsEmptyPage()
    {
        using var document = await GetJsonAsync("/api/awards/winners?awardId=NoSuchAward");
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(0, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, root.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task SalaryPlayerHistory_KnownPlayer_ReturnsCareerTotalAndDescendingSeasons()
    {
        using var document = await GetJsonAsync("/api/salaries/players/troutmi01");
        var root = document.RootElement;
        var seasons = root.GetProperty("seasons");

        Assert.Equal("troutmi01", root.GetProperty("playerId").GetString());
        Assert.Equal("Mike Trout", root.GetProperty("fullName").GetString());
        Assert.Equal(23676333, root.GetProperty("careerTotal").GetInt64());
        Assert.True(seasons.GetArrayLength() >= 4);
        Assert.Equal(2016, seasons[0].GetProperty("year").GetInt32());
        Assert.True(seasons[0].GetProperty("year").GetInt32() > seasons[seasons.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task SalaryTeamSeason_KnownTeam_ReturnsPayrollContract()
    {
        using var document = await GetJsonAsync("/api/salaries/teams/BOS/2015");
        var root = document.RootElement;
        var players = root.GetProperty("players");

        Assert.Equal(2015, root.GetProperty("year").GetInt32());
        Assert.Equal("BOS", root.GetProperty("teamId").GetString());
        Assert.Equal(181103400, root.GetProperty("totalPayroll").GetInt64());
        Assert.True(root.GetProperty("playerCount").GetInt32() > 20);
        Assert.True(players.GetArrayLength() > 20);
        Assert.Contains(players.EnumerateArray(), player => player.GetProperty("playerId").GetString() == "ortizda01");
    }

    [Fact]
    public async Task SalaryLeaders_FilteredYear_ReturnsPagedSalaryContract()
    {
        using var document = await GetJsonAsync("/api/salaries/leaders?year=2016&pageSize=3");
        var root = document.RootElement;
        var leaders = root.GetProperty("data");

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(3, root.GetProperty("pageSize").GetInt32());
        Assert.True(root.GetProperty("totalCount").GetInt32() > 3);
        Assert.Equal(3, leaders.GetArrayLength());
        Assert.Equal("kershcl01", leaders[0].GetProperty("playerId").GetString());
        Assert.True(leaders[0].GetProperty("salary").GetInt64() >= leaders[1].GetProperty("salary").GetInt64());
    }

    [Fact]
    public async Task SalaryLeaders_UnknownYear_ReturnsEmptyPage()
    {
        using var document = await GetJsonAsync("/api/salaries/leaders?year=1800");
        var root = document.RootElement;

        Assert.Equal(0, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, root.GetProperty("data").GetArrayLength());
    }

    [Fact]
    public async Task Parks_List_ReturnsPagedContract()
    {
        using var document = await GetJsonAsync("/api/parks?pageSize=3");
        var root = document.RootElement;
        var parks = root.GetProperty("data");

        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(3, root.GetProperty("pageSize").GetInt32());
        Assert.Equal(3, parks.GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(parks[0].GetProperty("parkKey").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(parks[0].GetProperty("parkName").GetString()));
    }

    [Fact]
    public async Task Parks_Detail_KnownPark_ReturnsSeasonHistory()
    {
        using var document = await GetJsonAsync("/api/parks/BOS07");
        var root = document.RootElement;
        var seasons = root.GetProperty("seasons");

        Assert.Equal("BOS07", root.GetProperty("parkKey").GetString());
        Assert.Equal("Fenway Park", root.GetProperty("parkName").GetString());
        Assert.Equal("Boston", root.GetProperty("city").GetString());
        Assert.Equal("MA", root.GetProperty("state").GetString());
        Assert.True(seasons.GetArrayLength() > 50);
        Assert.True(seasons[0].GetProperty("year").GetInt32() > seasons[seasons.GetArrayLength() - 1].GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task Parks_List_UnknownState_ReturnsEmptyPage()
    {
        using var document = await GetJsonAsync("/api/parks?state=ZZ");
        var root = document.RootElement;

        Assert.Equal(0, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, root.GetProperty("data").GetArrayLength());
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
