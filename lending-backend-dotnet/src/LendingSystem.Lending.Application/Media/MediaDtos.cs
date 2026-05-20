using System.Text.Json.Serialization;
using LendingSystem.Lending.Domain.Media;

namespace LendingSystem.Lending.Application.Media;

public sealed record MediaResponse(
    [property: JsonPropertyName("media_id")] int MediaId,
    [property: JsonPropertyName("order_id")] int? OrderId,
    [property: JsonPropertyName("object_id")] int ObjectId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("link")] string Link,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt)
{
    public static MediaResponse From(MediaAsset asset) => new(
        asset.MediaId,
        asset.OrderId,
        asset.ObjectId,
        asset.Type,
        asset.Description,
        asset.Url,
        asset.Link,
        asset.CreatedAt);
}
