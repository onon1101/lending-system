using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Loans;

public sealed class LoanMedia : Entity
{
    private LoanMedia(
        long mediaId,
        long orderId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt)
    {
        MediaId = mediaId;
        OrderId = orderId;
        Type = NormalizeRequired(type);
        Url = NormalizeRequired(url);
        Link = NormalizeOptional(link);
        Description = NormalizeOptional(description);
        CreatedAt = createdAt;
    }

    public long MediaId { get; }
    public long OrderId { get; }
    public string Type { get; }
    public string Url { get; }
    public string? Link { get; }
    public string? Description { get; }
    public DateTimeOffset CreatedAt { get; }

    public static LoanMedia Create(
        long orderId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt) =>
        new(0, orderId, type, url, link, description, createdAt);

    public static LoanMedia Rehydrate(
        long mediaId,
        long orderId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt) =>
        new(mediaId, orderId, type, url, link, description, createdAt);

    private static string NormalizeRequired(string value) => value.Trim();

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
