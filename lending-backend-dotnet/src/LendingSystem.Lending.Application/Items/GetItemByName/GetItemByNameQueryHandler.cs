using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.GetItemByName;

internal sealed class GetItemByNameQueryHandler(IItemQueryRepository items) : IRequestHandler<GetItemByNameQuery, Result<GetItemByNameResult>>
{
    public async Task<Result<GetItemByNameResult>> Handle(GetItemByNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ItemName))
        {
            return Result<GetItemByNameResult>.Failure(ItemErrors.ItemNameRequired());
        }

        var item = await items.GetByNameAsync(request.OwnerUsername, Uri.UnescapeDataString(request.ItemName.Trim()), cancellationToken);
        return item is null
            ? Result<GetItemByNameResult>.Failure(ItemErrors.ItemNotFound())
            : Result<GetItemByNameResult>.Success(Map(item));
    }

    private static GetItemByNameResult Map(Item item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.ImageUrl);
}
