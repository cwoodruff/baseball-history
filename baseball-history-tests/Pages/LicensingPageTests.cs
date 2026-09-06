using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class LicensingPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Licensing_RendersSourcesAndMitScope()
    {
        var html = await GetStringAsync("/Licensing");

        Assert.Contains("MIT License", html);
        Assert.Contains("does not cover the data", html);
        Assert.Contains("Lahman Baseball Database", html);
        Assert.Contains("Seamheads Negro Leagues Database", html);
        Assert.Contains("creativecommons.org/licenses/by-sa/3.0", html);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/About")]
    [InlineData("/ApiDocs")]
    public async Task Licensing_IsLinkedFrom(string url)
    {
        var html = await GetStringAsync(url);

        Assert.Contains("/Licensing", html);
    }
}
