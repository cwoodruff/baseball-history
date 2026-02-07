var builder = DistributedApplication.CreateBuilder(args);

var sqlite = builder.AddSqlite("sqlite", "", "lahman.db");

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.baseball_history_web>("baseball-history-web")
    .WithReference(sqlite)
    .WithReference(cache);

builder.Build().Run();