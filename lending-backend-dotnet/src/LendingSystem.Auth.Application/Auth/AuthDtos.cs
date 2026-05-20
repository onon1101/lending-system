using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth;

public sealed record LoginRequest(
    [Required]
    [property: JsonPropertyName("email")] string Email,
    [Required]
    [property: JsonPropertyName("password")] string Password);

public sealed record CreateUserRequest(
    [Required]
    [property: JsonPropertyName("name")] string Name,
    [Required]
    [property: JsonPropertyName("email")] string Email,
    [Required]
    [property: JsonPropertyName("password_hash")] string PasswordHash);

public sealed record GoogleLoginRequest(
    [Required]
    [property: JsonPropertyName("id_token")] string IdToken);

// ----------------- Response ----------------- 

public sealed record UserResponse(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("email")] string Email);

public sealed record AuthResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken);

public sealed record DeleteResponse(
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("message")] string Message
);