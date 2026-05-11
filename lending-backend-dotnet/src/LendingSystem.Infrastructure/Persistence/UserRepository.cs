using System.Security.Cryptography.X509Certificates;
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
                x.Name,
                x.Role ?? "",
                x.CreatedAt ?? default,
                x.UpdatedAt ?? default))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        var entity = new UserEntity
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new UserProfile(entity.UserId, entity.Name, entity.Email ?? "");
    }

    public async Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => new UserProfile(x.UserId, x.Name, x.Email ?? ""))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => EF.Functions.Like(x.Name, $"%{username}%") && !x.IsDeleted)
            .Select(x => new UserProfile(x.UserId, x.Name, x.Email ?? ""))
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
}
