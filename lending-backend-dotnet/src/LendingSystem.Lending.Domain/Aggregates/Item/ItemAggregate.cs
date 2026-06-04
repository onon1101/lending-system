using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregate.Item;

public static class ItemStatuses
{
    public const string Available = "Available";
    public const string OnLoan = "On Loan";
    public const string UnAvailable = "Unavailable";
}

public sealed record Item(
    long ItemId,
    long OwnerId,
    string ObjectName,
    string Maker,
    string Material,
    string Description,
    string CurrentStatus,
    string? ImageUrl);

public sealed record ItemSummary(
    long ItemId,
    long OwnerId,
    string ObjectName,
    string Maker,
    string Material,
    string Description,
    string CurrentStatus,
    string? OwnerUsername,
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

public sealed class ItemAggregate : Entity, IAggregateRoot
{
    private readonly List<ItemMedia> _media;

    private ItemAggregate(
        long itemId,
        long ownerId,
        string objectName,
        string maker,
        string material,
        string description,
        string currentStatus,
        string? imageUrl,
        IEnumerable<ItemMedia>? media)
    {
        ItemId = itemId;
        OwnerId = ownerId;
        ObjectName = objectName;
        Maker = maker;
        Material = material;
        Description = description;
        CurrentStatus = currentStatus;
        ImageUrl = imageUrl;
        _media = media?.ToList() ?? [];
    }

    public long ItemId { get; }
    public long OwnerId { get; }
    public string ObjectName { get; private set; }
    public string Maker { get; private set; }
    public string Material { get; private set; }
    public string Description { get; private set; }
    public string CurrentStatus { get; private set; }
    public string? ImageUrl { get; private set; }
    public IReadOnlyCollection<ItemMedia> Media => _media.AsReadOnly();

    public static ItemAggregate Create(
        long ownerId,
        string objectName,
        string maker,
        string material,
        string description,
        string? imageUrl = null)
    {
        var item = new ItemAggregate(0, ownerId, objectName, maker, material, description, ItemStatuses.Available, imageUrl, null);
        item.AddDomainEvent(new ItemCreatedDomainEvent(item));
        return item;
    }

    public static ItemAggregate Rehydrate(
        long itemId,
        long ownerId,
        string objectName,
        string maker,
        string material,
        string description,
        string currentStatus,
        string? imageUrl,
        IEnumerable<ItemMedia>? media = null) =>
        new(itemId, ownerId, objectName, maker, material, description, currentStatus, imageUrl, media);

    public void MarkOnLoan() => CurrentStatus = ItemStatuses.OnLoan;

    public void MarkAvailable() => CurrentStatus = ItemStatuses.Available;

    public void AddMedia(ItemMedia media)
    {
        if (ItemId > 0 && media.ItemId != ItemId)
        {
            throw new InvalidOperationException("Media does not belong to this item.");
        }

        _media.Add(media);
        AddDomainEvent(new ItemMediaAddedDomainEvent(media));
    }
}

public sealed class ItemCreatedDomainEvent(ItemAggregate item) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public ItemAggregate Item { get; } = item;
    public Item? CreatedItem { get; set; }
}

public sealed class ItemMediaAddedDomainEvent(ItemMedia media) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public ItemMedia Media { get; } = media;
    public ItemMedia? CreatedMedia { get; set; }
}
