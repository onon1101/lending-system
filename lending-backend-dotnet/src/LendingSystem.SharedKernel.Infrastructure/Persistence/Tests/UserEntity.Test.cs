using LendingSystem.IntegrationTest.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class UserEntity_Test : InitializeData
{
    public override string TableName => "users";
    public override int Order => 1;

    public override async Task InsertBulkAsync(LendingDbContext db, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        const int userCount = 10;

        var users = Enumerable
            .Range(1, userCount)
            .Select(id => GenerateScript(id, now))
            .Append(GenerateAdminScript(9001, now))
            .ToArray();
        var userNames = users.Select(x => x.Name).ToArray();

        await db.Users
            .Where(x => userNames.Contains(x.Name))
            .ExecuteDeleteAsync(cancellationToken);

        await db.Users.AddRangeAsync(users, cancellationToken);
        
        await db.SaveChangesAsync(cancellationToken);
    }

    private static UserEntity GenerateScript(int id, DateTimeOffset time)
        => new()
        {
            UserId = id,
            Name = $"testuser{id:00}",
            Email = $"test{id:00}@test{id:00}.com.tw",
            PasswordHash = "123",
            AuthProvider = "LOCAL",
            ProviderUserId = null,
            IsDeleted = false,
            Role = "user",
            CreatedAt = time,
            UpdatedAt = time 
        };

    private static UserEntity GenerateAdminScript(int id, DateTimeOffset time)
        => new()
        {
            UserId = id,
            Name = $"testadmin{id:00}",
            Email = $"testadmin{id:00}@testadmin{id:00}.com.tw",
            PasswordHash = "123",
            AuthProvider = "LOCAL",
            ProviderUserId = null,
            IsDeleted = false,
            Role = "admin",
            CreatedAt = time,
            UpdatedAt = time
        };
}
