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
    public int ItemId { get; init; }

    [JsonIgnore]
    public int CurrentUserId { get; init; }

    [JsonIgnore]
    public bool IsAdmin { get; init; }
}
