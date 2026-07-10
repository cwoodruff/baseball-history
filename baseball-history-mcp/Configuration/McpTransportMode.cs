namespace baseball_history_mcp.Configuration;

public enum McpTransportMode
{
    Stdio,
    Http
}

public sealed record McpTransportInfo(McpTransportMode Mode)
{
    public string Name => Mode == McpTransportMode.Http ? "http" : "stdio";

    public bool HttpEnabled => Mode == McpTransportMode.Http;
}
