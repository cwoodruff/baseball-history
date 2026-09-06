using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace baseball_history_mcp;

internal static class McpHostProgram
{
    private const string DefaultHttpUrl = "http://localhost:5190";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.AddBaseballMcpServer()
            .WithHttpTransport(options =>
                // Explicit: the 2026-07-28 MCP revision (SEP-2567) removed HTTP
                // sessions, and this read-only server needs no session state.
                options.Stateless = true);

        if (string.IsNullOrWhiteSpace(builder.Configuration["urls"])
            && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
        {
            builder.WebHost.UseUrls(DefaultHttpUrl);
        }

        var app = builder.Build();

        app.MapDefaultEndpoints();
        app.MapMcp("/");

        await app.RunAsync();
    }
}
