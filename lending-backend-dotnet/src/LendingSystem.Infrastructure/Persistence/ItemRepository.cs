using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Items;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class ItemRepository(LendingDbContext db) : IItemRepository
{
    public async Task<Item> CreateAsync(string objectName, string description, CancellationToken cancellationToken)
    {
        var entity = new ItemEntity
        {
            ObjectName = objectName,
            Description = description,
            CurrentStatus = ItemStatuses.Available
        };

        db.Items.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<Item?> GetByIdAsync(int objectId, CancellationToken cancellationToken)
    {
        var entity = await db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ObjectId == objectId, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await (
            from item in db.Items.AsNoTracking()
            join owner in db.Users.AsNoTracking() on item.OwnerId equals owner.UserId into owners
            from owner in owners.DefaultIfEmpty()
            orderby item.ObjectId
            select new ItemSummary(
                item.ObjectId,
                item.ObjectName,
                item.Description ?? "",
                item.CurrentStatus ?? "",
                owner == null ? null : owner.Name,
                owner == null ? null : owner.Email,
                item.ImageUrl))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Item?> UpdateAsync(int objectId, string? objectName, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken)
    {
        var entity = await db.Items.FirstOrDefaultAsync(x => x.ObjectId == objectId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            entity.ObjectName = objectName;
        }

        if (!string.IsNullOrEmpty(description))
        {
            entity.Description = description;
        }

        if (!string.IsNullOrEmpty(currentStatus))
        {
            entity.CurrentStatus = currentStatus;
        }

        if (imageUrl is not null)
        {
            entity.ImageUrl = imageUrl;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        return await db.Media
            .AsNoTracking()
            .Where(x => x.ObjectId == itemId)
            .Select(x => new ItemMediaSummary(
                x.Type,
                x.Order == null || x.Order.User == null ? null : x.Order.User.Name,
                x.Description ?? "",
                x.Link ?? "",
                x.Url,
                ToDateTimeOffset(x.CreatedAt)))
            .ToArrayAsync(cancellationToken);
    }

    private static Item Map(ItemEntity entity) => new(
        entity.ObjectId,
        entity.ObjectName,
        entity.Description ?? "",
        entity.CurrentStatus ?? "",
        entity.ImageUrl);

    private static DateTimeOffset ToDateTimeOffset(DateTime? value) =>
        value is null ? default : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
