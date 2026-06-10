using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace baseball_history_mcp;

internal static class BaseballMcpToolErrorHandling
{
    public static McpRequestHandler<CallToolRequestParams, CallToolResult> NormalizeToolFailures(
        McpRequestHandler<CallToolRequestParams, CallToolResult> next) =>
        async (request, cancellationToken) =>
        {
            try
            {
                return await next(request, cancellationToken);
            }
            catch (BaseballMcpUsageException ex) when (!cancellationToken.IsCancellationRequested)
            {
                return CreateToolFailure(ex.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch when (!cancellationToken.IsCancellationRequested)
            {
                return CreateToolFailure("The baseball-history MCP server could not complete that request. Retry with supported parameters or a narrower query.");
            }
        };

    public static CallToolResult CreateToolFailure(string message) =>
        new()
        {
            IsError = true,
            Content =
            [
                new TextContentBlock
                {
                    Text = message
                }
            ]
        };
}
