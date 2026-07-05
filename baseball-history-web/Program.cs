using System.IO.Compression;
using baseball_history_web.Api;
using baseball_history_web.Models;
using baseball_history_web.Services;
using htmxRazor.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("Lahman");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains('<'))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Lahman must be set via user-secrets, environment variables, or Azure App Service configuration.");
}

builder.Services.AddDbContext<BaseballDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure())
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
builder.Services.AddScoped<PlayerDetailService>();
builder.Services.AddHostedService<PlayerCacheService>();
builder.Services.AddRazorPages();
builder.Services.AddhtmxRazor();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UsehtmxRazor();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

// REST API
app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}
app.MapApiEndpoints();

app.Run();

public partial class Program;
