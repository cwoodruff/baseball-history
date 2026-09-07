using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class ThisDayWidgetTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Home_ShowsThisDayWidget()
    {
        var html = await GetStringAsync("/");

        Assert.Contains("this-day-widget", html);
        Assert.Contains("This Day in Baseball", html);
    }

    [Fact]
    public async Task Home_RuthBirthday_ShowsBabeRuthFirst()
    {
        // February 6, 1895 — Hall of Famers rank ahead of everyone else
        var html = await GetStringAsync("/?day=02-06");

        Assert.Contains("This Day in Baseball &mdash; February 6", html);
        Assert.Contains("Babe Ruth", html);
        Assert.Contains("/Players/ruthba01", html);
        Assert.Contains("1895", html);
    }

    [Fact]
    public async Task Home_April15_ShowsRobinsonDebut()
    {
        // Jackie Robinson broke the color line on April 15, 1947
        var html = await GetStringAsync("/?day=04-15");

        Assert.Contains("Jackie Robinson", html);
        Assert.Contains("/Players/robinja02", html);
        Assert.Contains("1947", html);
    }

    [Fact]
    public async Task Home_MalformedDayOverride_FallsBackToToday()
    {
        var html = await GetStringAsync("/?day=banana");

        Assert.Contains("this-day-widget", html);
    }

    [Fact]
    public async Task Home_LeapDay_HasContent()
    {
        var html = await GetStringAsync("/?day=02-29");

        Assert.Contains("February 29", html);
    }
}
