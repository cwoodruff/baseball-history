using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class HallOfFamePageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_RendersFullPageShell()
    {
        var html = await GetStringAsync("/HallOfFame");

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Baseball Hall of Fame", html);
        Assert.Contains("id=\"inductee-list\"", html);
        Assert.Contains("Hall of Fame Inductees", html);
    }

    [Fact]
    public async Task Index_YearFilter_Shows1936InductionClass()
    {
        var html = await GetStringAsync("/HallOfFame?year=1936");

        Assert.Contains("Babe Ruth", html);
        Assert.Contains("Ty Cobb", html);
        Assert.Contains("Clear Filters", html);
    }

    [Fact]
    public async Task Index_InvalidCategory_ShowsEmptyState()
    {
        var html = await GetStringAsync("/HallOfFame?category=NoSuchCategory");

        Assert.Contains("No Inductees Found", html);
        Assert.Contains("No Hall of Fame inductees match your criteria.", html);
    }

    [Fact]
    public async Task Index_PageBeyondMax_ClampsToLastPage()
    {
        var html = await GetHtmxStringAsync("/HallOfFame?page=999999");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(totalPages, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task Index_NonBoostedHtmx_ReturnsInducteeListPartial()
    {
        var html = await GetHtmxStringAsync("/HallOfFame?year=1936");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Hall of Fame Inductees", html);
        Assert.DoesNotContain("id=\"filter-form\"", html);
    }

    [Fact]
    public async Task Index_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/HallOfFame?year=1936", boosted: true);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Baseball Hall of Fame", html);
        Assert.Contains("id=\"inductee-list\"", html);
    }
}
