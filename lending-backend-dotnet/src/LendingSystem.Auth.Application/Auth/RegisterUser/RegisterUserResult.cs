using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth;

public sealed record RegisterUserResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);
