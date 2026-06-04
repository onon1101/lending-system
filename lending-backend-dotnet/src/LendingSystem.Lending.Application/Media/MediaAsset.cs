using LendingSystem.Lending.Domain.Aggregates.Item;
using LendingSystem.Lending.Domain.Aggregates.Loans;

namespace LendingSystem.Lending.Application.Media;

public static class MediaTypes
{
    public const string Image = "image";
    public const string Video = "video";
}

public sealed record MediaAsset(
    long MediaId,
    long? OrderId,
    long ObjectId,
    string Type,
    string Description,
    string Url,
    string Link,
    DateTimeOffset CreatedAt)
{
    public long ItemId => ObjectId;

    public static MediaAsset FromItemMedia(ItemMedia media) => new(
        media.MediaId,
        null,
        media.ItemId,
        media.Type,
        media.Description ?? "",
        media.Url,
        media.Link ?? "",
        media.CreatedAt);

    public static MediaAsset FromLendingMedia(LoanMedia media, long itemId) => new(
        media.MediaId,
        media.OrderId,
        itemId,
        media.Type,
        media.Description ?? "",
        media.Url,
        media.Link ?? "",
        media.CreatedAt);
}
