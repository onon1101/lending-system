using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Application.Media;
using LendingSystem.Lending.Domain.Item;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.UploadItemImage;

internal sealed class UploadItemImageCommandHandler(
    IItemCommandRepository items,
    IItemQueryRepository itemQueries,
    IObjectStorage storage) : IRequestHandler<UploadItemImageCommand, Result<UploadItemImageResult>>
{
    public async Task<Result<UploadItemImageResult>> Handle(UploadItemImageCommand request, CancellationToken cancellationToken)
    {
        var itemByName = await itemQueries.GetByNameAsync(request.OwnerUsername, request.ObjectName, cancellationToken);
        var existingItem = itemByName is null ? null : await items.GetByIdForCommandAsync(itemByName.ItemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<UploadItemImageResult>.Failure(ItemErrors.ItemNotFound());
        }

        if (!request.IsAdmin && existingItem.OwnerId != request.CurrentUserId)
        {
            return Result<UploadItemImageResult>.Failure(ItemErrors.UpdateOwnItemsOnly());
        }

        var uploadedImage = await UploadItemImageFileAsync(request.FileFormat, cancellationToken);
        if (!uploadedImage.IsSuccess)
        {
            return Result<UploadItemImageResult>.Failure(uploadedImage.Error);
        }

        var item = await items.UpdateAsync(
            existingItem.ItemId,
            null,
            null,
            null,
            null,
            null,
            MediaStorageHelper.RewritePublicMediaHost(uploadedImage.Data!.Url),
            cancellationToken);

        if (item is null)
        {
            await storage.DeleteObjectAsync(uploadedImage.Data.ObjectName, cancellationToken);
            return Result<UploadItemImageResult>.Failure(ItemErrors.ItemNotFound());
        }

        return Result<UploadItemImageResult>.Success(Map(item));
    }

    private static UploadItemImageResult Map(Item item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.ImageUrl);

    private async Task<Result<StoredObject>> UploadItemImageFileAsync(FileFormat fileFormat, CancellationToken cancellationToken)
    {
        if (!fileFormat.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<StoredObject>.Failure(ItemErrors.FileMustBeImageType());
        }

        var stored = await storage.UploadItemImageAsync(fileFormat.Stream, fileFormat.Size, fileFormat.FileName, fileFormat.ContentType, cancellationToken);
        return Result<StoredObject>.Success(stored);
    }
}
