using LendingSystem.SharedKernel.Infrastructure.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class ItemMediaEntity_Test : InitializeData
{
    public override string TableName => "item_media";
    public override int Order => 5;

    public override async Task InsertBulkAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        const int mediaCount = 10;
        var itemNames = Enumerable
            .Range(1, mediaCount)
            .Select(id => $"Item test {id:00}")
            .ToArray();
        var items = await db.Items
            .Where(x => itemNames.Contains(x.ObjectName))
            .OrderBy(x => x.ObjectName)
            .Select(x => new { x.ItemId, x.ObjectName })
            .ToArrayAsync(cancellationToken);
        var media = items
            .Select((item, index) => GenerateScript(index + 1, item.ItemId))
            .ToArray();

        await db.ItemMedia.AddRangeAsync(media, cancellationToken);
    }

    private static ItemMediaEntity GenerateScript(
        int id,
        long itemId) => new()
    {
        MediaId = 4000 + id,
        ItemId = itemId,
        Type = id % 2 == 0 ? "video" : "image",
        Url = $"https://example.com/items/{id:00}/media",
        Link = $"https://example.com/items/{id:00}",
        Description = $"Item media test {id:00}"
    };
}
