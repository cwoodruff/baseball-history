var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("baseball-data");

var path = builder.AppHostDirectory;

var sqltext = String.Concat(
    File.ReadAllText(Path.Combine(path, @"../scripts/create-database.sql")),
    " ",
    File.ReadAllText(Path.Combine(path, @"../scripts/baseball_history_schema.sql")),
    " "
);

var db = sql.AddDatabase("baseballhistory")
    .WithCreationScript(sqltext);

builder.AddProject<Projects.baseball_history_web>("web")
    .WaitFor(db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/");

builder.Build().Run();
