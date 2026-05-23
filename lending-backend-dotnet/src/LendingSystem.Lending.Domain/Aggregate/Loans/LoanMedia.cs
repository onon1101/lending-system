using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregate.Loans;

public sealed class LoanMedia : Entity
{
    private LoanMedia(
        int mediaId,
        int orderId,
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

    public int MediaId { get; }
    public int OrderId { get; }
    public string Type { get; }
    public string Url { get; }
    public string? Link { get; }
    public string? Description { get; }
    public DateTimeOffset CreatedAt { get; }

    public static LoanMedia Create(
        int orderId,
        string type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt) =>
        new(0, orderId, type, url, link, description, createdAt);

    public static LoanMedia Rehydrate(
        int mediaId,
        int orderId,
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
