using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record CreateItemCommand(
    [Required]
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("maker")] string? Maker,
    [property: JsonPropertyName("material")] string? Material,
    [Required]
    [property: JsonPropertyName("description")] string Description) : ICommand<CreateItemResult>
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonIgnore]
    public FileFormat? FileFormat { get; init; }
}
