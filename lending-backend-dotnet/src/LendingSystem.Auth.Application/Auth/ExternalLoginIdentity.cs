using LendingSystem.Auth.Domain.Users;

namespace LendingSystem.Auth.Application.Auth;

public sealed record ExternalLoginIdentity(
    AuthProvider Provider,
    string ProviderUserId,
    string Email,
    string DisplayName);
