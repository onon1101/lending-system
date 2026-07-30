using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Item;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.GetItemsByUserId;

internal sealed class GetItemsByUserIdQueryHandler(IItemQueryRepository items) : IRequestHandler<GetItemsByUserIdQuery, Result<IReadOnlyCollection<GetItemsByUserIdResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemsByUserIdResult>>> Handle(GetItemsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var result = await items.GetItemsByUserId(request.UserId, cancellationToken);
        return result is null
            ? Result<IReadOnlyCollection<GetItemsByUserIdResult>>.Failure(ItemErrors.UserNotFound())
            : Result<IReadOnlyCollection<GetItemsByUserIdResult>>.Success(result.Select(Map).ToArray());
    }

    private static GetItemsByUserIdResult Map(ItemSummary item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.OwnerUsername,
        item.OwnerName,
        item.OwnerEmail,
        item.ImageUrl);
}
