using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace baseball_history_tests.Mcp;

[CollectionDefinition("mcp-host")]
public sealed class McpHostCollection : ICollectionFixture<McpHostFixture>;

[Collection("mcp-host")]
public sealed class McpProtocolIntegrationTests(McpHostFixture fixture)
{
    [Fact]
    public async Task Host_ListsShippedToolsAndResources()
    {
        using var toolsResponse = await fixture.RequestAsync("tools/list", new { });
        using var resourcesResponse = await fixture.RequestAsync("resources/list", new { });

        var tools = toolsResponse.RootElement.GetProperty("result").GetProperty("tools").EnumerateArray().ToList();
        var resources = resourcesResponse.RootElement.GetProperty("result").GetProperty("resources").EnumerateArray().ToList();

        var toolNames = tools.Select(tool => tool.GetProperty("name").GetString()).ToHashSet();
        var resourceUris = resources.Select(resource => resource.GetProperty("uri").GetString()).ToHashSet();

        Assert.Subset(new HashSet<string?>
        {
            "search_players",
            "get_player",
            "list_franchises",
            "get_franchise",
            "get_team_season",
            "get_batting_leaders",
            "get_pitching_leaders",
            "list_hall_of_fame_inductees",
            "get_hall_of_fame_voting_history",
            "get_player_salary_history",
            "get_salary_leaders",
            "get_server_diagnostics"
        }, toolNames);

        Assert.Subset(new HashSet<string?>
        {
            "baseball-history://server/info",
            "baseball-history://server/workflow-guide",
            "baseball-history://server/stats-catalog",
            "baseball-history://server/diagnostics",
            "baseball-history://hall-of-fame/guide",
            "baseball-history://salary/guide"
        }, resourceUris);

        var teamSeasonTool = tools.Single(tool => tool.GetProperty("name").GetString() == "get_team_season");
        Assert.Contains(
            teamSeasonTool.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()),
            value => value == "teamId");
        Assert.Contains(
            teamSeasonTool.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()),
            value => value == "league");
        Assert.Contains(
            teamSeasonTool.GetProperty("inputSchema").GetProperty("required").EnumerateArray().Select(value => value.GetString()),
            value => value == "year");

        var workflowGuide = resources.Single(resource => resource.GetProperty("uri").GetString() == "baseball-history://server/workflow-guide");
        Assert.Equal("application/json", workflowGuide.GetProperty("mimeType").GetString());
    }

    [Fact]
    public async Task Host_ReadsMetadataAndGuideResources()
    {
        using var infoResponse = await fixture.RequestAsync("resources/read", new { uri = "baseball-history://server/info" });
        using var guideResponse = await fixture.RequestAsync("resources/read", new { uri = "baseball-history://server/workflow-guide" });

        using var infoPayload = JsonDocument.Parse(McpHostFixture.ReadResourceText(infoResponse));
        using var workflowGuide = JsonDocument.Parse(McpHostFixture.ReadResourceText(guideResponse));

        Assert.Equal("baseball-history-mcp", infoPayload.RootElement.GetProperty("name").GetString());
        Assert.Equal("http", infoPayload.RootElement.GetProperty("transport").GetString());
        Assert.Contains(
            infoPayload.RootElement.GetProperty("toolNames").EnumerateArray().Select(value => value.GetString()),
            value => value == "get_salary_leaders");
        Assert.Equal(50, infoPayload.RootElement.GetProperty("limits").GetProperty("hallOfFamePageSizeMax").GetInt32());
        Assert.Equal(40, infoPayload.RootElement.GetProperty("limits").GetProperty("salaryHistorySeasonsMax").GetInt32());
        Assert.Equal(50, infoPayload.RootElement.GetProperty("limits").GetProperty("salaryLeaderboardPageSizeMax").GetInt32());
        Assert.Contains(
            infoPayload.RootElement.GetProperty("resources").EnumerateArray().Select(value => value.GetProperty("uri").GetString()),
            value => value == "baseball-history://server/workflow-guide");
        Assert.Contains(
            workflowGuide.RootElement.GetProperty("workflows").EnumerateArray().Select(value => value.GetProperty("name").GetString()),
            value => value == "salary-history-and-leaders");
    }

    [Fact]
    public async Task Host_CallsDiscoveryAndLeaderboardToolsTheWayClientsDo()
    {
        using var searchResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "search_players", arguments = new { query = "Ruth", pageSize = 1 } });
        using var searchPayload = McpHostFixture.ReadToolPayload(searchResponse);
        var playerId = searchPayload.RootElement.GetProperty("items")[0].GetProperty("playerId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(playerId));

        using var playerResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_player", arguments = new { playerId } });
        Assert.False(McpHostFixture.IsToolError(playerResponse), McpHostFixture.ReadToolMessage(playerResponse));
        using var playerPayload = McpHostFixture.ReadToolPayload(playerResponse);
        Assert.Equal(playerId, playerPayload.RootElement.GetProperty("playerId").GetString());

        using var franchiseResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_franchise", arguments = new { franchiseId = "NYY" } });
        Assert.False(McpHostFixture.IsToolError(franchiseResponse), McpHostFixture.ReadToolMessage(franchiseResponse));
        using var franchisePayload = McpHostFixture.ReadToolPayload(franchiseResponse);
        Assert.True(franchisePayload.RootElement.GetProperty("seasons").GetArrayLength() > 0);

        using var teamSeasonResponse = await fixture.RequestAsync(
            "tools/call",
            new
            {
                name = "get_team_season",
                arguments = new
                {
                    teamId = "NYA",
                    league = "AL",
                    year = 2020
                }
            });
        Assert.False(McpHostFixture.IsToolError(teamSeasonResponse), McpHostFixture.ReadToolMessage(teamSeasonResponse));
        using var teamSeasonPayload = McpHostFixture.ReadToolPayload(teamSeasonResponse);
        Assert.Equal("NYA", teamSeasonPayload.RootElement.GetProperty("teamId").GetString());
        Assert.True(teamSeasonPayload.RootElement.GetProperty("batters").GetArrayLength() > 0);

        using var battingResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_batting_leaders", arguments = new { stat = "hr", pageSize = 3 } });
        Assert.False(McpHostFixture.IsToolError(battingResponse), McpHostFixture.ReadToolMessage(battingResponse));
        using var battingPayload = McpHostFixture.ReadToolPayload(battingResponse);
        var battingItems = battingPayload.RootElement.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(3, battingItems.Count);
        Assert.True(battingItems[0].GetProperty("homeRuns").GetInt32() >= battingItems[1].GetProperty("homeRuns").GetInt32());

        using var pitchingResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_pitching_leaders", arguments = new { stat = "era", singleSeason = true, minInningsPitched = 100, pageSize = 3 } });
        Assert.False(McpHostFixture.IsToolError(pitchingResponse), McpHostFixture.ReadToolMessage(pitchingResponse));
        using var pitchingPayload = McpHostFixture.ReadToolPayload(pitchingResponse);
        var pitchingItems = pitchingPayload.RootElement.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(3, pitchingItems.Count);
        Assert.True(pitchingItems[0].GetProperty("era").GetDouble() <= pitchingItems[1].GetProperty("era").GetDouble());
    }

    [Fact]
    public async Task Host_CallsHallOfFameSalaryAndDiagnosticsToolsTheWayClientsDo()
    {
        using var hallResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "list_hall_of_fame_inductees", arguments = new { pageSize = 999 } });
        Assert.False(McpHostFixture.IsToolError(hallResponse), McpHostFixture.ReadToolMessage(hallResponse));
        using var hallPayload = McpHostFixture.ReadToolPayload(hallResponse);
        Assert.True(hallPayload.RootElement.GetProperty("wasPageSizeClamped").GetBoolean());
        Assert.Equal(50, hallPayload.RootElement.GetProperty("maxPageSize").GetInt32());
        var hallPlayerId = hallPayload.RootElement.GetProperty("items")[0].GetProperty("playerId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(hallPlayerId));

        using var votingResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_hall_of_fame_voting_history", arguments = new { playerId = hallPlayerId } });
        Assert.False(McpHostFixture.IsToolError(votingResponse), McpHostFixture.ReadToolMessage(votingResponse));
        using var votingPayload = McpHostFixture.ReadToolPayload(votingResponse);
        Assert.Equal(hallPlayerId, votingPayload.RootElement.GetProperty("playerId").GetString());
        Assert.True(votingPayload.RootElement.GetProperty("votingHistory").GetArrayLength() > 0);

        using var salaryLeadersResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_salary_leaders", arguments = new { year = 2016, pageSize = 999 } });
        Assert.False(McpHostFixture.IsToolError(salaryLeadersResponse), McpHostFixture.ReadToolMessage(salaryLeadersResponse));
        using var salaryLeadersPayload = McpHostFixture.ReadToolPayload(salaryLeadersResponse);
        Assert.True(salaryLeadersPayload.RootElement.GetProperty("wasPageSizeClamped").GetBoolean());
        Assert.Equal(50, salaryLeadersPayload.RootElement.GetProperty("maxPageSize").GetInt32());
        var salaryLeader = salaryLeadersPayload.RootElement.GetProperty("items")[0];
        var salaryPlayerId = salaryLeader.GetProperty("playerId").GetString();

        using var playerSalaryResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_player_salary_history", arguments = new { playerId = salaryPlayerId } });
        Assert.False(McpHostFixture.IsToolError(playerSalaryResponse), McpHostFixture.ReadToolMessage(playerSalaryResponse));
        using var playerSalaryPayload = McpHostFixture.ReadToolPayload(playerSalaryResponse);
        Assert.Equal(salaryPlayerId, playerSalaryPayload.RootElement.GetProperty("playerId").GetString());
        Assert.True(playerSalaryPayload.RootElement.GetProperty("returnedSeasonCount").GetInt32() <= 40);
        Assert.True(playerSalaryPayload.RootElement.GetProperty("careerTotal").GetInt64() > 0);

        using var diagnosticsResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_server_diagnostics", arguments = new { } });
        Assert.False(McpHostFixture.IsToolError(diagnosticsResponse), McpHostFixture.ReadToolMessage(diagnosticsResponse));
        using var diagnosticsPayload = McpHostFixture.ReadToolPayload(diagnosticsResponse);
        Assert.True(diagnosticsPayload.RootElement.GetProperty("databaseReachable").GetBoolean());
        Assert.True(diagnosticsPayload.RootElement.GetProperty("toolCount").GetInt32() >= 12);
    }

    [Fact]
    public async Task Host_InvalidToolCalls_ReturnSanitizedUsageErrors()
    {
        using var invalidBattingResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_batting_leaders", arguments = new { stat = "war" } });
        Assert.True(McpHostFixture.IsToolError(invalidBattingResponse));
        var battingMessage = McpHostFixture.ReadToolMessage(invalidBattingResponse);
        Assert.Contains("Unsupported batting stat", battingMessage);
        Assert.DoesNotContain("Password=", battingMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host=", battingMessage, StringComparison.OrdinalIgnoreCase);

        using var invalidPitchingResponse = await fixture.RequestAsync(
            "tools/call",
            new { name = "get_pitching_leaders", arguments = new { fromYear = 2001, toYear = 1999 } });
        Assert.True(McpHostFixture.IsToolError(invalidPitchingResponse));
        Assert.Contains("fromYear must be less than or equal to toYear", McpHostFixture.ReadToolMessage(invalidPitchingResponse));
    }

    [Fact]
    public async Task Host_WithPlaceholderConnectionString_FailsFast()
    {
        using var process = McpHostFixture.StartHostProcess("Host=<server>;Database=<db>;", baseAddress: null, isolateUserSecrets: true);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var exited = true;
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            exited = false;
        }

        if (!exited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        var stderr = await process.StandardError.ReadToEndAsync();

        Assert.True(exited);
        Assert.NotEqual(0, process.ExitCode);
        Assert.Contains("ConnectionStrings:Lahman", stderr);
    }
}

