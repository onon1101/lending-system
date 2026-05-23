using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Media;

public sealed record UploadPrivateMediaResult(
    [property: JsonIgnore] long MediaId,
    [property: JsonIgnore] long? OrderId,
    [property: JsonIgnore] long ObjectId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
