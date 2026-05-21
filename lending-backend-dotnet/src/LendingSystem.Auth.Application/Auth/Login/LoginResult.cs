using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth;

public sealed record LoginResult(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken);
