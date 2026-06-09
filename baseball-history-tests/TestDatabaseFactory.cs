using baseball_history_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace baseball_history_tests;

internal static class TestDatabaseFactory
{
    private static readonly Lazy<string> ConnectionString = new(LoadConnectionString);

    internal static BaseballDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BaseballDbContext>()
            .UseNpgsql(ConnectionString.Value)
            .Options;

        return new BaseballDbContext(options);
    }

    private static string LoadConnectionString()
    {
        var testDir = AppContext.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
        var webProjectDir = Path.Combine(solutionDir, "baseball-history-web");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(webProjectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Lahman");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains('<'))
        {
            throw new InvalidOperationException(
                "Tests require ConnectionStrings:Lahman from user-secrets or environment variables.");
        }

        return connectionString;
    }
}
