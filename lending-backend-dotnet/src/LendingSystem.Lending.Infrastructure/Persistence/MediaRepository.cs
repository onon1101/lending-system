using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.Lending.Application.Media;
using MediatR;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public sealed class MediaRepository(IPublisher publisher) : IMediaCommandRepository
{
    public async Task<MediaAsset> CreateItemMediaAsync(long itemId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        var aggregate = ItemAggregate.Rehydrate(itemId, 0, "", "", "", "", ItemStatuses.Available, null);
        var media = ItemMedia.Create(itemId, type, url, link, description, DateTimeOffset.UtcNow);
        aggregate.AddMedia(media);

        var domainEvent = aggregate.DomainEvents.OfType<ItemMediaAddedDomainEvent>().Single();
        await publisher.Publish(domainEvent, cancellationToken);
        aggregate.ClearDomainEvents();

        return MediaAsset.FromItemMedia(domainEvent.CreatedMedia ?? media);
    }

    public async Task<MediaAsset> CreateLendingMediaAsync(long orderId, long itemId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        var loan = Loan.Rehydrate(orderId, itemId, DateOnly.MinValue, DateOnly.MinValue, null, LoanStatuses.OnLoan);
        var aggregate = LoansAggregate.Rehydrate(0, null, "", [loan]);
        var media = LoanMedia.Create(orderId, type, url, link, description, DateTimeOffset.UtcNow);
        aggregate.AddMedia(media);

        var domainEvent = aggregate.DomainEvents.OfType<LoanMediaAddedDomainEvent>().Single();
        await publisher.Publish(domainEvent, cancellationToken);
        aggregate.ClearDomainEvents();

        return MediaAsset.FromLendingMedia(domainEvent.CreatedMedia ?? media, itemId);
    }
}
