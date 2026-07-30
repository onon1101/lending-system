using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Item;
using LendingSystem.Lending.Domain.Item_old;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.GetItemById;

internal sealed class GetItemByIdQueryHandler(IItemQueryRepository items) : IRequestHandler<GetItemByIdQuery, Result<GetItemByIdResult>>
{
    public async Task<Result<GetItemByIdResult>> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(request.ItemId, cancellationToken);
        return item is null
            ? Result<GetItemByIdResult>.Failure(ItemErrors.ItemNotFound())
            : Result<GetItemByIdResult>.Success(Map(item));
    }

    private static GetItemByIdResult Map(ItemAggregate item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.ImageUrl);
}
