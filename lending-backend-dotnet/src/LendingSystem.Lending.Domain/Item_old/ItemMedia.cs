//using LendingSystem.SharedKernel.Domain.Common;

//namespace LendingSystem.Lending.Domain.Item;

//public sealed class ItemMedia : Entity
//{
//    private ItemMedia(
//        long mediaId,
//        long itemId,
//        string type,
//        string url,
//        string? link,
//        string? description,
//        DateTimeOffset createdAt)
//    {
//        MediaId = mediaId;
//        ItemId = itemId;
//        Type = NormalizeRequired(type);
//        Url = NormalizeRequired(url);
//        Link = NormalizeOptional(link);
//        Description = NormalizeOptional(description);
//        CreatedAt = createdAt;
//    }

//    public long MediaId { get; }
//    public long ItemId { get; }
//    public string Type { get; }
//    public string Url { get; }
//    public string? Link { get; }
//    public string? Description { get; }
//    public DateTimeOffset CreatedAt { get; }

//    public static ItemMedia Create(
//        long itemId,
//        string type,
//        string url,
//        string? link,
//        string? description,
//        DateTimeOffset createdAt) =>
//        new(0, itemId, type, url, link, description, createdAt);

//    public static ItemMedia Rehydrate(
//        long mediaId,
//        long itemId,
//        string type,
//        string url,
//        string? link,
//        string? description,
//        DateTimeOffset createdAt) =>
//        new(mediaId, itemId, type, url, link, description, createdAt);

//    private static string NormalizeRequired(string value) => value.Trim();

//    private static string? NormalizeOptional(string? value)
//    {
//        var normalized = value?.Trim();
//        return string.IsNullOrEmpty(normalized) ? null : normalized;
//    }
//}
