using Microsoft.AspNetCore.Mvc.Testing;

namespace baseball_history_tests.Pages;

public class McpDocsPageTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task McpDocs_FullPage_RendersTitleToolsAndResources()
    {
        var html = await GetStringAsync("/McpDocs");

        Assert.Contains("MCP Server", html);
        Assert.Contains("search_players", html);
        Assert.Contains("get_batting_leaders", html);
        Assert.Contains("get_player_postseason", html);
        Assert.Contains("get_player_fielding", html);
        Assert.Contains("search_parks", html);
        Assert.Contains("get_park", html);
        Assert.Contains("get_manager", html);
        Assert.Contains("17 read-only tools", html);
        Assert.Contains("get_server_diagnostics", html);
        // What's-new section links the expanded surface to the new site features
        Assert.Contains("id=\"whats-new\"", html);
        Assert.Contains("href=\"/Parks\"", html);
        Assert.Contains("href=\"/Managers\"", html);
        Assert.Contains("href=\"/NegroLeagues\"", html);
        Assert.Contains("href=\"/AllStar\"", html);
        Assert.Contains("baseball-history://server/info", html);
        Assert.Contains("ConnectionStrings__Lahman", html);
    }

    [Fact]
    public async Task ShellHeader_IncludesMcpNavLink()
    {
        // The shell header renders on every page; fetch the MCP page itself so the
        // test's intent (the nav contains an MCP link) is self-evident.
        var html = await GetStringAsync("/McpDocs");

        Assert.Contains("href=\"/McpDocs\"", html);
        Assert.Contains(">MCP</a>", html);
    }
}
