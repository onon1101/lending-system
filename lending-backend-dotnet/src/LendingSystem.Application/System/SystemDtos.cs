using System.Text.Json.Serialization;

namespace LendingSystem.Application.System;

public sealed record DependencyStatus(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("error")] string? Error = null);

public sealed record ServiceHealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("service")] string Service,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp);

public sealed record SystemStatusResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("service")] string Service,
    [property: JsonPropertyName("database")] DependencyStatus Database,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp);
