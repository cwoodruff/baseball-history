using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class ParksPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_ReturnsFullPage_WithBallparksHeading()
    {
        var html = await GetStringAsync("/Parks");

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("Ballparks", html);
        Assert.Contains("ballparks", html); // "N ballparks" count line
    }

    [Fact]
    public async Task Index_ShowsPagination()
    {
        var html = await GetStringAsync("/Parks");

        var (currentPage, totalPages) = ParsePaginationSummary(html);
        Assert.Equal(1, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task Index_SearchFilter_FindsFenway()
    {
        var html = await GetStringAsync("/Parks?q=fenway");

        Assert.Contains("Fenway Park", html);
        Assert.Contains("/Parks/BOS07", html);
        Assert.DoesNotContain("Wrigley Field", html);
    }

    [Fact]
    public async Task Index_SearchFilter_MatchesAlias()
    {
        // Wrigley Field (CHI11) was formerly Weeghman Park
        var html = await GetStringAsync("/Parks?q=weeghman");

        Assert.Contains("Wrigley Field", html);
        Assert.Contains("/Parks/CHI11", html);
    }

    [Fact]
    public async Task Index_StateFilter_LimitsResults()
    {
        var html = await GetStringAsync("/Parks?state=MA");

        Assert.Contains("Fenway Park", html);
        Assert.DoesNotContain("Yankee Stadium", html);
    }

    [Fact]
    public async Task Index_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/Parks");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("park-filter-form", html);
    }

    [Fact]
    public async Task Detail_KnownPark_ShowsParkInfoAndHistory()
    {
        var html = await GetStringAsync("/Parks/BOS07");

        Assert.Contains("Fenway Park", html);
        Assert.Contains("Boston", html);
        // Attendance history chart
        Assert.Contains("park-attendance-chart", html);
        Assert.Contains("Attendance History", html);
        // Tenants and season history link to team season pages
        Assert.Contains("/Teams/Season/BOS/AL/", html);
        // The Braves borrowed Fenway in 1913-14, so it has multiple tenants
        Assert.Contains("/Teams/Season/BSN/NL/", html);
        // Lifespan starts with Fenway's 1912 opening
        Assert.Contains("1912", html);
    }

    [Fact]
    public async Task Detail_HtmxRequest_ReturnsPartial()
    {
        var html = await GetHtmxStringAsync("/Parks/BOS07");

        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("Fenway Park", html);
    }

    [Fact]
    public async Task Detail_UnknownPark_Returns404()
    {
        var response = await Client.GetAsync("/Parks/NOPE99");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TeamSeasonPage_LinksToHomePark()
    {
        var html = await GetStringAsync("/Teams/Season/BOS/AL/1975");

        Assert.Contains("/Parks/BOS07", html);
    }

    [Fact]
    public async Task Navigation_IncludesBallparksLink()
    {
        var html = await GetStringAsync("/Parks");

        Assert.Contains("href=\"/Parks\"", html);
    }
}
