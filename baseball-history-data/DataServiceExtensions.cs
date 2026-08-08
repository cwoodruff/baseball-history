using BaseballHistory.Data.Models;
using BaseballHistory.Data.Querying;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaseballHistory.Data;

public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BaseballDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<ILeaderboardQueryService, LeaderboardQueryService>();

        return services;
    }
}
