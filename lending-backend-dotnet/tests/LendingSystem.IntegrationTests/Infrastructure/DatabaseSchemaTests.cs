using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

public sealed class DatabaseSchemaTests(DatabaseSchemaFixture fixture) : IClassFixture<DatabaseSchemaFixture>
{
    private readonly DatabaseSchemaFixture _fixture = fixture;

    [Fact]
    public async Task Can_apply_database_schema()
    {
        await IntegrationTestDatabase.UpdateSchemaAsync();

        await using var db = IntegrationTestDatabase.CreateDbContext();

        // 更新 Database Schema
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();

        Assert.Empty(pendingMigrations);
        Assert.True(await db.Users.AnyAsync(x => x.Name == "testuser01"));
        Assert.True(await db.Items.AnyAsync(x => x.ObjectName == "Item test 01"));
        Assert.True(await db.BorrowerDetails.AnyAsync(x => x.BorrowerName == "Borrower test 01"));
        Assert.True(await db.Orders.AnyAsync());
        Assert.True(await db.ItemMedia.AnyAsync(x => x.Description == "Item media test 01"));
        Assert.True(await db.LendingMedia.AnyAsync(x => x.Description == "Lending media test 01"));
    }
}
