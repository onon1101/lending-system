using LendingSystem.Auth.Domain.Users;

namespace LendingSystem.Auth.Application.Abstractions;

public interface IUserRepository
{
    Task<UserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserEntity?> FindByProviderAsync(string authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken);
    Task<UserEntity> CreateExternalAsync(string name, string email, string authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<UserEntity?> LinkProviderAsync(int userId, string authProvider, string providerUserId, CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken);
}
