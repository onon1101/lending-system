using System.Text.Json.Serialization;

namespace LendingSystem.Application.Auth;

public sealed record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

public sealed record CreateUserRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password_hash")] string PasswordHash);

public sealed record UserResponse(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);

public sealed record AuthResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken);
