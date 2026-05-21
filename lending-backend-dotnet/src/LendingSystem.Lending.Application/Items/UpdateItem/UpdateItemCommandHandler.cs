using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Items;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

internal sealed class UpdateItemCommandHandler(IItemRepository items) : IRequestHandler<UpdateItemCommand, Result<UpdateItemResult>>
{
    public async Task<Result<UpdateItemResult>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var existingItem = await items.GetByIdAsync(request.ItemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<UpdateItemResult>.Failure(ItemErrors.ItemNotFound());
        }

        if (!request.IsAdmin && existingItem.OwnerId != request.CurrentUserId)
        {
            return Result<UpdateItemResult>.Failure(ItemErrors.UpdateOwnItemsOnly());
        }

        var item = await items.UpdateAsync(
            request.ItemId,
            request.ObjectName?.Trim(),
            request.Maker?.Trim(),
            request.Material?.Trim(),
            request.Description,
            request.CurrentStatus,
            request.ImageUrl,
            cancellationToken);

        return Result<UpdateItemResult>.Success(Map(item!));
    }

    private static UpdateItemResult Map(Item item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.ImageUrl);
}
