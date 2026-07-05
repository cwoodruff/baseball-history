using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class ErrorPagesAndPolishTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    // --- #44: custom 404 / error pages ---

    [Fact]
    public async Task UnknownRoute_ReturnsBranded404Page()
    {
        var response = await Client.GetAsync("/this-page-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Swing and a Miss!", html);
        Assert.Contains("href=\"/Players\"", html);
        Assert.Contains("href=\"/HallOfFame\"", html);
    }

    [Fact]
    public async Task UnknownPlayer_ReturnsBranded404Page()
    {
        var response = await Client.GetAsync("/Players/nosuchplayer999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Swing and a Miss!", html);
    }

    [Fact]
    public async Task ApiUnknownRoute_StaysBodylessFor404()
    {
        var response = await Client.GetAsync("/api/players/nosuchplayer999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Swing and a Miss!", body);
    }

    [Fact]
    public async Task ErrorPage_IsFriendly_WithoutDevModeText()
    {
        var html = await GetStringAsync("/Error");

        Assert.Contains("Rain Delay", html);
        Assert.DoesNotContain("Development Mode", html);
        Assert.DoesNotContain("ASPNETCORE_ENVIRONMENT", html);
    }

    // --- #45: dark mode ---

    [Fact]
    public async Task Layout_AppliesThemeBeforeFirstPaint()
    {
        var html = await GetStringAsync("/");

        Assert.Contains("bb-theme", html);
        Assert.Contains("prefers-color-scheme: dark", html);
        Assert.Contains("data-bs-theme", html);
    }

    [Fact]
    public async Task Navbar_HasThemeToggle()
    {
        var html = await GetStringAsync("/");

        Assert.Contains("id=\"theme-toggle\"", html);
        Assert.Contains("aria-label=\"Toggle dark mode\"", html);
    }

    [Fact]
    public async Task SiteCss_ContainsDarkThemeOverrides()
    {
        var css = await GetStringAsync("/css/site.css");

        Assert.Contains("[data-bs-theme=\"dark\"]", css);
        Assert.Contains(".theme-icon-dark", css);
    }

    // --- #46: loading feedback ---

    [Fact]
    public async Task PlayersPage_HasLoadingIndicatorWiring()
    {
        var html = await GetStringAsync("/Players");

        Assert.Contains("hx-indicator=\"#players-loading\"", html);
        Assert.Contains("id=\"players-loading\"", html);
    }

    [Fact]
    public async Task TeamsPage_HasLoadingIndicatorWiring()
    {
        var html = await GetStringAsync("/Teams");

        Assert.Contains("hx-indicator=\"#teams-loading\"", html);
        Assert.Contains("id=\"teams-loading\"", html);
    }

    [Fact]
    public async Task Layout_HasModalLoadingOverlay()
    {
        var html = await GetStringAsync("/");

        Assert.Contains("id=\"modal-loading\"", html);
        Assert.Contains("modal-loading-overlay", html);
    }
}
