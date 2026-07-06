using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PitchingLeaderboardTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task StatsPitching_FullPage_RendersShellFilterAndLeaderboardHost()
    {
        var html = await GetStringAsync("/Stats/Pitching?stat=w");

        AssertFullPageShell(html);
        Assert.Contains("id=\"filter-form\"", html);
        Assert.Contains("id=\"leaderboard\"", html);
        Assert.Contains(">Pitching Leaders</h1>", html);
    }

    [Fact]
    public async Task StatsPitching_NonBoostedHtmx_ReturnsLeaderboardPartialOnly()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=so");

        AssertPartialResponse(html);
        Assert.DoesNotContain("id=\"filter-form\"", html);
        Assert.DoesNotContain("id=\"leaderboard\"", html);
        Assert.Contains("table-baseball", html);
        Assert.Contains("Pitching Leaders - Strikeouts", html);
    }

    [Fact]
    public async Task StatsPitching_BoostedHtmx_ReturnsFullPageShell()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=sv", boosted: true);

        AssertFullPageShell(html);
        Assert.Contains("id=\"filter-form\"", html);
        Assert.Contains("id=\"leaderboard\"", html);
    }

    [Fact]
    public async Task StatsPitching_ERA_SingleSeason_ShowsAscendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=era&singleSeason=true");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"ascending\"", html);
        Assert.Contains("▲", html);
        Assert.Contains("table-baseball", html);
    }

    [Fact]
    public async Task StatsPitching_ERA_Career_ShowsAscendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=era&singleSeason=false&minIp=1000");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"ascending\"", html);
        Assert.Contains("▲", html);
        Assert.Contains("Pitching Leaders - ERA", html);
    }

    [Fact]
    public async Task StatsPitching_WHIP_SingleSeason_ShowsAscendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=whip&singleSeason=true");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"ascending\"", html);
        Assert.Contains("▲", html);
        Assert.Contains("table-baseball", html);
    }

    [Fact]
    public async Task StatsPitching_WHIP_Career_ShowsAscendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=whip&singleSeason=false&minIp=1000");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"ascending\"", html);
        Assert.Contains("▲", html);
        Assert.Contains("Pitching Leaders - WHIP", html);
    }

    [Fact]
    public async Task StatsPitching_Wins_ShowsDescendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"descending\"", html);
        Assert.Contains("▼", html);
    }

    [Fact]
    public async Task StatsPitching_Strikeouts_ShowsDescendingIndicator()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=so");

        AssertPartialResponse(html);
        Assert.Contains("aria-sort=\"descending\"", html);
        Assert.Contains("▼", html);
    }

    [Fact]
    public async Task StatsPitching_YearFilter_PreservesInHtmxRequest()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w&fromYear=2020&toYear=2025");

        AssertPartialResponse(html);
        Assert.Contains("hx-include=\"#filter-form\"", html);
        Assert.Contains("Pitching Leaders - Wins", html);
    }

    [Fact]
    public async Task StatsPitching_LeagueFilter_PreservesInHtmxRequest()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=era&league=AL&singleSeason=true");

        AssertPartialResponse(html);
        Assert.Contains("hx-include=\"#filter-form\"", html);
        Assert.Contains("aria-sort=\"ascending\"", html);
    }

    [Fact]
    public async Task StatsPitching_Htmx_PageTwo_PreservesPaginationAndModalContracts()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=so&page=2");

        AssertPartialResponse(html);
        Assert.Contains("Page 2 of", html);
        Assert.Contains("hx-get=\"/Stats/Pitching", html);
        Assert.Contains("hx-target=\"#leaderboard\"", html);
        Assert.Contains("hx-boost=\"false\"", html);
        Assert.Contains("hx-get=\"/Players/Modal/", html);
        Assert.Contains("hx-target=\"#modal-container\"", html);
    }

    [Fact]
    public async Task StatsPitching_Htmx_PageZero_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w&page=0");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
        Assert.True(totalPages > 1);
    }

    [Fact]
    public async Task StatsPitching_Htmx_NegativePage_ClampsToFirstPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=era&page=-10");
        var (currentPage, _) = ParsePaginationSummary(html);

        Assert.Equal(1, currentPage);
    }

    [Fact]
    public async Task StatsPitching_Htmx_PageBeyondMax_ClampsToLastPage()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=so&page=999999");
        var (currentPage, totalPages) = ParsePaginationSummary(html);

        Assert.Equal(totalPages, currentPage);
    }

    [Fact]
    public async Task StatsPitching_SingleSeasonMode_ShowsYearAndTeamColumns()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w&singleSeason=true");

        AssertPartialResponse(html);
        Assert.Contains("<th scope=\"col\">Year</th>", html);
        Assert.Contains("<th scope=\"col\">Team</th>", html);
    }

    [Fact]
    public async Task StatsPitching_CareerMode_HidesYearAndTeamColumns()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w&singleSeason=false");

        AssertPartialResponse(html);
        Assert.DoesNotContain("<th scope=\"col\">Year</th>", html);
        Assert.DoesNotContain("<th scope=\"col\">Team</th>", html);
    }

    [Fact]
    public async Task StatsPitching_SortByWalks_PutsCareerWalksLeaderFirst()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=bb&singleSeason=false");

        // Nolan Ryan is the all-time walks leader (2,795); before the fix,
        // stat=bb fell through to the default and sorted by wins (Cy Young first)
        var firstRow = html.IndexOf("<tbody>", StringComparison.Ordinal);
        var rowRegion = html.Substring(firstRow, 1500);
        Assert.Contains("Nolan Ryan", rowRegion);
        Assert.Contains("2,795", rowRegion);
    }

    [Theory]
    [InlineData("/Stats/Pitching?stat=w")]
    [InlineData("/Stats/Batting?stat=hr")]
    public async Task StatsPages_YearDropdowns_SpanTheFullDataset(string url)
    {
        var html = await GetStringAsync(url);

        // Lahman data starts in 1871; the dropdowns used to truncate at 1976
        Assert.Contains("value=\"1871\"", html);
        Assert.Contains("value=\"1950\"", html);
    }

    [Fact]
    public async Task StatsPitching_HOFBadge_AppearsForInductees()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w&singleSeason=false");

        AssertPartialResponse(html);
        Assert.Contains("rhx-badge", html);
        Assert.Contains("HOF</span>", html);
    }

    [Fact]
    public async Task StatsPitching_FilterForm_AllControlsPresent()
    {
        var html = await GetStringAsync("/Stats/Pitching");

        AssertFullPageShell(html);
        Assert.Contains("name=\"stat\"", html);
        Assert.Contains("name=\"fromYear\"", html);
        Assert.Contains("name=\"toYear\"", html);
        Assert.Contains("name=\"league\"", html);
        Assert.Contains("name=\"minIp\"", html);
        Assert.Contains("name=\"singleSeason\"", html);
    }

    [Fact]
    public async Task StatsPitching_StatColumnHeaders_LinkToCorrectSortParams()
    {
        var html = await GetHtmxStringAsync("/Stats/Pitching?stat=w");

        AssertPartialResponse(html);
        Assert.Contains("hx-get=\"/Stats/Pitching?stat=era\"", html);
        Assert.Contains("hx-get=\"/Stats/Pitching?stat=whip\"", html);
        Assert.Contains("hx-get=\"/Stats/Pitching?stat=so\"", html);
        Assert.Contains("hx-get=\"/Stats/Pitching?stat=w\"", html);
    }

    private static void AssertFullPageShell(string html)
    {
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("hx-boost=\"true\"", html);
        Assert.Contains("class=\"search-container\"", html);
        Assert.Contains("id=\"search-results\"", html);
        Assert.Contains("id=\"modal-container\"", html);
    }

    private static void AssertPartialResponse(string html)
    {
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        Assert.DoesNotContain("hx-boost=\"true\"", html);
    }
}
