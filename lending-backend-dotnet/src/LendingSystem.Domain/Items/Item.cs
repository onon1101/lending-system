namespace LendingSystem.Domain.Items;

public static class ItemStatuses
{
    public const string Available = "Available";
    public const string OnLoan = "On Loan";
}

public sealed record Item(
    int ItemId,
    string ObjectName,
    string Maker,
    string Material,
    string Description,
    string CurrentStatus,
    string? ImageUrl);

public sealed record ItemSummary(
    int ItemId,
    string ObjectName,
    string Maker,
    string Material,
    string Description,
    string CurrentStatus,
    string? OwnerName,
    string? OwnerEmail,
    string? ImageUrl);

public sealed record ItemMediaSummary(
    string Type,
    string? Creator,
    string Description,
    string OriginalLink,
    string Media,
    DateTimeOffset CreatedAt);
