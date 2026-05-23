using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record UpdateItemCommand(
    [property: JsonPropertyName("object_name")] string? ObjectName,
    [property: JsonPropertyName("maker")] string? Maker,
    [property: JsonPropertyName("material")] string? Material,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("current_status")] string? CurrentStatus,
    [property: JsonPropertyName("image_url")] string? ImageUrl) : ICommand<UpdateItemResult>
{
    [JsonIgnore]
    public long ItemId { get; init; }

    [JsonIgnore]
    public string OwnerUsername { get; init; } = "";

    [JsonIgnore]
    public string OriginalObjectName { get; init; } = "";

    [JsonIgnore]
    public long CurrentUserId { get; init; }

    [JsonIgnore]
    public bool IsAdmin { get; init; }
}
