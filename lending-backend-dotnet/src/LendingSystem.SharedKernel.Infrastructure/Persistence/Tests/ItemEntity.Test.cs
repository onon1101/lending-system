using LendingSystem.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class ItemEntity_Test : InitializeData
{
    public override string TableName => "items";
    public override int Order => 2;
    public override async Task InsertBulkAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        const int itemCount = 10;
        var userNames = Enumerable
            .Range(1, itemCount)
            .Select(id => $"testuser{id:00}")
            .ToArray();
        var users = await db.Users
            .Where(x => userNames.Contains(x.Name))
            .OrderBy(x => x.Name)
            .Select(x => new { x.UserId, x.Name })
            .ToArrayAsync(cancellationToken);

        var items = users
            .Select((user, index) => GenerateScript(index + 1, user.UserId))
            .ToArray();

        var itemName = items
            .Select(x => x.ObjectName)
            .ToArray();

        await db.Items
            .Where(x => itemName.Contains(x.ObjectName))
            .ExecuteDeleteAsync(cancellationToken);

        await db.Items.AddRangeAsync(items, cancellationToken);
    }

    private static ItemEntity GenerateScript(
        int id,
        long ownerId) => new()
    {
        ItemId = 1000 + id,
        OwnerId = ownerId,
        ObjectName = $"Item test {id:00}",
        CurrentStatus = "Available",
        ImageUrl = string.Empty,
        Description = string.Empty,
        Maker = $"test-marker-{id:00}",
        Material = $"test-material-{id:00}",
    };
}
