using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PostseasonPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_RendersFullPageShell()
    {
        var html = await GetStringAsync("/Postseason");

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Postseason Results", html);
        Assert.Contains("id=\"postseason-list\"", html);
        Assert.Contains("Postseason Series", html);
    }

    [Fact]
    public async Task Index_ProvidesYearNavigationLinks()
    {
        var html = await GetStringAsync("/Postseason");

        Assert.Contains("href=\"/Postseason?year=", html);
        Assert.Contains("hx-target=\"#postseason-list\"", html);
        Assert.Contains("hx-push-url=\"true\"", html);
    }

    [Fact]
    public async Task Index_YearFilter_Orders2016RoundsChronologically()
    {
        var html = await GetStringAsync("/Postseason?year=2016");
        var seriesSection = html[html.IndexOf("<strong>Postseason Series</strong>", StringComparison.Ordinal)..];

        var alWildCard = seriesSection.IndexOf("AL Wild Card", StringComparison.Ordinal);
        var nlWildCard = seriesSection.IndexOf("NL Wild Card", StringComparison.Ordinal);
        var alDivision = seriesSection.IndexOf("AL Division Series", StringComparison.Ordinal);
        var nlDivision = seriesSection.IndexOf("NL Division Series", StringComparison.Ordinal);
        var alChampionship = seriesSection.IndexOf("AL Championship Series", StringComparison.Ordinal);
        var nlChampionship = seriesSection.IndexOf("NL Championship Series", StringComparison.Ordinal);
        var worldSeries = seriesSection.IndexOf("World Series", StringComparison.Ordinal);

        Assert.True(alWildCard >= 0);
        Assert.True(nlWildCard >= 0);
        Assert.True(alDivision > alWildCard);
        Assert.True(nlDivision > nlWildCard);
        Assert.True(alChampionship > alDivision);
        Assert.True(nlChampionship > nlDivision);
        Assert.True(worldSeries > alChampionship);
        Assert.True(worldSeries > nlChampionship);
    }

    [Fact]
    public async Task Index_EmptySeason_ShowsEmptyState()
    {
        var html = await GetStringAsync("/Postseason?year=1800");

        Assert.Contains("No Postseason Data Found", html);
        Assert.Contains("No postseason series match your criteria.", html);
    }

    [Fact]
    public async Task Index_PageBeyondMax_ClampsToLastPage()
    {
        var html = await GetHtmxStringAsync("/Postseason?page=999999");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(totalPages, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task Index_NonBoostedHtmx_ReturnsSeriesListPartial()
    {
        var html = await GetHtmxStringAsync("/Postseason?year=2016");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Postseason Series", html);
        Assert.DoesNotContain("id=\"filter-form\"", html);
    }

    [Fact]
    public async Task Index_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Postseason?year=2016", boosted: true);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Postseason Results", html);
        Assert.Contains("id=\"postseason-list\"", html);
    }
}
