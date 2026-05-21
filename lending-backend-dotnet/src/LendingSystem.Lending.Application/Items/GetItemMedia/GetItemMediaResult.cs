using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemMediaResult(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string? Creator,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("link")] string OriginalLink,
    [property: JsonPropertyName("url")] string Media,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
