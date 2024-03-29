using Microsoft.Restier.EntityFrameworkCore;
using odata_baseballstats.Data;

namespace odata_baseballstats.Controllers;

public class BaseballStatsApi(IServiceProvider serviceProvider)
    : EntityFrameworkApi<BaseballStats2022Context>(serviceProvider);