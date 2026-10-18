using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using ModelContextProtocol;
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
            catch (McpProtocolException ex) when (ex.ErrorCode == McpErrorCode.InvalidParams && !cancellationToken.IsCancellationRequested)
            {
                // Input validation messages say what to fix, so pass them through
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

    private static readonly ConcurrentDictionary<string, IReadOnlyList<string>> SupportedArgumentsByTool = new(StringComparer.Ordinal);

    /// <summary>
    /// Rejects arguments the tool's input schema doesn't declare. Without this
    /// the SDK ignores them and the tool silently runs on its defaults, so a
    /// misspelled or invented parameter looks like a successful answer.
    /// </summary>
    public static McpRequestHandler<CallToolRequestParams, CallToolResult> RejectUnknownArguments(
        McpRequestHandler<CallToolRequestParams, CallToolResult> next) =>
        async (request, cancellationToken) =>
        {
            var arguments = request.Params?.Arguments;
            if (arguments is { Count: > 0 } && TryGetTool(request, out var tool))
            {
                var supported = SupportedArgumentsByTool.GetOrAdd(tool.ProtocolTool.Name, _ => ReadSchemaProperties(tool.ProtocolTool.InputSchema));
                var unknown = arguments.Keys.Where(name => !supported.Contains(name, StringComparer.Ordinal)).ToList();
                if (unknown.Count > 0)
                {
                    var supportedText = supported.Count == 0 ? "none" : string.Join(", ", supported);
                    return CreateToolFailure(
                        $"Unknown argument{(unknown.Count == 1 ? "" : "s")} for {tool.ProtocolTool.Name}: {string.Join(", ", unknown)}. Supported arguments: {supportedText}.");
                }
            }

            return await next(request, cancellationToken);
        };

    private static bool TryGetTool(RequestContext<CallToolRequestParams> request, [NotNullWhen(true)] out McpServerTool? tool)
    {
        tool = request.MatchedPrimitive as McpServerTool;
        if (tool is not null)
        {
            return true;
        }

        // Unknown tool names fall through to the SDK's own error
        var name = request.Params?.Name;
        return name is not null &&
            request.Server.ServerOptions.ToolCollection?.TryGetPrimitive(name, out tool) == true;
    }

    private static IReadOnlyList<string> ReadSchemaProperties(JsonElement inputSchema) =>
        inputSchema.ValueKind == JsonValueKind.Object &&
        inputSchema.TryGetProperty("properties", out var properties) &&
        properties.ValueKind == JsonValueKind.Object
            ? properties.EnumerateObject().Select(p => p.Name).ToList()
            : [];

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
