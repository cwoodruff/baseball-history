using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class NavigationAndAccessibilityTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // --- #40: active nav highlighting ---

    [Theory]
    [InlineData("/Players?letter=A", ">Players</a>")]
    [InlineData("/Teams", ">Teams</a>")]
    [InlineData("/Compare", ">Compare</a>")]
    public async Task Navbar_MarksCurrentSectionActive(string url, string linkText)
    {
        var html = await GetStringAsync(url);

        Assert.Contains($"class=\"nav-link active\" aria-current=\"page\"", html);
        // the active attributes belong to the link for this section
        var activeIndex = html.IndexOf("class=\"nav-link active\"", StringComparison.Ordinal);
        var closing = html.IndexOf("</a>", activeIndex, StringComparison.Ordinal) + 4;
        var anchor = html[activeIndex..closing];
        Assert.EndsWith(linkText, anchor);
    }

    [Fact]
    public async Task Navbar_StatsDropdown_ActiveForHallOfFame()
    {
        var html = await GetStringAsync("/HallOfFame");

        Assert.Contains("class=\"nav-link active dropdown-toggle\"", html);
    }

    [Fact]
    public async Task Navbar_Home_NotActiveOnOtherPages()
    {
        var html = await GetStringAsync("/About");

        var homeAnchor = html.Substring(html.IndexOf(">Home</a>", StringComparison.Ordinal) - 200, 209);
        Assert.DoesNotContain("nav-link active", homeAnchor);
    }

    // --- #40: breadcrumbs ---

    [Fact]
    public async Task Franchise_ShowsBreadcrumbTrail()
    {
        var html = await GetStringAsync("/Teams/Franchise/NYY");

        Assert.Contains("aria-label=\"breadcrumb\"", html);
        Assert.Contains("<a href=\"/Teams\">Teams</a>", html);
        Assert.Contains("aria-current=\"page\">New York Yankees</li>", html);
    }

    [Fact]
    public async Task TeamSeason_ShowsThreeLevelBreadcrumb()
    {
        var html = await GetStringAsync("/Teams/Season/NYA/AL/1927");

        Assert.Contains("<a href=\"/Teams\">Teams</a>", html);
        Assert.Contains("href=\"/Teams/Franchise/NYY\">New York Yankees</a>", html);
        Assert.Contains("aria-current=\"page\">1927 New York Yankees</li>", html);
    }

    [Fact]
    public async Task PlayerDetails_UsesSharedBreadcrumbComponent()
    {
        var html = await GetStringAsync("/Players/ruthba01");

        Assert.Contains("<a href=\"/Players\">Players</a>", html);
        Assert.Contains("aria-current=\"page\">Babe Ruth</li>", html);
    }

    // --- #41: keyboard accessibility ---

    [Fact]
    public async Task Layout_HasSkipLinkToMainContent()
    {
        var html = await GetStringAsync("/");

        Assert.Contains("class=\"skip-link\" href=\"#main-content\"", html);
        Assert.Contains("id=\"main-content\"", html);
    }

    [Fact]
    public async Task PlayerCards_AreRealAnchorsToPlayerPages()
    {
        var html = await GetStringAsync("/Players?letter=A");

        Assert.Contains("<a class=\"card player-card h-100 text-reset text-decoration-none\"", html);
        Assert.Contains("href=\"/Players/", html);
    }

    [Fact]
    public async Task Pagination_ActivePage_HasAriaCurrent()
    {
        var html = await GetStringAsync("/Players?letter=A");

        Assert.Contains("aria-current=\"page\"", html);
        Assert.Contains("href=\"/Players?letter=A&page=2\"", html.Replace("&amp;", "&"));
    }

    [Fact]
    public async Task AlphabetNav_Letters_HaveRealHrefs()
    {
        var html = await GetStringAsync("/Players?letter=A");

        Assert.Contains("href=\"/Players?letter=B&page=1\"", html.Replace("&amp;", "&"));
    }

    [Fact]
    public async Task Leaderboard_SortHeaders_HaveHrefsAndAriaSort()
    {
        var html = await GetStringAsync("/Stats/Batting?stat=hr");

        Assert.Contains("aria-sort=\"descending\"", html);
        Assert.Contains("href=\"/Stats/Batting?stat=g\"", html);
        Assert.Contains("<caption class=\"visually-hidden\">", html);
        Assert.Contains("scope=\"col\"", html);
        Assert.DoesNotContain("href=\"#\"", html);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Players?letter=A")]
    [InlineData("/HallOfFame")]
    [InlineData("/Stats/Pitching?stat=w")]
    [InlineData("/Teams/Season/NYA/AL/1927")]
    public async Task Pages_HaveNoPlaceholderHrefs(string url)
    {
        var html = await GetStringAsync(url);

        Assert.DoesNotContain("href=\"#\"", html);
    }
}
