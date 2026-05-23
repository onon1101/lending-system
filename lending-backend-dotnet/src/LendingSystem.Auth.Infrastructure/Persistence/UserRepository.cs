using System.Text;
using System.ComponentModel.DataAnnotations;
using Dapper;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.Auth.Domain.Users;
using LendingSystem.SharedKernel.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using LendingDbContext = LendingSystem.SharedKernel.Infrastructure.Persistence.LendingDbContext;
using PersistenceUserEntity = LendingSystem.SharedKernel.Infrastructure.Persistence.UserEntity;

namespace LendingSystem.Auth.Infrastructure.Persistence;

public sealed class UserRepository(
    LendingDbContext db,
    EmailAddressAttribute emailAddressAttribute,
    IQueryConnectionFactory queryConnectionFactory) : IUserCommandRepository, IUserQueryRepository
{
    public async Task<UserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                user_id as UserId,
                email as Email,
                password_hash as PasswordHash,
                display_name as DisplayName,
                role as Role,
                auth_provider as AuthProvider,
                provider_user_id as ProviderUserId,
                created_at as CreatedAt,
                updated_at as UpdatedAt
            from users
            where email = @Email
              and is_deleted = false;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));

        return row is null ? null : MapUser(row);
    }

    public async Task<UserEntity?> FindByProviderAsync(AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                user_id as UserId,
                email as Email,
                password_hash as PasswordHash,
                display_name as DisplayName,
                role as Role,
                auth_provider as AuthProvider,
                provider_user_id as ProviderUserId,
                created_at as CreatedAt,
                updated_at as UpdatedAt
            from users
            where upper(auth_provider) = @Provider
              and provider_user_id = @ProviderUserId
              and is_deleted = false;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Provider = authProvider.Value, ProviderUserId = providerUserId }, cancellationToken: cancellationToken));

        return row is null ? null : MapUser(row);
    }

    public async Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        var entity = new PersistenceUserEntity
        {
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            DisplayName = name,
            Email = email,
            PasswordHash = passwordHash,
            AuthProvider = AuthProvider.Local.Value
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserProfile(entity.UserId, entity.DisplayName, entity.Email ?? "");
    }

    public async Task<UserEntity> CreateExternalAsync(string name, string email, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = new PersistenceUserEntity
        {
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            DisplayName = name,
            Email = email,
            AuthProvider = authProvider.Value,
            ProviderUserId = providerUserId
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity);
    }

    public async Task<UserEntity?> LinkProviderAsync(int userId, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.AuthProvider = authProvider.Value;
        entity.ProviderUserId = providerUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity);
    }

    public async Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                user_id as UserId,
                display_name as Name,
                coalesce(email, '') as Email
            from users
            where user_id = @UserId
              and is_deleted = false;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserProfile>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                user_id as UserId,
                display_name as Name,
                coalesce(email, '') as Email
            from users
            where display_name ilike @Pattern
              and is_deleted = false
            order by user_id
            limit 1;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserProfile>(
            new CommandDefinition(sql, new { Pattern = $"%{username}%" }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (entity is null)
            return false;

        entity.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<string> CreateUniqueNameAsync(string email, CancellationToken cancellationToken)
    {
        var candidate = CreateNameFromEmail(email);
        var suffix = string.Empty;

        for (var attempt = 0; ; attempt++)
        {
            var name = $"{candidate}{suffix}";
            const string sql = """
                select exists (
                    select 1
                    from users
                    where name = @Name
                );
                """;

            using var connection = queryConnectionFactory.CreateConnection();
            var exists = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { Name = name }, cancellationToken: cancellationToken));
            if (!exists)
            {
                return name;
            }

            suffix = ToLetters(attempt + 1);
            candidate = candidate[..Math.Min(candidate.Length, 100 - suffix.Length)];
        }
    }

    private static string CreateNameFromEmail(string email)
    {
        var localPart = email.Split('@', 2)[0];
        var builder = new StringBuilder(localPart.Length);

        foreach (var character in localPart)
        {
            if (character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.Length == 0
            ? "user"
            : builder.ToString()[..Math.Min(builder.Length, 100)];
    }

    private static string ToLetters(int value)
    {
        var builder = new StringBuilder();

        while (value > 0)
        {
            value--;
            builder.Insert(0, (char)('a' + value % 26));
            value /= 26;
        }

        return builder.ToString();
    }

    private UserEntity MapUser(PersistenceUserEntity entity) => UserEntity.Create(
        emailAddressAttribute,
        entity.UserId,
        entity.Email ?? "",
        entity.PasswordHash ?? "",
        entity.DisplayName,
        entity.Role ?? "",
        entity.AuthProvider,
        entity.ProviderUserId,
        entity.CreatedAt ?? default,
        entity.UpdatedAt ?? default);

    private UserEntity MapUser(UserRow row) => UserEntity.Create(
        emailAddressAttribute,
        row.UserId,
        row.Email ?? "",
        row.PasswordHash ?? "",
        row.DisplayName,
        row.Role ?? "",
        row.AuthProvider,
        row.ProviderUserId,
        row.CreatedAt ?? default,
        row.UpdatedAt ?? default);

    private sealed record UserRow(
        int UserId,
        string? Email,
        string? PasswordHash,
        string DisplayName,
        string? Role,
        string AuthProvider,
        string? ProviderUserId,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt);
}