public sealed class McpHostFixture : IAsyncLifetime
{
    private Process? _process;
    private HttpClient? _client;
    private string _sessionId = string.Empty;
    private int _nextId;

    public async Task InitializeAsync()
    {
        var baseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}");
        _process = StartHostProcess(TestDatabaseFactory.GetConnectionString(), baseAddress);
        _client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(20) };

        await WaitForHealthyAsync();

        using var response = await PostJsonRpcAsync(
            "initialize",
            new
            {
                protocolVersion = "2025-03-26",
                capabilities = new { },
                clientInfo = new
                {
                    name = "baseball-history-tests",
                    version = "1.0"
                }
            },
            sessionId: null);
        response.EnsureSuccessStatusCode();
        _sessionId = response.Headers.GetValues("Mcp-Session-Id").Single();

        using var initializeDocument = await ReadJsonRpcDocumentAsync(response);
        Assert.Equal(
            "baseball-history-mcp",
            initializeDocument.RootElement.GetProperty("result").GetProperty("serverInfo").GetProperty("name").GetString());

        await NotifyAsync("notifications/initialized");
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _client = null;

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

    public async Task<JsonDocument> RequestAsync(string method, object? parameters)
    {
        using var response = await PostJsonRpcAsync(method, parameters, _sessionId);
        response.EnsureSuccessStatusCode();
        return await ReadJsonRpcDocumentAsync(response);
    }

    public static JsonDocument ReadToolPayload(JsonDocument response) =>
        JsonDocument.Parse(ReadToolMessage(response));

    public static string ReadToolMessage(JsonDocument response) =>
        response.RootElement.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()
        ?? throw new InvalidOperationException("Expected MCP tool text content.");

    public static bool IsToolError(JsonDocument response) =>
        response.RootElement.GetProperty("result").TryGetProperty("isError", out var isError) && isError.GetBoolean();

    public static string ReadResourceText(JsonDocument response) =>
        response.RootElement.GetProperty("result").GetProperty("contents")[0].GetProperty("text").GetString()
        ?? throw new InvalidOperationException("Expected MCP resource text content.");

    public static Process StartHostProcess(string connectionString, Uri? baseAddress, bool isolateUserSecrets = false)
    {
        var projectPath = GetProjectPath("baseball-history-mcp", "baseball-history-mcp.csproj");
        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{projectPath}\" --no-build --no-launch-profile")
        {
            WorkingDirectory = GetSolutionDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.Environment["ConnectionStrings__Lahman"] = connectionString;
        startInfo.Environment["ASPNETCORE_URLS"] = (baseAddress ?? new Uri($"http://127.0.0.1:{GetFreePort()}")).ToString();
        if (isolateUserSecrets)
        {
            startInfo.Environment["HOME"] = GetSolutionDirectory();
            startInfo.Environment["DOTNET_CLI_HOME"] = GetSolutionDirectory();
        }

        startInfo.Environment["Logging__LogLevel__Default"] = "Warning";
        startInfo.Environment["Logging__LogLevel__Microsoft"] = "Warning";
        startInfo.Environment["Logging__LogLevel__Microsoft.Hosting.Lifetime"] = "Warning";
        startInfo.Environment["Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command"] = "Warning";

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Unable to start baseball-history-mcp.");
    }

    private async Task NotifyAsync(string method)
    {
        var payload = JsonSerializer.Serialize(new { jsonrpc = "2.0", method });
        using var request = CreateJsonRpcRequest(payload, _sessionId);
        using var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> PostJsonRpcAsync(string method, object? parameters, string? sessionId)
    {
        var payload = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = Interlocked.Increment(ref _nextId),
            method,
            @params = parameters
        });

        using var request = CreateJsonRpcRequest(payload, sessionId);
        return await Client.SendAsync(request);
    }

    private static HttpRequestMessage CreateJsonRpcRequest(string payload, string? sessionId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Accept", "application/json, text/event-stream");
        if (!string.IsNullOrEmpty(sessionId))
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
                throw new InvalidOperationException($"MCP host exited during startup.{Environment.NewLine}{stderr}");
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

        throw new TimeoutException("MCP host did not become healthy within 30 seconds.", lastError);
    }

    private HttpClient Client => _client ?? throw new InvalidOperationException("MCP host process has not started.");

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static string GetSolutionDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static string GetProjectPath(string projectDirectory, string projectFileName) =>
        Path.Combine(GetSolutionDirectory(), projectDirectory, projectFileName);
}
