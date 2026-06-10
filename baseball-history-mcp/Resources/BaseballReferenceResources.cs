using System.ComponentModel;
using baseball_history_mcp.Metadata;
using ModelContextProtocol.Server;

namespace baseball_history_mcp.Resources;

[McpServerResourceType]
public sealed class BaseballReferenceResources(BaseballMcpMetadataService metadataService)
{
    [McpServerResource(UriTemplate = "baseball-history://server/info", Name = "server-info", Title = "Server Info", MimeType = "application/json")]
    [Description("Server metadata, startup requirements, resource discovery URIs, and configured limits.")]
    public async Task<string> GetServerInfoAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetServerInfoAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://server/stats-catalog", Name = "stats-catalog", Title = "Stats Catalog", MimeType = "application/json")]
    [Description("Supported batting and pitching stat categories plus the supported Lahman year span.")]
    public async Task<string> GetStatsCatalogAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetStatsCatalogAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://server/diagnostics", Name = "server-diagnostics", Title = "Server Diagnostics", MimeType = "application/json")]
    [Description("Safe runtime posture, configured limits, and connectivity status without exposing secrets.")]
    public async Task<string> GetServerDiagnosticsAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetServerDiagnosticsAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://server/transport-policy", Name = "transport-policy", Title = "Transport Policy", MimeType = "application/json")]
    [Description("V1 transport posture, including the HTTP go/no-go recommendation and the MCP C# SDK host-validation/CORS guidance behind it.")]
    public Task<string> GetTransportPolicyAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(BaseballMcpMetadataService.Serialize(metadataService.GetTransportPolicy()));

    [McpServerResource(UriTemplate = "baseball-history://guides/getting-started", Name = "getting-started-guide", Title = "Getting Started Guide", MimeType = "text/markdown")]
    [Description("How real MCP clients should discover the shipped surface before making baseball-history tool calls.")]
    public Task<string> GetGettingStartedGuideAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(
            """
            # Baseball History MCP — Getting Started

            1. Read `baseball-history://server/info` to discover tool names, resource links, startup requirements, and result limits.
            2. Read `baseball-history://server/stats-catalog` before calling leaderboard tools so you only send supported stat keys.
            3. Use the discovery tools first:
               - `search_players`
               - `list_franchises`
               - `get_franchise`
               - `get_team_season`
            4. Use the domain tools that match the shipped v1 surface:
               - `get_batting_leaders`, `get_pitching_leaders`
               - `list_hall_of_fame_inductees`, `get_hall_of_fame_voting_history`
               - `get_player_salary_history`, `get_team_payroll`, `get_salary_leaders`
               - `get_server_diagnostics`
            5. Treat every collection as bounded. Respect the configured caps reported by `server/info`, including Hall of Fame paging plus salary-history and team-payroll item limits.

            This server is read-only and stdio-only in v1.
            """);

    [McpServerResource(UriTemplate = "baseball-history://guides/workflows", Name = "workflow-guide", Title = "Workflow Guide", MimeType = "text/markdown")]
    [Description("Representative v1 workflows for player, franchise, team-season, leaderboard, Hall of Fame, and salary questions.")]
    public Task<string> GetWorkflowGuideAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(
            """
            # Baseball History MCP — Workflow Guide

            ## Player lookup workflow
            - `search_players`
            - `get_player`

            ## Franchise to team-season workflow
            - `list_franchises`
            - `get_franchise`
            - `get_team_season`

            ## Leaderboard workflow
            - `baseball-history://server/stats-catalog`
            - `get_batting_leaders` or `get_pitching_leaders`

            ## Hall of Fame workflow
            - `list_hall_of_fame_inductees`
            - `get_hall_of_fame_voting_history`

            ## Salary workflow
            - `get_salary_leaders`
            - `get_player_salary_history` or `get_team_payroll`

            ## Runtime posture workflow
            - `get_server_diagnostics`
            - `baseball-history://server/transport-policy`

            Do not assume HTTP transport, browser access, write tools, or broader REST parity in v1.
            """);
}
