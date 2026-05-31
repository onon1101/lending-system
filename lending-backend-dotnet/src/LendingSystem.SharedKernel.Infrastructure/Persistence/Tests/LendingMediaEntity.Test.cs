using LendingSystem.IntegrationTest.Framework.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class LendingMediaEntity_Test : InitializeData
{
    public override string TableName => "lending_media";
    public override int Order => 6;

    public override async Task InsertBulkAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        var orders = await db.Orders
            .OrderBy(x => x.OrderId)
            .Select(x => new { x.OrderId })
            .Take(10)
            .ToArrayAsync(cancellationToken);
        var media = orders
            .Select((order, index) => GenerateScript(index + 1, order.OrderId))
            .ToArray();

        await db.LendingMedia.AddRangeAsync(media, cancellationToken);
    }

    private static LendingMediaEntity GenerateScript(
        int id,
        long orderId) => new()
    {
        MediaId = 5000 + id,
        OrderId = orderId,
        Type = id % 2 == 0 ? "video" : "image",
        Url = $"https://example.com/lending/{id:00}/media",
        Link = $"https://example.com/lending/{id:00}",
        Description = $"Lending media test {id:00}"
    };
}
