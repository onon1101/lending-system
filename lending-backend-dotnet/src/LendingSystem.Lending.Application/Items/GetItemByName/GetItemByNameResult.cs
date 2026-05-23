using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemByNameResult(
    [property: JsonIgnore] int ItemId,
    [property: JsonIgnore] int OwnerId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl);
