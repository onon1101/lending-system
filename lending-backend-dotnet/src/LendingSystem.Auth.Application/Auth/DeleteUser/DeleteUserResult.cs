using System.Text.Json.Serialization;

namespace LendingSystem.Auth.Application.Auth;

public sealed record DeleteUserResult(
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("message")] string Message);
