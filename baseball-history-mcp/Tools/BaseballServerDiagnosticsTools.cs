using System.ComponentModel;
using baseball_history_mcp.Metadata;
using ModelContextProtocol.Server;

namespace baseball_history_mcp.Tools;

[McpServerToolType]
public sealed class BaseballServerDiagnosticsTools(BaseballMcpMetadataService metadataService)
{
    [McpServerTool(Name = "get_server_diagnostics", ReadOnly = true, Title = "Get Server Diagnostics")]
    [Description("Return safe runtime posture, configured result caps, and query timeout settings without exposing secrets.")]
    public Task<ServerDiagnosticsDocument> GetServerDiagnosticsAsync(CancellationToken cancellationToken = default) =>
        metadataService.GetServerDiagnosticsAsync(cancellationToken);
}
