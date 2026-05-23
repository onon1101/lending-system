using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record CreateItemResult(
    [property: JsonIgnore] long ItemId,
    [property: JsonIgnore] long OwnerId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl);
