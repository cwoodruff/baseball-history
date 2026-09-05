using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class DataScopeStatementTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task About_RendersDataScopeStatement()
    {
        var html = await GetStringAsync("/About");

        Assert.Contains("id=\"data-scope\"", html);
        Assert.Contains("no park factors", html);
        Assert.Contains("not context-adjusted", html);
        Assert.Contains("Negro Leagues records are partial", html);
    }

    [Theory]
    [InlineData("/Stats/Batting")]
    [InlineData("/Stats/Pitching")]
    public async Task Leaderboards_LinkToDataScopeStatement(string url)
    {
        var html = await GetStringAsync(url);

        Assert.Contains("/About#data-scope", html);
        Assert.Contains("not context-adjusted", html);
    }
}
