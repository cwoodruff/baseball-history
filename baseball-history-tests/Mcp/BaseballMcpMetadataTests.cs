using System.Text.Json;
using baseball_history_mcp.Configuration;
using baseball_history_mcp.Metadata;
using baseball_history_mcp.Resources;
using baseball_history_mcp.Tools;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace baseball_history_tests.Mcp;

public class BaseballMcpMetadataTests
{
    [Fact]
    public void AppSettings_DeclareTimeoutAndPageSizeCaps()
    {
        var projectDir = GetProjectDirectory("baseball-history-mcp");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var options = configuration.GetSection(BaseballMcpOptions.SectionName).Get<BaseballMcpOptions>();

        Assert.NotNull(options);
        Assert.Equal(30, options.QueryTimeoutSeconds);
        Assert.Equal(100, options.Limits.PlayerSearchPageSizeMax);
        Assert.Equal(50, options.Limits.FranchiseListPageSizeMax);
        Assert.Equal(100, options.Limits.HallOfFamePageSizeMax);
        Assert.Equal(100, options.Limits.LeaderboardPageSizeMax);
        Assert.Equal(20, options.Limits.SalaryHistorySeasonCountMax);
        Assert.Equal(25, options.Limits.TeamPayrollPlayerCountMax);
    }

    [Fact]
    public async Task GetServerDiagnosticsAsync_ReturnsSafeRuntimePosture()
    {
        var service = CreateMetadataService();

        var diagnostics = await service.GetServerDiagnosticsAsync();
        var json = JsonSerializer.Serialize(diagnostics);

        Assert.True(diagnostics.ConnectionStringConfigured);
        Assert.True(diagnostics.DatabaseReachable);
        Assert.Equal("ConnectionStrings:Lahman", diagnostics.ConnectionStringKey);
        Assert.Equal(30, diagnostics.Limits.QueryTimeoutSeconds);
        Assert.Equal(50, diagnostics.Limits.FranchiseListPageSizeMax);
        Assert.Equal(100, diagnostics.Limits.HallOfFamePageSizeMax);
        Assert.Equal(20, diagnostics.Limits.SalaryHistorySeasonCountMax);
        Assert.Equal(25, diagnostics.Limits.TeamPayrollPlayerCountMax);
        Assert.True(diagnostics.ToolCount >= 13);
        Assert.True(diagnostics.ResourceCount >= 6);
        Assert.DoesNotContain("Password=", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host=", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetStatsCatalogAsync_DescribesSupportedStatsAndYearSpan()
    {
        var service = CreateMetadataService();

        var catalog = await service.GetStatsCatalogAsync();

        Assert.NotNull(catalog.SupportedYearSpan);
        Assert.True(catalog.SupportedYearSpan!.StartYear <= catalog.SupportedYearSpan.EndYear);

        var batting = Assert.Single(catalog.Categories, category => category.Category == "batting");
        var pitching = Assert.Single(catalog.Categories, category => category.Category == "pitching");

        Assert.Contains(batting.Stats, stat => stat.Key == "ops");
        Assert.Contains(pitching.Stats, stat => stat.Key == "era" && stat.SortDirection == "ascending");
    }

    [Fact]
    public async Task ResourceAndToolSurfaces_ReturnDiscoverableMetadata()
    {
        var metadataService = CreateMetadataService();
        var resources = new BaseballReferenceResources(metadataService);
        var diagnosticsTool = new BaseballServerDiagnosticsTools(metadataService);

        using var infoDocument = JsonDocument.Parse(await resources.GetServerInfoAsync());
        using var catalogDocument = JsonDocument.Parse(await resources.GetStatsCatalogAsync());
        using var transportPolicyDocument = JsonDocument.Parse(await resources.GetTransportPolicyAsync());
        var workflowGuide = await resources.GetWorkflowGuideAsync();
        var diagnostics = await diagnosticsTool.GetServerDiagnosticsAsync();

        Assert.Equal("baseball-history-mcp", infoDocument.RootElement.GetProperty("name").GetString());
        Assert.False(infoDocument.RootElement.GetProperty("httpTransportEnabled").GetBoolean());
        Assert.Contains("No-go for v1", infoDocument.RootElement.GetProperty("httpTransportRecommendation").GetString());
        Assert.Equal("ConnectionStrings:Lahman", infoDocument.RootElement.GetProperty("connectionStringKey").GetString());
        Assert.Equal(50, infoDocument.RootElement.GetProperty("limits").GetProperty("franchiseListPageSizeMax").GetInt32());
        Assert.Equal(100, infoDocument.RootElement.GetProperty("limits").GetProperty("hallOfFamePageSizeMax").GetInt32());
        Assert.Equal(20, infoDocument.RootElement.GetProperty("limits").GetProperty("salaryHistorySeasonCountMax").GetInt32());
        Assert.Equal(25, infoDocument.RootElement.GetProperty("limits").GetProperty("teamPayrollPlayerCountMax").GetInt32());
        Assert.Contains(
            infoDocument.RootElement.GetProperty("resources").EnumerateArray().Select(resource => resource.GetProperty("uri").GetString()),
            uri => uri == "baseball-history://server/diagnostics");
        Assert.Contains(
            infoDocument.RootElement.GetProperty("resources").EnumerateArray().Select(resource => resource.GetProperty("uri").GetString()),
            uri => uri == "baseball-history://server/transport-policy");
        Assert.Contains(
            infoDocument.RootElement.GetProperty("resources").EnumerateArray().Select(resource => resource.GetProperty("uri").GetString()),
            uri => uri == "baseball-history://guides/workflows");
        Assert.Contains(
            infoDocument.RootElement.GetProperty("toolNames").EnumerateArray().Select(tool => tool.GetString()),
            tool => tool == "get_salary_leaders");
        Assert.Equal("Teams.yearID", catalogDocument.RootElement.GetProperty("supportedYearSpan").GetProperty("source").GetString());
        Assert.False(transportPolicyDocument.RootElement.GetProperty("httpEnabled").GetBoolean());
        Assert.Contains(
            transportPolicyDocument.RootElement.GetProperty("sdkGuidance").EnumerateArray().Select(entry => entry.GetString()),
            entry => entry!.Contains("AllowedHosts", StringComparison.Ordinal));
        Assert.Contains(
            transportPolicyDocument.RootElement.GetProperty("sdkGuidance").EnumerateArray().Select(entry => entry.GetString()),
            entry => entry!.Contains("CORS", StringComparison.Ordinal));
        Assert.Contains("get_team_season", workflowGuide);
        Assert.True(diagnostics.DatabaseReachable);
    }

    private static BaseballMcpMetadataService CreateMetadataService(BaseballMcpOptions? options = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Lahman"] = TestDatabaseFactory.GetConnectionString()
            })
            .Build();

        return new BaseballMcpMetadataService(
            new TestDbContextFactory(),
            configuration,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(options ?? new BaseballMcpOptions()),
            new ServerBuildMetadata(
                Name: "baseball-history-mcp",
                Title: "Baseball History MCP",
                Version: "1.0.0-test",
                Description: "Read-only Lahman data access for players, franchises, team seasons, leaderboards, Hall of Fame, salaries, diagnostics, and guide resources."));
    }

    private static string GetProjectDirectory(string projectName)
    {
        var testDir = AppContext.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
        return Path.Combine(solutionDir, projectName);
    }

    private sealed class TestDbContextFactory : IDbContextFactory<BaseballDbContext>
    {
        public BaseballDbContext CreateDbContext() => TestDatabaseFactory.CreateContext();

        public Task<BaseballDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
