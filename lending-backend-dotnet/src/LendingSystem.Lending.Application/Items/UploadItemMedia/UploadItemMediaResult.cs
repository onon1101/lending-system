using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record UploadItemMediaResult(
    [property: JsonPropertyName("media_id")] int MediaId,
    [property: JsonPropertyName("order_id")] int? OrderId,
    [property: JsonPropertyName("object_id")] int ObjectId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
