using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

internal sealed class UploadItemMediaCommandHandler(
    IItemCommandRepository items,
    IItemQueryRepository itemQueries,
    IMediaCommandRepository media,
    IObjectStorage storage) : IRequestHandler<UploadItemMediaCommand, Result<UploadItemMediaResult>>
{
    public async Task<Result<UploadItemMediaResult>> Handle(UploadItemMediaCommand request, CancellationToken cancellationToken)
    {
        var itemByName = await itemQueries.GetByNameAsync(request.OwnerUsername, request.ObjectName, cancellationToken);
        var existingItem = itemByName is null ? null : await items.GetByIdForCommandAsync(itemByName.ItemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<UploadItemMediaResult>.Failure(ItemErrors.ItemNotFound());
        }

        if (!request.IsAdmin && existingItem.OwnerId != request.CurrentUserId)
        {
            return Result<UploadItemMediaResult>.Failure(ItemErrors.UpdateOwnItemsOnly());
        }

        var upload = await UploadMediaFileAsync(
            request.Stream,
            request.Size,
            request.FileName,
            request.ContentType,
            cancellationToken);

        if (!upload.IsSuccess)
        {
            return Result<UploadItemMediaResult>.Failure(upload.Error);
        }

        if (!PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId))
        {
            return Result<UploadItemMediaResult>.Failure(MediaErrors.LendingOrderRequired());
        }

        var asset = await media.CreateLendingMediaAsync(
            orderId,
            existingItem.ItemId,
            upload.Data!.Type,
            MediaStorageHelper.RewritePublicMediaHost(upload.Data.Stored.Url),
            request.Link,
            request.Description,
            cancellationToken);

        return Result<UploadItemMediaResult>.Success(new UploadItemMediaResult(
            asset.MediaId,
            asset.OrderId,
            asset.ObjectId,
            asset.Type,
            asset.Description,
            asset.Url,
            asset.Link,
            asset.CreatedAt));
    }

    private async Task<Result<UploadedMediaFile>> UploadMediaFileAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            var storedVideo = await storage.UploadItemVideoAsync(stream, size, fileName, contentType, cancellationToken);
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Video, storedVideo));
        }

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var storedImage = await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken);
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Image, storedImage));
        }

        return Result<UploadedMediaFile>.Failure(ItemErrors.UnsupportedFileType());
    }

    private sealed record UploadedMediaFile(string Type, StoredObject Stored);
}
