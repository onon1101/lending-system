using System.Security.Cryptography.X509Certificates;
using System.Text;
using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class UserRepository(LendingDbContext db) : IUserRepository
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.Email == email && !x.IsDeleted)
            .Select(x => new User(
                x.UserId,
                x.Email ?? "",
                x.PasswordHash ?? "",
                x.DisplayName,
                x.Role ?? "",
                x.CreatedAt ?? default,
                x.UpdatedAt ?? default))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        var entity = new UserEntity
        {
            Name = await CreateUniqueNameAsync(email, cancellationToken),
            DisplayName = name,
            Email = email,
            PasswordHash = passwordHash
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserProfile(entity.UserId, entity.DisplayName, entity.Email ?? "");
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
            if (character is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
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
}
