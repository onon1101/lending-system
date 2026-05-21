using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Infrastructure.Persistence;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public sealed class MediaRepository(LendingDbContext db) : IMediaRepository
{
    public async Task<MediaAsset> CreateItemMediaAsync(int itemId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        var entity = new ItemMediaEntity
        {
            ItemId = itemId,
            Type = type,
            Url = url,
            Link = link,
            Description = description
        };

        db.ItemMedia.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new MediaAsset(
            entity.MediaId,
            null,
            entity.ItemId,
            entity.Type,
            entity.Description ?? "",
            entity.Url,
            entity.Link ?? "",
            entity.CreatedAt is null ? default : new DateTimeOffset(DateTime.SpecifyKind(entity.CreatedAt.Value, DateTimeKind.Utc)));
    }

    public async Task<MediaAsset> CreateLendingMediaAsync(int orderId, int itemId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        var entity = new LendingMediaEntity
        {
            OrderId = orderId,
            Type = type,
            Url = url,
            Link = link,
            Description = description
        };

        db.LendingMedia.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new MediaAsset(
            entity.MediaId,
            entity.OrderId,
            itemId,
            entity.Type,
            entity.Description ?? "",
            entity.Url,
            entity.Link ?? "",
            entity.CreatedAt is null ? default : new DateTimeOffset(DateTime.SpecifyKind(entity.CreatedAt.Value, DateTimeKind.Utc)));
    }
}
