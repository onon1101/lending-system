using System.Text;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Dapper;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.Auth.Domain.Users;
using LendingSystem.SharedKernel.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using ApplicationGeneratedKey = LendingSystem.SharedKernel.Infrastructure.Persistence.ApplicationGeneratedKey;
using LendingDbContext = LendingSystem.SharedKernel.Infrastructure.Persistence.LendingDbContext;
using PersistenceUserAuthIdentityEntity = LendingSystem.SharedKernel.Infrastructure.Persistence.UserAuthIdentityEntity;
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
                u.user_id as UserId,
                coalesce(a.metadata_json ->> 'email', a.identifier) as Email,
                a.metadata_json ->> 'passwordHash' as PasswordHash,
                u.name as Name,
                u.role as Role,
                a.type as AuthProvider,
                case when a.type = 'LOCAL' then null else a.identifier end as ProviderUserId,
                u.created_at as CreatedAt,
                u.updated_at as UpdatedAt
            from users u
            join user_auth_identities a on a.user_id = u.user_id
            where a.type = 'LOCAL'
              and a.identifier = @Email
              and u.status = 'ACTIVE';
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
                u.user_id as UserId,
                coalesce(a.metadata_json ->> 'email', '') as Email,
                a.metadata_json ->> 'passwordHash' as PasswordHash,
                u.name as Name,
                u.role as Role,
                a.type as AuthProvider,
                a.identifier as ProviderUserId,
                u.created_at as CreatedAt,
                u.updated_at as UpdatedAt
            from users u
            join user_auth_identities a on a.user_id = u.user_id
            where a.type = @Provider
              and a.identifier = @ProviderUserId
              and u.status = 'ACTIVE';
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
            UserId = ApplicationGeneratedKey.NewId(),
            Name = name,
            Status = "ACTIVE"
        };
        entity.AuthIdentities.Add(new PersistenceUserAuthIdentityEntity
        {
            Id = ApplicationGeneratedKey.NewId(),
            UserId = entity.UserId,
            Type = AuthProvider.Local.Value,
            Identifier = email,
            MetadataJson = CreateLocalMetadata(email, passwordHash)
        });

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserProfile(entity.UserId, entity.Name, email);
    }

    public async Task<UserEntity> CreateExternalAsync(string name, string email, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = new PersistenceUserEntity
        {
            UserId = ApplicationGeneratedKey.NewId(),
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            Status = "ACTIVE"
        };
        entity.AuthIdentities.Add(new PersistenceUserAuthIdentityEntity
        {
            Id = ApplicationGeneratedKey.NewId(),
            UserId = entity.UserId,
            Type = authProvider.Value,
            Identifier = providerUserId,
            MetadataJson = CreateExternalMetadata(email)
        });

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity, entity.AuthIdentities.Single());
    }

    public async Task<UserEntity?> LinkProviderAsync(long userId, AuthProvider authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .Include(x => x.AuthIdentities)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "ACTIVE", cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var identity = entity.AuthIdentities.FirstOrDefault(x =>
            x.Type == authProvider.Value && x.Identifier == providerUserId);
        if (identity is null)
        {
            identity = new PersistenceUserAuthIdentityEntity
            {
                Id = ApplicationGeneratedKey.NewId(),
                UserId = userId,
                Type = authProvider.Value,
                Identifier = providerUserId,
                MetadataJson = CreateExternalMetadata(entity.AuthIdentities.FirstOrDefault(x => x.Type == AuthProvider.Local.Value)?.Identifier ?? "")
            };
            entity.AuthIdentities.Add(identity);
        }
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity, identity);
    }

    public async Task<bool> GetExistsAsync(string name, string email, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT COUNT(1)
                           FROM users u
                           WHERE u.status = 'ACTIVE'
                             AND (
                                 u.name = @Name
                                 OR EXISTS (
                                     SELECT 1
                                     FROM user_auth_identities a
                                     WHERE a.user_id = u.user_id
                                       AND a.type = 'LOCAL'
                                       AND a.identifier = @Email
                                 )
                             );
                           """;
        
        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { Name = name, Email = email }, cancellationToken: cancellationToken)) > 0;
    }

    public async Task<UserProfile?> GetByIdAsync(long userId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                user_id as UserId,
                name as Name,
                coalesce(auth.email, '') as Email
            from users u
            left join lateral (
                select coalesce(a.metadata_json ->> 'email', a.identifier) as email
                from user_auth_identities a
                where a.user_id = u.user_id
                order by case when a.type = 'LOCAL' then 0 else 1 end, a.id
                limit 1
            ) auth on true
            where u.user_id = @UserId
              and u.status = 'ACTIVE';
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
                name as Name,
                coalesce(auth.email, '') as Email
            from users u
            left join lateral (
                select coalesce(a.metadata_json ->> 'email', a.identifier) as email
                from user_auth_identities a
                where a.user_id = u.user_id
                order by case when a.type = 'LOCAL' then 0 else 1 end, a.id
                limit 1
            ) auth on true
            where u.name ilike @Pattern
              and u.status = 'ACTIVE'
            order by u.user_id
            limit 1;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserProfile>(
            new CommandDefinition(sql, new { Pattern = $"%{username}%" }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(long userId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (entity is null)
            return false;

        entity.Status = "DELETED";
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

    private static string CreateLocalMetadata(string email, string passwordHash) =>
        JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["email"] = email,
            ["passwordHash"] = passwordHash
        });

    private static string CreateExternalMetadata(string email) =>
        JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["email"] = email
        });

    private static (string Email, string PasswordHash) ReadMetadata(string metadataJson)
    {
        using var document = JsonDocument.Parse(metadataJson);
        var root = document.RootElement;
        var email = root.TryGetProperty("email", out var emailElement) ? emailElement.GetString() ?? "" : "";
        var passwordHash = root.TryGetProperty("passwordHash", out var passwordHashElement)
            ? passwordHashElement.GetString() ?? ""
            : "";
        return (email, passwordHash);
    }

    private UserEntity MapUser(PersistenceUserEntity entity, PersistenceUserAuthIdentityEntity identity)
    {
        var (email, passwordHash) = ReadMetadata(identity.MetadataJson);
        return UserEntity.Create(
            emailAddressAttribute,
            entity.UserId,
            identity.Type == AuthProvider.Local.Value ? identity.Identifier : email,
            passwordHash,
            entity.Name,
            entity.Role ?? "",
            identity.Type,
            identity.Type == AuthProvider.Local.Value ? null : identity.Identifier,
            entity.CreatedAt ?? default,
            entity.UpdatedAt ?? default);
    }

    private UserEntity MapUser(UserRow row) => UserEntity.Create(
        emailAddressAttribute,
        row.UserId,
        row.Email ?? "",
        row.PasswordHash ?? "",
        row.Name,
        row.Role ?? "",
        row.AuthProvider,
        row.ProviderUserId,
        row.CreatedAt ?? default,
        row.UpdatedAt ?? default);

    private sealed record UserRow(
        long UserId,
        string? Email,
        string? PasswordHash,
        string Name,
        string? Role,
        string AuthProvider,
        string? ProviderUserId,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt);
}
