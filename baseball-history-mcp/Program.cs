using System.Reflection;
using baseball_history_mcp.Configuration;
using baseball_history_mcp.Metadata;
using baseball_history_mcp.Querying;
using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;

namespace baseball_history_mcp;

internal static class McpHostProgram
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.Configuration.AddUserSecrets<UserSecretsMarker>(optional: true);

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
        var mcpOptions = BaseballMcpOptionsValidator.Validate(
            builder.Configuration.GetSection(BaseballMcpOptions.SectionName).Get<BaseballMcpOptions>() ?? new BaseballMcpOptions());

        var connectionString = builder.Configuration.GetConnectionString("Lahman");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains('<'))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Lahman must be set via user-secrets, environment variables, or Azure App Service configuration.");
        }

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton(Options.Create(mcpOptions));
        builder.Services.AddPooledDbContextFactory<BaseballDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.CommandTimeout(mcpOptions.QueryTimeoutSeconds).EnableRetryOnFailure())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        builder.Services.AddSingleton(new ServerBuildMetadata(
            Name: "baseball-history-mcp",
            Title: "Baseball History MCP",
            Version: version,
            Description: "Read-only Lahman data access for players, franchises, leaderboards, and safe server diagnostics."));
        builder.Services.AddSingleton<BaseballMcpMetadataService>();

        builder.Services.AddSingleton<IHallOfFameReadService, HallOfFameReadService>();
        builder.Services.AddSingleton<IPlayerReadService, PlayerReadService>();
        builder.Services.AddSingleton<IFranchiseReadService, FranchiseReadService>();
        builder.Services.AddSingleton<ITeamReadService, TeamReadService>();
        builder.Services.AddSingleton<ILeaderboardReadService, LeaderboardReadService>();
        builder.Services.AddSingleton<ISalaryReadService, SalaryReadService>();

        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "baseball-history-mcp",
                    Version = version,
                    Title = "Baseball History MCP",
                    Description = "Read-only Lahman data access for players, franchises, leaderboards, and safe server diagnostics."
                };
                options.ServerInstructions =
                    "Use these read-only baseball history tools for player lookup, franchise lookup, deterministic team-season reads, curated leaderboards, Hall of Fame history, salary history, workflow guidance, and runtime diagnostics. Start with baseball-history://server/workflow-guide for question routing, then use baseball-history://server/info, baseball-history://server/stats-catalog, baseball-history://server/diagnostics, baseball-history://hall-of-fame/guide, baseball-history://salary/guide, or the get_server_diagnostics tool as needed. This server never mutates data.";
            })
            .WithStdioServerTransport()
            .WithResourcesFromAssembly()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }
}
