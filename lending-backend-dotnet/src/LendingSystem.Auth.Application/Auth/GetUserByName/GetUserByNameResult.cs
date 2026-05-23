using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth.GetUserByName;

public sealed record GetUserByNameResult(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email);