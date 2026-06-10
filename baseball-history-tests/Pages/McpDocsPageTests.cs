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
        Assert.Contains("get_server_diagnostics", html);
        Assert.Contains("baseball-history://server/info", html);
        Assert.Contains("ConnectionStrings__Lahman", html);
    }

    [Fact]
    public async Task ShellHeader_IncludesMcpNavLink()
    {
        var html = await GetStringAsync("/ApiDocs");

        Assert.Contains("href=\"/McpDocs\"", html);
        Assert.Contains(">MCP</a>", html);
    }
}
