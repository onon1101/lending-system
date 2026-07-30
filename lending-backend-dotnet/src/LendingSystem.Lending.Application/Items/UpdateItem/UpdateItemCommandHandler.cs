using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Item;
using LendingSystem.Lending.Domain.Item_old;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.UpdateItem;

internal sealed class UpdateItemCommandHandler(
    IItemCommandRepository items,
    IItemQueryRepository itemQueries) : IRequestHandler<UpdateItemCommand, Result<UpdateItemResult>>
{
    public async Task<Result<UpdateItemResult>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var itemId = request.ItemId;
        if (!string.IsNullOrWhiteSpace(request.OwnerUsername) && !string.IsNullOrWhiteSpace(request.OriginalObjectName))
        {
            var itemByName = await itemQueries.GetByNameAsync(request.OwnerUsername, request.OriginalObjectName, cancellationToken);
            itemId = itemByName?.ItemId ?? 0;
        }

        var existingItem = await items.GetByIdForCommandAsync(itemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<UpdateItemResult>.Failure(ItemErrors.ItemNotFound());
        }

        if (!request.IsAdmin && existingItem.OwnerId != request.CurrentUserId)
        {
            return Result<UpdateItemResult>.Failure(ItemErrors.UpdateOwnItemsOnly());
        }

        var item = await items.UpdateAsync(
            itemId,
            request.ObjectName?.Trim(),
            request.Maker?.Trim(),
            request.Material?.Trim(),
            request.Description,
            request.CurrentStatus,
            request.ImageUrl,
            cancellationToken);

        return Result<UpdateItemResult>.Success(Map(item!));
    }

    private static UpdateItemResult Map(ItemAggregate item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.ImageUrl);
}
