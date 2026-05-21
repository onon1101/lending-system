using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemByIdResult(
    [property: JsonPropertyName("item_id")] int ItemId,
    [property: JsonPropertyName("owner_id")] int OwnerId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl);
