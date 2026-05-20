using System.Text;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;
using LendingDbContext = LendingSystem.SharedKernel.Infrastructure.Persistence.LendingDbContext;
using PersistenceUserEntity = LendingSystem.SharedKernel.Infrastructure.Persistence.UserEntity;

namespace LendingSystem.Auth.Infrastructure.Persistence;

public sealed class UserRepository(LendingDbContext db) : IUserRepository
{
    public async Task<UserEntity?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .AsNoTracking()
            .Where(x => x.Email == email && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapUser(entity);
    }

    public async Task<UserEntity?> FindByProviderAsync(string authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .AsNoTracking()
            .Where(x => x.AuthProvider == authProvider && x.ProviderUserId == providerUserId && !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapUser(entity);
    }

    public async Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        var entity = new PersistenceUserEntity
        {
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            DisplayName = name,
            Email = email,
            PasswordHash = passwordHash,
            AuthProvider = "local"
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserProfile(entity.UserId, entity.DisplayName, entity.Email ?? "");
    }

    public async Task<UserEntity> CreateExternalAsync(string name, string email, string authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = new PersistenceUserEntity
        {
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            DisplayName = name,
            Email = email,
            AuthProvider = authProvider,
            ProviderUserId = providerUserId
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity);
    }

    public async Task<UserEntity?> LinkProviderAsync(int userId, string authProvider, string providerUserId, CancellationToken cancellationToken)
    {
        var entity = await db.Users
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.AuthProvider = authProvider;
        entity.ProviderUserId = providerUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return MapUser(entity);
    }

    public async Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => new UserProfile(x.UserId, x.DisplayName, x.Email ?? ""))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.DisplayName, $"%{username}%") && !x.IsDeleted)
            .Select(x => new UserProfile(x.UserId, x.DisplayName, x.Email ?? ""))
            .FirstOrDefaultAsync(cancellationToken);
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
            if (!await db.Users.AnyAsync(x => x.Name == name, cancellationToken))
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

    private static UserEntity MapUser(PersistenceUserEntity entity) => UserEntity.Create(
        entity.UserId,
        entity.Email ?? "",
        entity.PasswordHash ?? "",
        entity.DisplayName,
        entity.Role ?? "",
        entity.AuthProvider,
        entity.ProviderUserId,
        entity.CreatedAt ?? default,
        entity.UpdatedAt ?? default);
}
