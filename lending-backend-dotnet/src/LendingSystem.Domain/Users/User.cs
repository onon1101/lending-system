namespace LendingSystem.Domain.Users;

public sealed record User(
    int Id,
    string Email,
    string PasswordHash,
    string Name,
    string Role,
    string AuthProvider,
    string? ProviderUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UserProfile(int UserId, string Name, string Email);
