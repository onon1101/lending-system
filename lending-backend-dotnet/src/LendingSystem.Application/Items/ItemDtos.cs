using System.Text.Json.Serialization;

namespace LendingSystem.Application.Items;

public sealed record CreateItemRequest(
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string? Maker,
    [property: JsonPropertyName("material")] string? Material,
    [property: JsonPropertyName("description")] string Description);

public sealed record UpdateItemRequest(
    [property: JsonPropertyName("object_name")] string? ObjectName,
    [property: JsonPropertyName("maker")] string? Maker,
    [property: JsonPropertyName("material")] string? Material,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("current_status")] string? CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl);

public sealed record ItemResponse(
    [property: JsonPropertyName("item_id")] int ItemId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl);

public sealed record ItemSummaryResponse(
    [property: JsonPropertyName("item_id")] int ItemId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("owner_name")] string? OwnerName,
    [property: JsonPropertyName("owner_email")] string? OwnerEmail,
    [property: JsonPropertyName("image_url")] string? ImageUrl);

public sealed record ItemMediaResponse(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string? Creator,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("link")] string OriginalLink,
    [property: JsonPropertyName("url")] string Media,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
