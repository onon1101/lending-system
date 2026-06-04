using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth.RegisterUser;

public sealed record RegisterUserResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);
