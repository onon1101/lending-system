using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemsByUserNameResult(
    [property: JsonIgnore] long ItemId,
    [property: JsonIgnore] long OwnerId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string Maker,
    [property: JsonPropertyName("material")] string Material,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("current_status")] string CurrentStatus,
    [property: JsonPropertyName("owner_username")] string? OwnerUsername,
    [property: JsonPropertyName("owner_name")] string? OwnerName,
    [property: JsonPropertyName("owner_email")] string? OwnerEmail,
    [property: JsonPropertyName("image_url")] string? ImageUrl);
