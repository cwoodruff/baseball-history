using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

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
        Assert.False(infoPayload.RootElement.GetProperty("httpTransportEnabled").GetBoolean());
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
        using var process = McpHostFixture.StartHostProcess("Host=<server>;Database=<db>;", isolateUserSecrets: true);

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
    private readonly ConcurrentQueue<string> _stderrLines = new();
    private readonly Channel<string> _stdoutLines = Channel.CreateUnbounded<string>();
    private Process? _process;
    private Task? _stdoutPump;
    private Task? _stderrPump;
    private bool _initialized;
    private int _nextId;

    public async Task InitializeAsync()
    {
        _process = StartHostProcess(TestDatabaseFactory.GetConnectionString());
        _stdoutPump = PumpAsync(_process.StandardOutput, line => _stdoutLines.Writer.WriteAsync(line).AsTask());
        _stderrPump = PumpAsync(_process.StandardError, line =>
        {
            _stderrLines.Enqueue(line);
            while (_stderrLines.Count > 200 && _stderrLines.TryDequeue(out _))
            {
            }

            return Task.CompletedTask;
        });

        using var initializeResponse = await RequestAsync(
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
            });

        Assert.Equal("baseball-history-mcp", initializeResponse.RootElement.GetProperty("result").GetProperty("serverInfo").GetProperty("name").GetString());
        await NotifyAsync("notifications/initialized");
        _initialized = true;
    }

    public async Task DisposeAsync()
    {
        _stdoutLines.Writer.TryComplete();

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
        }

        if (_stdoutPump is not null)
        {
            await _stdoutPump;
        }

        if (_stderrPump is not null)
        {
            await _stderrPump;
        }

        _process?.Dispose();
        _process = null;
    }

    public async Task<JsonDocument> RequestAsync(string method, object? parameters)
    {
        if (_process is null)
        {
            throw new InvalidOperationException("MCP host process has not started.");
        }

        if (!_initialized && method != "initialize")
        {
            throw new InvalidOperationException("MCP host must be initialized before sending requests.");
        }

        var request = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            id = Interlocked.Increment(ref _nextId),
            method,
            @params = parameters
        });

        await _process.StandardInput.WriteLineAsync(request);
        await _process.StandardInput.FlushAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            var line = await _stdoutLines.Reader.ReadAsync(timeout.Token);
            return JsonDocument.Parse(line);
        }
        catch (OperationCanceledException ex)
        {
            throw new TimeoutException($"Timed out waiting for MCP response to '{method}'.{Environment.NewLine}{GetDiagnostics()}", ex);
        }
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

    public static Process StartHostProcess(string connectionString, bool isolateUserSecrets = false)
    {
        var projectPath = GetProjectPath("baseball-history-mcp", "baseball-history-mcp.csproj");
        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{projectPath}\" --no-build --no-launch-profile")
        {
            WorkingDirectory = GetSolutionDirectory(),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.Environment["ConnectionStrings__Lahman"] = connectionString;
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
        if (_process is null)
        {
            throw new InvalidOperationException("MCP host process has not started.");
        }

        var notification = JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method
        });

        await _process.StandardInput.WriteLineAsync(notification);
        await _process.StandardInput.FlushAsync();
    }

    private string GetDiagnostics()
    {
        var stderr = string.Join(Environment.NewLine, _stderrLines.Reverse().Take(25).Reverse());
        return string.IsNullOrWhiteSpace(stderr)
            ? "No stderr output captured."
            : $"Recent stderr:{Environment.NewLine}{stderr}";
    }

    private static async Task PumpAsync(StreamReader reader, Func<string, Task> onLine)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            await onLine(line);
        }
    }

    private static string GetSolutionDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static string GetProjectPath(string projectDirectory, string projectFileName) =>
        Path.Combine(GetSolutionDirectory(), projectDirectory, projectFileName);
}
