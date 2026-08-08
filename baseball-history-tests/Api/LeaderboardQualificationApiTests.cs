using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Api;

public class LeaderboardQualificationApiTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task BattingLeaders_DefaultQualified_ExcludesSubThresholdPlayers()
    {
        // Default qualified=true should exclude players with insufficient PA
        using var document = await GetJsonAsync("/api/leaders/batting?stat=avg&pageSize=50");
        var root = document.RootElement;
        var items = root.GetProperty("data");

        Assert.True(items.GetArrayLength() > 0);

        // Verify no player has a 1.000 average with 1-2 AB (the classic small-sample outlier)
        Assert.All(
            items.EnumerateArray(),
            player =>
            {
                var avg = player.GetProperty("battingAverage").GetDouble();
                var ab = player.GetProperty("atBats").GetInt32();

                // If someone has 1.000 avg, they should have meaningful AB count (not 1-2)
                if (avg >= 0.999)
                {
                    Assert.True(ab >= 100, $"Player {player.GetProperty("playerName").GetString()} has {avg:F3} avg with only {ab} AB");
                }
            });
    }

    [Fact]
    public async Task BattingLeaders_QualifiedFalse_ReturnsMoreResults()
    {
        // qualified=false should return more results than qualified=true for rate stats
        using var qualifiedDoc = await GetJsonAsync("/api/leaders/batting?stat=avg&pageSize=100");
        using var unqualifiedDoc = await GetJsonAsync("/api/leaders/batting?stat=avg&qualified=false&pageSize=100");

        var qualifiedTotal = qualifiedDoc.RootElement.GetProperty("totalCount").GetInt32();
        var unqualifiedTotal = unqualifiedDoc.RootElement.GetProperty("totalCount").GetInt32();

        // Unqualified should have more results (includes small-sample players)
        Assert.True(unqualifiedTotal > qualifiedTotal,
            $"Expected unqualified ({unqualifiedTotal}) > qualified ({qualifiedTotal})");
    }

    [Fact]
    public async Task BattingLeaders_ExplicitMinAb_OverridesQualification()
    {
        // Explicit minAb should override automatic qualification
        using var document = await GetJsonAsync("/api/leaders/batting?stat=avg&minAb=500&pageSize=50");
        var root = document.RootElement;
        var items = root.GetProperty("data");

        Assert.True(items.GetArrayLength() > 0);

        // All players should have at least 500 AB
        Assert.All(
            items.EnumerateArray(),
            player => Assert.True(player.GetProperty("atBats").GetInt32() >= 500));
    }

    [Fact]
    public async Task PitchingLeaders_DefaultQualified_ExcludesSubThresholdPlayers()
    {
        // Default qualified=true should exclude pitchers with insufficient IP
        using var document = await GetJsonAsync("/api/leaders/pitching?stat=era&pageSize=50");
        var root = document.RootElement;
        var items = root.GetProperty("data");

        Assert.True(items.GetArrayLength() > 0);

        // Verify no pitcher has an ERA of 0.00 with trivial IP (the classic relief appearance outlier)
        Assert.All(
            items.EnumerateArray(),
            pitcher =>
            {
                var era = pitcher.GetProperty("era").GetDouble();
                var ip = pitcher.GetProperty("inningsPitched").GetDouble();

                // If someone has 0.00 ERA, they should have meaningful IP
                if (era <= 0.01)
                {
                    Assert.True(ip >= 10, $"Pitcher {pitcher.GetProperty("playerName").GetString()} has {era:F2} ERA with only {ip:F1} IP");
                }
            });
    }

    [Fact]
    public async Task PitchingLeaders_QualifiedFalse_ReturnsMoreResults()
    {
        // qualified=false should return more results than qualified=true for rate stats
        using var qualifiedDoc = await GetJsonAsync("/api/leaders/pitching?stat=era&pageSize=100");
        using var unqualifiedDoc = await GetJsonAsync("/api/leaders/pitching?stat=era&qualified=false&pageSize=100");

        var qualifiedTotal = qualifiedDoc.RootElement.GetProperty("totalCount").GetInt32();
        var unqualifiedTotal = unqualifiedDoc.RootElement.GetProperty("totalCount").GetInt32();

        // Unqualified should have more results (includes small-sample pitchers)
        Assert.True(unqualifiedTotal > qualifiedTotal,
            $"Expected unqualified ({unqualifiedTotal}) > qualified ({qualifiedTotal})");
    }

    [Fact]
    public async Task PitchingLeaders_ExplicitMinIp_OverridesQualification()
    {
        // Explicit minIp should override automatic qualification
        using var document = await GetJsonAsync("/api/leaders/pitching?stat=era&minIp=100&pageSize=50");
        var root = document.RootElement;
        var items = root.GetProperty("data");

        Assert.True(items.GetArrayLength() > 0);

        // All pitchers should have at least 100 IP
        Assert.All(
            items.EnumerateArray(),
            pitcher => Assert.True(pitcher.GetProperty("inningsPitched").GetDouble() >= 100));
    }

    [Fact]
    public async Task BattingLeaders_CountingStat_UnaffectedByQualification()
    {
        // Counting stats like HR should not be affected by qualification
        using var qualifiedDoc = await GetJsonAsync("/api/leaders/batting?stat=hr&pageSize=10");
        using var unqualifiedDoc = await GetJsonAsync("/api/leaders/batting?stat=hr&qualified=false&pageSize=10");

        var qualifiedItems = qualifiedDoc.RootElement.GetProperty("data");
        var unqualifiedItems = unqualifiedDoc.RootElement.GetProperty("data");

        // Both should have the same results for counting stats
        Assert.Equal(qualifiedItems.GetArrayLength(), unqualifiedItems.GetArrayLength());
    }

    [Fact]
    public async Task PitchingLeaders_CountingStat_UnaffectedByQualification()
    {
        // Counting stats like W (wins) should not be affected by qualification
        using var qualifiedDoc = await GetJsonAsync("/api/leaders/pitching?stat=w&pageSize=10");
        using var unqualifiedDoc = await GetJsonAsync("/api/leaders/pitching?stat=w&qualified=false&pageSize=10");

        var qualifiedItems = qualifiedDoc.RootElement.GetProperty("data");
        var unqualifiedItems = unqualifiedDoc.RootElement.GetProperty("data");

        // Both should have the same results for counting stats
        Assert.Equal(qualifiedItems.GetArrayLength(), unqualifiedItems.GetArrayLength());
    }

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
