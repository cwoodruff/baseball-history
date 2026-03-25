using System.IO.Compression;
using baseball_history_web.Models;
using baseball_history_web.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Read connection string from config (overridable in Azure App Settings)
var connectionString = builder.Configuration.GetConnectionString("Lahman")
                       ?? "Data Source=lahman.db;Mode=ReadOnly;Cache=Shared";

builder.Services.AddDbContext<BaseballDbContext>(options =>
    options.UseSqlite(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    [
        "text/html",
        "application/javascript",
        "text/css",
        "application/json",
        "text/plain",
        "image/svg+xml"
    ]);
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

// Add services to the container.
builder.Services.AddSingleton<TeamColorService>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Enable SQLite WAL mode using a separate writable connection (WAL is persistent, only needs to be set once)
{
    using var walConnection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=lahman.db");
    walConnection.Open();
    using var cmd = walConnection.CreateCommand();
    cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
    cmd.ExecuteNonQuery();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
