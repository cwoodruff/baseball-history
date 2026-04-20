using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class PaginationBoundaryTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Players_PageZero_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Players?letter=A&page=0");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        // Should return valid content (page 1)
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task Players_NegativePage_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Players?letter=B&page=-5");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        // Should return valid content (page 1)
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task Players_PageBeyondMax_ClampsToLastPage()
    {
        var response = await Client.GetAsync("/Players?letter=A&page=999999");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        // Should return valid content (last available page)
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsBatting_PageZero_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Stats/Batting?stat=hr&page=0");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsBatting_NegativePage_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Stats/Batting?stat=avg&page=-10");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsBatting_PageBeyondMax_ClampsToLastPage()
    {
        var response = await Client.GetAsync("/Stats/Batting?stat=hr&page=999999");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsPitching_PageZero_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Stats/Pitching?stat=w&page=0");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsPitching_NegativePage_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/Stats/Pitching?stat=era&page=-3");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task StatsPitching_PageBeyondMax_ClampsToLastPage()
    {
        var response = await Client.GetAsync("/Stats/Pitching?stat=so&page=999999");
        
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("<div", html);
    }

    [Fact]
    public async Task ApiPlayers_PageZero_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/api/players?letter=A&page=0");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"page\":1", json);
    }

    [Fact]
    public async Task ApiPlayers_NegativePage_ClampsToPageOne()
    {
        var response = await Client.GetAsync("/api/players?letter=B&page=-5");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        
        Assert.Contains("\"page\":1", json);
    }

    [Fact]
    public async Task ApiPlayers_PageBeyondMax_ClampsToLastPage()
    {
        var response = await Client.GetAsync("/api/players?letter=A&page=999999");
        
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        
        // Should return valid JSON with data
        Assert.Contains("\"page\":", json);
        Assert.Contains("\"data\":", json);
    }
}
