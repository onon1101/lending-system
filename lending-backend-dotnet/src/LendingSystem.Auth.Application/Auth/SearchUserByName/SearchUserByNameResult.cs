using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth;

public sealed record SearchUserByNameResult(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);
