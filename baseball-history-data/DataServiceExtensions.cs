using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaseballHistory.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
    {
        // Singleton options lifetime allows consumers (e.g. the MCP server) to also
        // register a pooled IDbContextFactory, which resolves options from the root scope.
        services.AddDbContext<BaseballDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddScoped<ILeaderboardQueryService, LeaderboardQueryService>();

        return services;
    }
}
