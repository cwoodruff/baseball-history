using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class BrowserFilterTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // --- #42: Players browser filters ---

    [Fact]
    public async Task Players_NameFilter_SearchesAcrossAllLetters()
    {
        var html = await GetStringAsync("/Players?q=babe+ruth");

        Assert.Contains("Babe Ruth", html);
        Assert.Contains("id=\"player-filter-form\"", html);
    }

    [Fact]
    public async Task Players_PositionFilter_ReturnsResults()
    {
        var html = await GetStringAsync("/Players?letter=A&pos=P");

        Assert.Contains("player-card", html);
        Assert.Contains("value=\"P\" selected", html);
    }

    [Fact]
    public async Task Players_EraFilter_ReturnsPlayersActiveInDecade()
    {
        var html = await GetStringAsync("/Players?q=babe+ruth&era=1920");

        Assert.Contains("Babe Ruth", html);

        // Ruth was not active in the 1950s
        var html2 = await GetStringAsync("/Players?q=babe+ruth&era=1950");
        Assert.DoesNotContain("player-card", html2);
    }

    [Fact]
    public async Task Players_SortByHomeRuns_PutsSluggerFirst()
    {
        var html = await GetStringAsync("/Players?letter=A&sort=hr");

        var firstCard = html.IndexOf("player-card", StringComparison.Ordinal);
        Assert.True(firstCard >= 0);
        var cardRegion = html.Substring(firstCard, 2000);
        Assert.Contains("Hank Aaron", cardRegion);
    }

    [Fact]
    public async Task Players_Pagination_PreservesFilters()
    {
        var html = (await GetStringAsync("/Players?letter=A&pos=P")).Replace("&amp;", "&");

        Assert.Contains("pos=P", html);
        Assert.Contains("letter=A", html);
    }

    [Fact]
    public async Task Players_AlphabetNav_PreservesNonSearchFilters()
    {
        var html = (await GetStringAsync("/Players?letter=A&pos=C")).Replace("&amp;", "&");

        // letter links keep the position filter
        Assert.Contains("/Players?pos=C&letter=B", html);
    }

    [Fact]
    public async Task Players_DefaultView_UnaffectedByFilterPlumbing()
    {
        var html = await GetStringAsync("/Players");

        Assert.Contains("id=\"players-content\"", html);
        Assert.Contains("id=\"player-filter-form\"", html);
        Assert.Contains("class=\"alphabet-nav\"", html);
    }

    // --- #43: Teams browser filters ---

    [Fact]
    public async Task Teams_NameFilter_MatchesFranchiseName()
    {
        var html = await GetStringAsync("/Teams?q=yankees");

        Assert.Contains("New York Yankees", html);
        Assert.DoesNotContain("Boston Red Sox", html);
    }

    [Fact]
    public async Task Teams_NameFilter_WithLeague_Compose()
    {
        var html = await GetStringAsync("/Teams?league=AL&q=sox");

        Assert.Contains("Red Sox", html);
        Assert.Contains("White Sox", html);
        Assert.DoesNotContain("Yankees", html);
        Assert.Contains("name=\"league\" value=\"AL\"", html);
    }

    [Fact]
    public async Task Teams_EraFilter_ShowsFranchisesActiveInDecade()
    {
        var html = await GetStringAsync("/Teams?era=1880");

        // 1880s-only franchises qualify, modern-only expansion teams do not
        Assert.Contains("Historical Franchises", html);
        Assert.DoesNotContain("Arizona Diamondbacks", html);
    }

    [Fact]
    public async Task Teams_FilterForm_PresentOnFullPage()
    {
        var html = await GetStringAsync("/Teams");

        Assert.Contains("id=\"team-filter-form\"", html);
        Assert.Contains("id=\"team-search\"", html);
        Assert.Contains("id=\"team-era\"", html);
    }

    [Fact]
    public async Task Teams_LeagueButtons_PreserveFiltersInUrls()
    {
        var html = (await GetStringAsync("/Teams?q=sox&era=1990")).Replace("&amp;", "&");

        Assert.Contains("/Teams?league=AL&q=sox&era=1990", html);
    }
}
