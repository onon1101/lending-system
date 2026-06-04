using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth.GetUserById;

public sealed record GetUserByIdResult(
    [property: JsonIgnore] long UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);
