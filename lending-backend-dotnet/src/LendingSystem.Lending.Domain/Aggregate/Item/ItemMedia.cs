using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregate.Item;

public sealed class ItemMedia : Entity
{
    private ItemMedia(
        int mediaId,
        int itemId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt)
    {
        MediaId = mediaId;
        ItemId = itemId;
        Type = NormalizeRequired(type);
        Url = NormalizeRequired(url);
        Link = NormalizeOptional(link);
        Description = NormalizeOptional(description);
        CreatedAt = createdAt;
    }

    public int MediaId { get; }
    public int ItemId { get; }
    public string Type { get; }
    public string Url { get; }
    public string? Link { get; }
    public string? Description { get; }
    public DateTimeOffset CreatedAt { get; }

    public static ItemMedia Create(
        int itemId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt) =>
        new(0, itemId, type, url, link, description, createdAt);

    public static ItemMedia Rehydrate(
        int mediaId,
        int itemId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt) =>
        new(mediaId, itemId, type, url, link, description, createdAt);

    private static string NormalizeRequired(string value) => value.Trim();

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
