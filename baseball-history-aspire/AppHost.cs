var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.baseball_history_web>("baseball-history-web")
    .WithReference(cache);

builder.Build().Run();