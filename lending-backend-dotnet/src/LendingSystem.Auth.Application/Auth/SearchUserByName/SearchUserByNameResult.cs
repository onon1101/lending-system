using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth.SearchUserByName;

public sealed record SearchUserByNameResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);
