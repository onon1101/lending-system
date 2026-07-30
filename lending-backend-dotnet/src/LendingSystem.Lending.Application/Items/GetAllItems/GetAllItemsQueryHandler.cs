using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Item;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.GetAllItems;

internal sealed class GetAllItemsQueryHandler(IItemQueryRepository items) : IRequestHandler<GetAllItemsQuery, Result<IReadOnlyCollection<GetAllItemsResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetAllItemsResult>>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var result = await items.GetAllAsync(cancellationToken);
        return Result<IReadOnlyCollection<GetAllItemsResult>>.Success(result.Select(Map).ToArray());
    }

    private static GetAllItemsResult Map(ItemSummary item) => new(
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
