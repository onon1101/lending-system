using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using MediatR;

namespace LendingSystem.Lending.Infrastructure.Persistence;

internal sealed class ItemCreatedDomainEventHandler(LendingDbContext db) : INotificationHandler<ItemCreatedDomainEvent>
{
    public async Task Handle(ItemCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var item = notification.Item;
        var entity = new ItemEntity
        {
            OwnerId = item.OwnerId,
            ObjectName = item.ObjectName,
            Maker = item.Maker,
            Material = item.Material,
            Description = item.Description,
            CurrentStatus = item.CurrentStatus,
            ImageUrl = item.ImageUrl
        };

        db.Items.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        notification.CreatedItem = new Item(
            entity.ItemId,
            entity.OwnerId,
            entity.ObjectName,
            entity.Maker,
            entity.Material,
            entity.Description ?? "",
            entity.CurrentStatus ?? "",
            entity.ImageUrl);
    }
}

internal sealed class ItemMediaAddedDomainEventHandler(LendingDbContext db) : INotificationHandler<ItemMediaAddedDomainEvent>
{
    public async Task Handle(ItemMediaAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var media = notification.Media;
        var entity = new ItemMediaEntity
        {
            ItemId = media.ItemId,
            Type = media.Type,
            Url = media.Url,
            Link = media.Link,
            Description = media.Description
        };

        db.ItemMedia.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        notification.CreatedMedia = ItemMedia.Rehydrate(
            entity.MediaId,
            entity.ItemId,
            entity.Type,
            entity.Url,
            entity.Link,
            entity.Description,
            DomainEventHandlerDateTime.ToDateTimeOffset(entity.CreatedAt));
    }
}

internal sealed class LoanCreatedDomainEventHandler(LendingDbContext db) : INotificationHandler<LoanCreatedDomainEvent>
{
    public async Task Handle(LoanCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var loan = notification.Loan;
        var entity = new OrderEntity
        {
            BorrowerDetailId = notification.BorrowerDetailId,
            ObjectId = loan.ItemId,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            ActualReturnDate = loan.ActualReturnDate,
            Status = loan.Status
        };

        db.Orders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        notification.CreatedLoan = new UserLoan(
            entity.OrderId,
            0,
            entity.StartDate,
            entity.EndDate,
            entity.Status,
            [
                new LoanItemDetail(
                    entity.OrderId,
                    entity.ObjectId,
                    "",
                    entity.Status,
                    entity.ActualReturnDate)
            ]);
    }
}

internal sealed class LoanRequestCreatedDomainEventHandler(LendingDbContext db) : INotificationHandler<LoanRequestCreatedDomainEvent>
{
    public async Task Handle(LoanRequestCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var loan = notification.Loan;
        var entity = new OrderEntity
        {
            BorrowerDetailId = notification.BorrowerDetailId,
            ObjectId = loan.ItemId,
            StartDate = loan.StartDate,
            EndDate = loan.EndDate,
            ActualReturnDate = loan.ActualReturnDate,
            Status = loan.Status
        };

        db.Orders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        notification.CreatedLoan = new UserLoan(
            entity.OrderId,
            notification.BorrowerUserId,
            entity.StartDate,
            entity.EndDate,
            entity.Status,
            [
                new LoanItemDetail(
                    entity.OrderId,
                    entity.ObjectId,
                    notification.ItemName,
                    entity.Status,
                    entity.ActualReturnDate)
            ]);
    }
}

internal sealed class LoanMediaAddedDomainEventHandler(LendingDbContext db) : INotificationHandler<LoanMediaAddedDomainEvent>
{
    public async Task Handle(LoanMediaAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var media = notification.Media;
        var entity = new LendingMediaEntity
        {
            OrderId = media.OrderId,
            Type = media.Type,
            Url = media.Url,
            Link = media.Link,
            Description = media.Description
        };

        db.LendingMedia.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        notification.CreatedMedia = LoanMedia.Rehydrate(
            entity.MediaId,
            entity.OrderId,
            entity.Type,
            entity.Url,
            entity.Link,
            entity.Description,
            DomainEventHandlerDateTime.ToDateTimeOffset(entity.CreatedAt));
    }
}

file static class DomainEventHandlerDateTime
{
    public static DateTimeOffset ToDateTimeOffset(DateTime? value) =>
        value is null ? default : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
}
