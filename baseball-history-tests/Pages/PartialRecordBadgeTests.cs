using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PartialRecordBadgeTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // Surname-only segregation-era record ("Smith", debut 1884)
    private const string PartialPlayerId = "smith01";
    private const string DocumentedPlayerId = "ruthba01";

    [Fact]
    public async Task PlayerDetails_PartialRecord_RendersBadgeWithApprovedCopy()
    {
        var html = await GetStringAsync($"/Players/{PartialPlayerId}");

        Assert.Contains("partial-record-badge", html);
        Assert.Contains("Partial record", html);
        Assert.Contains("Historically incomplete record", html);
    }

    [Fact]
    public async Task PlayerDetails_DocumentedPlayer_HasNoBadge()
    {
        var html = await GetStringAsync($"/Players/{DocumentedPlayerId}");

        Assert.DoesNotContain("partial-record", html);
    }

    [Fact]
    public async Task PlayerModal_PartialRecord_RendersBadge()
    {
        var html = await GetStringAsync($"/Players/Modal/{PartialPlayerId}");

        Assert.Contains("partial-record-badge", html);
    }

    [Fact]
    public async Task PlayersList_PartialRecord_RendersMarkerWithAriaLabel()
    {
        var html = await GetStringAsync("/Players?q=malcolm");

        Assert.Contains("partial-record-marker", html);
        Assert.Contains("aria-label=", html);
    }

    [Fact]
    public async Task BadgeExplanation_CarriesApprovedCopy()
    {
        var html = await GetStringAsync($"/Players/{PartialPlayerId}");

        // Razor HTML-encodes the attribute value (apostrophes, em dashes), so
        // assert on distinctive plain-ASCII fragments of the approved copy.
        Assert.Contains("Only a partial name survived in the original sources", html);
        Assert.Contains("box scores and rosters were unevenly", html);
    }

    [Fact]
    public async Task ApiPlayerDetail_ReportsIsPartialRecord()
    {
        using var partial = JsonDocument.Parse(await GetStringAsync($"/api/players/{PartialPlayerId}"));
        using var documented = JsonDocument.Parse(await GetStringAsync($"/api/players/{DocumentedPlayerId}"));

        Assert.True(partial.RootElement.GetProperty("isPartialRecord").GetBoolean());
        Assert.False(documented.RootElement.GetProperty("isPartialRecord").GetBoolean());
    }
}
