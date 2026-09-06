using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class SurvivingRecordsPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SurvivingRecords_RendersFeaturePage()
    {
        var html = await GetStringAsync("/SurvivingRecords");

        Assert.Contains("The Surviving Record", html);
        Assert.Contains("Josh Gibson", html);
        Assert.Contains("Seamheads", html);
        Assert.Contains("href=\"/Players/gibsojo99\"", html);
        Assert.Contains("/About#data-scope", html);
    }

    [Fact]
    public async Task PartialRecordBadge_LinksToSurvivingRecords()
    {
        var html = await GetStringAsync("/Players/smith01");

        Assert.Contains("href=\"/SurvivingRecords\"", html);
        Assert.Contains("partial-record-badge", html);
    }

    [Fact]
    public async Task About_DataScope_LinksToSurvivingRecords()
    {
        var html = await GetStringAsync("/About");

        Assert.Contains("href=\"/SurvivingRecords\"", html);
    }
}
