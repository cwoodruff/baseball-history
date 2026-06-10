namespace baseball_history_mcp.Metadata;

public sealed record TransportPolicyDocument(
    string CurrentTransport,
    bool HttpEnabled,
    string V1Recommendation,
    IReadOnlyList<string> DecisionDrivers,
    IReadOnlyList<string> SdkGuidance,
    IReadOnlyList<string> RevisitCriteria);
