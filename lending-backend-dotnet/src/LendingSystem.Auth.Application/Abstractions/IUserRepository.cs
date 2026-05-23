using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.Auth.Domain.Users;

namespace LendingSystem.Auth.Application.Abstractions;

public interface IUserCommandRepository
{
    Task<UserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserEntity?> FindByProviderAsync(AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken);
    Task<UserEntity> CreateExternalAsync(string name, string email, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<UserEntity?> LinkProviderAsync(long userId, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long userId, CancellationToken cancellationToken);
}

public interface IUserQueryRepository
{
    Task<UserProfile?> GetByIdAsync(long userId, CancellationToken cancellationToken);
    Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken);
}
