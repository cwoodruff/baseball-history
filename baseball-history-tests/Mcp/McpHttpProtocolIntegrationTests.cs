using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace baseball_history_tests.Mcp;

[CollectionDefinition("mcp-http-host")]
public sealed class McpHttpHostCollection : ICollectionFixture<McpHttpHostFixture>;

[Collection("mcp-http-host")]
public sealed class McpHttpProtocolIntegrationTests(McpHttpHostFixture fixture)
{
    [Fact]
    public async Task HttpHost_HealthzEndpoint_ReturnsHealthy()
    {
        var response = await fixture.Client.GetAsync("/healthz");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task HttpHost_Initialize_ReturnsServerInfoWithoutSession()
    {
        var (response, document) = await fixture.SendInitializeAsync();
        using (document)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Stateless Streamable HTTP (2026-07-28 revision, SEP-2567): the
            // server must not mint session ids.
            Assert.False(response.Headers.Contains("Mcp-Session-Id"), "Stateless server must not issue an Mcp-Session-Id header.");
            Assert.Equal(
                "baseball-history-mcp",
                document.RootElement.GetProperty("result").GetProperty("serverInfo").GetProperty("name").GetString());
        }
    }

    [Fact]
    public async Task HttpHost_ToolsList_ReturnsShippedToolsStateless()
    {
        using var toolsDocument = await fixture.SendRequestAsync("tools/list", new { }, sessionId: null);

        var toolNames = toolsDocument.RootElement
            .GetProperty("result")
            .GetProperty("tools")
            .EnumerateArray()
            .Select(tool => tool.GetProperty("name").GetString())
            .ToHashSet();

        Assert.Contains("search_players", toolNames);
        Assert.Contains("get_server_diagnostics", toolNames);
    }
}

public sealed class McpHttpHostFixture : IAsyncLifetime
{
    private Process? _process;
    private int _nextId;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var port = GetFreePort();
        var baseAddress = new Uri($"http://127.0.0.1:{port}");

        var projectPath = Path.Combine(GetSolutionDirectory(), "baseball-history-mcp", "baseball-history-mcp.csproj");
        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{projectPath}\" --no-build --no-launch-profile")
        {
            WorkingDirectory = GetSolutionDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.Environment["ConnectionStrings__Lahman"] = TestDatabaseFactory.GetConnectionString();
        startInfo.Environment["ASPNETCORE_URLS"] = baseAddress.ToString();
        startInfo.Environment["Logging__LogLevel__Default"] = "Warning";

        _process = Process.Start(startInfo) ?? throw new InvalidOperationException("Unable to start baseball-history-mcp in HTTP mode.");
        Client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(20) };

        await WaitForHealthyAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();

        if (_process is not null)
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
            }

            _process.Dispose();
            _process = null;
        }

        return Task.CompletedTask;
    }

    public async Task<(HttpResponseMessage Response, JsonDocument Document)> SendInitializeAsync()
    {
        var response = await PostJsonRpcAsync(
            "initialize",
            new
            {
                protocolVersion = "2025-03-26",
                capabilities = new { },
                clientInfo = new { name = "baseball-history-tests-http", version = "1.0" }
            },
            sessionId: null);

        return (response, await ReadJsonRpcDocumentAsync(response));
    }

    public async Task<JsonDocument> SendRequestAsync(string method, object parameters, string? sessionId)
    {
        var response = await PostJsonRpcAsync(method, parameters, sessionId);
        return await ReadJsonRpcDocumentAsync(response);
    }

    private async Task<HttpResponseMessage> PostJsonRpcAsync(string method, object parameters, string? sessionId)
    {
        var payload = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = Interlocked.Increment(ref _nextId),
            method,
            @params = parameters
        });

        using var request = CreateJsonRpcRequest(payload, sessionId);
        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private HttpRequestMessage CreateJsonRpcRequest(string payload, string? sessionId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Accept", "application/json, text/event-stream");
        if (sessionId is not null)
        {
            request.Headers.Add("Mcp-Session-Id", sessionId);
        }

        return request;
    }

    private static async Task<JsonDocument> ReadJsonRpcDocumentAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            var dataLine = body.Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .FirstOrDefault(line => line.StartsWith("data: ", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"No SSE data line in response:{Environment.NewLine}{body}");
            return JsonDocument.Parse(dataLine["data: ".Length..]);
        }

        return JsonDocument.Parse(body);
    }

    private async Task WaitForHealthyAsync()
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            if (_process is { HasExited: true })
            {
                var stderr = await _process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"MCP HTTP host exited during startup.{Environment.NewLine}{stderr}");
            }

            try
            {
                using var response = await Client.GetAsync("/healthz");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new TimeoutException("MCP HTTP host did not become healthy within 30 seconds.", lastError);
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string GetSolutionDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
