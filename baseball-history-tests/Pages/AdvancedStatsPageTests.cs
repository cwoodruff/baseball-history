using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class AdvancedStatsPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PlayerDetails_ShowsAdvancedBattingAndCareerIndex()
    {
        var html = await GetStringAsync("/Players/ruthba01");

        Assert.Contains("Advanced Batting", html);
        Assert.Contains("OPS vs Lg", html);
        Assert.Contains("Advanced Pitching", html);
        Assert.Contains("href=\"/Glossary\"", html);
    }

    [Fact]
    public async Task PlayerDetails_NegroLeaguesStar_ShowsQualifiedCareer()
    {
        // The season-relative threshold is the whole point: Gibson's documented
        // PAs qualify against the schedules his teams actually played.
        var html = await GetStringAsync("/Players/gibsojo99");

        Assert.Contains("Qualified for career rate leaderboards", html);
    }

    [Fact]
    public async Task Glossary_RendersDefinitionsAndCaveats()
    {
        var html = await GetStringAsync("/Glossary");

        Assert.Contains("Season-relative qualification", html);
        Assert.Contains("call it OPS+", html);
        Assert.Contains("No park factors", html);
        Assert.Contains("/SurvivingRecords", html);
        Assert.Contains("/About#data-scope", html);
    }
}
