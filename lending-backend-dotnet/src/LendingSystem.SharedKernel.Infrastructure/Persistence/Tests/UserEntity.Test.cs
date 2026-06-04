using LendingSystem.SharedKernel.Infrastructure.Abstractions;
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

        foreach (var user in users)
        {
            var identity = user.AuthIdentities.Single();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "users" ("user_id", "name", "status", "role", "created_at", "updated_at")
                VALUES ({user.UserId}, {user.Name}, {user.Status}, {user.Role}, {user.CreatedAt}, {user.UpdatedAt})
                ON CONFLICT ("name") DO UPDATE
                SET "status" = EXCLUDED."status",
                    "role" = EXCLUDED."role",
                    "updated_at" = EXCLUDED."updated_at";

                INSERT INTO "user_auth_identities" ("id", "user_id", "type", "identifier", "metadata_json", "created_at", "updated_at")
                VALUES ({identity.Id}, {user.UserId}, {identity.Type}, {identity.Identifier}, {identity.MetadataJson}::jsonb, {identity.CreatedAt}, {identity.UpdatedAt})
                ON CONFLICT ("type", "identifier") DO UPDATE
                SET "metadata_json" = EXCLUDED."metadata_json",
                    "updated_at" = EXCLUDED."updated_at";
                """, cancellationToken);
        }
    }

    private static UserEntity GenerateScript(int id, DateTimeOffset time)
        => new()
        {
            UserId = id,
            Name = $"testuser{id:00}",
            Status = "ACTIVE",
            Role = "user",
            CreatedAt = time,
            UpdatedAt = time,
            AuthIdentities =
            [
                new UserAuthIdentityEntity
                {
                    Id = 10000 + id,
                    UserId = id,
                    Type = "LOCAL",
                    Identifier = $"test{id:00}@test{id:00}.com.tw",
                    MetadataJson = $$"""{"email":"test{{id.ToString("00")}}@test{{id.ToString("00")}}.com.tw","passwordHash":"123"}""",
                    CreatedAt = time,
                    UpdatedAt = time
                }
            ]
        };

    private static UserEntity GenerateAdminScript(int id, DateTimeOffset time)
        => new()
        {
            UserId = id,
            Name = $"testadmin{id:00}",
            Status = "ACTIVE",
            Role = "admin",
            CreatedAt = time,
            UpdatedAt = time,
            AuthIdentities =
            [
                new UserAuthIdentityEntity
                {
                    Id = 10000 + id,
                    UserId = id,
                    Type = "LOCAL",
                    Identifier = $"testadmin{id:00}@testadmin{id:00}.com.tw",
                    MetadataJson = $$"""{"email":"testadmin{{id.ToString("00")}}@testadmin{{id.ToString("00")}}.com.tw","passwordHash":"123"}""",
                    CreatedAt = time,
                    UpdatedAt = time
                }
            ]
        };
}
