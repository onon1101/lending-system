using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Infrastructure.Persistence;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public sealed class MediaRepository(LendingDbContext db) : IMediaRepository
{
    public async Task<MediaAsset> CreateAsync(int? orderId, int objectId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        var entity = new MediaEntity
        {
            OrderId = orderId,
            ObjectId = objectId,
            Type = type,
            Url = url,
            Link = link,
            Description = description
        };

        db.Media.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return new MediaAsset(
            entity.MediaId,
            entity.OrderId,
            entity.ObjectId,
            entity.Type,
            entity.Description ?? "",
            entity.Url,
            entity.Link ?? "",
            entity.CreatedAt is null ? default : new DateTimeOffset(DateTime.SpecifyKind(entity.CreatedAt.Value, DateTimeKind.Utc)));
    }
}
