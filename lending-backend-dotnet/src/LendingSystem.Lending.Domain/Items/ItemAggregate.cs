using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Item;

public sealed class ItemAggregate : IAggregateRoot
{
    private readonly List<MediaEntity> _media = [];

    public ItemAggregate(long itemId,
        long ownerId,
        string? maker,
        string? material,
        string? description,
        ItemStatus status)
    {
        ItemId = itemId;
        OwnerId = ownerId;
        Maker = maker;
        Material = material;
        Description = description;
        CurrentStatus = status;
    }

    public long ItemId { get; }
    public long OwnerId { get; }
    public string? Maker { get; private set; }
    public string? Material { get; private set; }
    public string? Description { get; private set; }
    public ItemStatus CurrentStatus { get; private set; }

    public IReadOnlyCollection<MediaEntity> Medias => _media.AsReadOnly();

    public static Result<ItemAggregate> Create(
        long itemId,
        long ownerId,
        string? maker,
        string? material,
        string? description,
        ItemStatus status)
    {

        var item = new ItemAggregate(
            itemId,
            ownerId,
            maker,
            material,
            description,
            status);

        //item.AddDomainEvent()

        return Result<ItemAggregate>.Success(item);
    }

    public bool MarkOnLoan()
    {
        if (CurrentStatus == ItemStatus.OnLoan)
        {
            return false;
        }

        CurrentStatus = ItemStatus.OnLoan;
        return true;
    }

    public bool MarkAvaliable()
    {
        CurrentStatus = ItemStatus.Available;
        return true;
    }

    public bool AddMedia(MediaEntity media)
    {
        _media.Add(media);

        return true;
    }
}
