using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

namespace baseball_history_tests.Pages;

public class PageRoutingIntegrationTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Players_WithoutHtmx_ReturnsFullPage()
    {
        var response = await Client.GetAsync("/Players?letter=A");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
        Assert.Contains("</html>", html);
    }

    [Fact]
    public async Task Players_WithHtmxRequest_ReturnsPartial()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Players?letter=B");
        request.Headers.Add("HX-Request", "true");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.DoesNotContain("<html", html);
        // Should contain partial content markers
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task Players_WithHtmxBoosted_ReturnsFullPage()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Players?letter=C");
        request.Headers.Add("HX-Request", "true");
        request.Headers.Add("HX-Boosted", "true");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<!DOCTYPE html>", html);
    }

    [Fact]
    public async Task Search_WithoutQuery_ReturnsPartial()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Search");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        // Search always returns partial
        Assert.DoesNotContain("<!DOCTYPE html>", html);
    }

    [Fact]
    public async Task Search_WithQuery_ReturnsResults()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Search?q=Ruth");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("<!DOCTYPE html>", html);
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsBatting_WithoutHtmx_ReturnsFullPage()
    {
        var response = await Client.GetAsync("/Stats/Batting?stat=hr");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
    }

    [Fact]
    public async Task StatsBatting_WithHtmxRequest_ReturnsPartial()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Stats/Batting?stat=avg");
        request.Headers.Add("HX-Request", "true");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("<!DOCTYPE html>", html);
    }

    [Fact]
    public async Task StatsPitching_WithoutHtmx_ReturnsFullPage()
    {
        var response = await Client.GetAsync("/Stats/Pitching?stat=w");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
    }

    [Fact]
    public async Task StatsPitching_WithHtmxRequest_ReturnsPartial()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Stats/Pitching?stat=era");
        request.Headers.Add("HX-Request", "true");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("<!DOCTYPE html>", html);
    }

    [Fact]
    public async Task Teams_WithoutHtmx_ReturnsFullPage()
    {
        var response = await Client.GetAsync("/Teams");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
    }

    [Fact]
    public async Task Teams_WithHtmxRequest_ReturnsPartial()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Teams?league=AL");
        request.Headers.Add("HX-Request", "true");
        
        var response = await Client.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("<!DOCTYPE html>", html);
    }

    [Fact]
    public async Task About_RendersHtmxRazorAssets_AndProofComponent()
    {
        var response = await Client.GetAsync("/About");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("/_rhx/css/rhx-core.css", html);
        Assert.Contains("/_rhx/js/rhx-core.js", html);
        Assert.Contains("/_rhx/css/components/rhx-button.css", html);
        Assert.DoesNotContain("<rhx-button", html);
        Assert.Contains("View Source on GitHub", html);
    }

    [Fact]
    public async Task HtmxRazorFoundationAsset_IsServedFromRhxPath()
    {
        var response = await Client.GetAsync("/_rhx/css/rhx-core.css");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }
}
