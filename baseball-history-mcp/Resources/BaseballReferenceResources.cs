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

    [McpServerResource(UriTemplate = "baseball-history://server/workflow-guide", Name = "workflow-guide", Title = "Workflow Guide", MimeType = "application/json")]
    [Description("Workflow routing for common baseball questions so clients choose the correct shipped tool or guide resource.")]
    public async Task<string> GetWorkflowGuideAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetWorkflowGuideAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://server/stats-catalog", Name = "stats-catalog", Title = "Stats Catalog", MimeType = "application/json")]
    [Description("Supported batting and pitching stat categories plus the supported Lahman year span.")]
    public async Task<string> GetStatsCatalogAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetStatsCatalogAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://server/diagnostics", Name = "server-diagnostics", Title = "Server Diagnostics", MimeType = "application/json")]
    [Description("Safe runtime posture, configured limits, and connectivity status without exposing secrets.")]
    public async Task<string> GetServerDiagnosticsAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetServerDiagnosticsAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://hall-of-fame/guide", Name = "hall-of-fame-guide", Title = "Hall of Fame Guide", MimeType = "application/json")]
    [Description("Hall of Fame tool limits, year coverage, and voting-history caveats.")]
    public async Task<string> GetHallOfFameGuideAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetHallOfFameGuideAsync(cancellationToken));

    [McpServerResource(UriTemplate = "baseball-history://salary/guide", Name = "salary-guide", Title = "Salary Guide", MimeType = "application/json")]
    [Description("Salary-history and salary-leader tool limits, year coverage, and payload caveats.")]
    public async Task<string> GetSalaryGuideAsync(CancellationToken cancellationToken = default) =>
        BaseballMcpMetadataService.Serialize(await metadataService.GetSalaryGuideAsync(cancellationToken));
}
