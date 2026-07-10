using baseball_history_mcp.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace baseball_history_mcp;

internal static class McpHostProgram
{
    private const string DefaultHttpUrl = "http://localhost:5190";

    public static async Task Main(string[] args)
    {
        var transport = ResolveTransport(args);

        if (transport.HttpEnabled)
        {
            await RunHttpServerAsync(args, transport);
            return;
        }

        await RunStdioServerAsync(args, transport);
    }

    private static async Task RunStdioServerAsync(string[] args, McpTransportInfo transport)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // stdout carries the JSON-RPC protocol stream, so all logging goes to stderr.
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        builder.AddBaseballMcpServer(transport)
            .WithStdioServerTransport();

        await builder.Build().RunAsync();
    }

    private static async Task RunHttpServerAsync(string[] args, McpTransportInfo transport)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.AddBaseballMcpServer(transport)
            .WithHttpTransport();

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

    private static McpTransportInfo ResolveTransport(string[] args)
    {
        var value = ReadTransportArgument(args)
            ?? Environment.GetEnvironmentVariable("MCP_TRANSPORT")
            ?? Environment.GetEnvironmentVariable("Mcp__Transport");

        return value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "stdio" => new McpTransportInfo(McpTransportMode.Stdio),
            "http" => new McpTransportInfo(McpTransportMode.Http),
            var unknown => throw new InvalidOperationException(
                $"Unknown MCP transport '{unknown}'. Supported values are 'stdio' and 'http'.")
        };
    }

    private static string? ReadTransportArgument(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--transport", StringComparison.OrdinalIgnoreCase))
            {
                return i + 1 < args.Length
                    ? args[i + 1]
                    : throw new InvalidOperationException("--transport requires a value of 'stdio' or 'http'.");
            }

            if (args[i].StartsWith("--transport=", StringComparison.OrdinalIgnoreCase))
            {
                return args[i]["--transport=".Length..];
            }
        }

        return null;
    }
}
