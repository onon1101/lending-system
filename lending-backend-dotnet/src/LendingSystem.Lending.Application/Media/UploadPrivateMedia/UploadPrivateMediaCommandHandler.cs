using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Media.UploadPrivateMedia;

internal sealed class UploadPrivateMediaCommandHandler(
    IItemQueryRepository items,
    IMediaCommandRepository media,
    IObjectStorage storage) : IRequestHandler<UploadPrivateMediaCommand, Result<UploadPrivateMediaResult>>
{
    public async Task<Result<UploadPrivateMediaResult>> Handle(UploadPrivateMediaCommand request, CancellationToken cancellationToken)
    {
        if (!PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId))
        {
            return Result<UploadPrivateMediaResult>.Failure(MediaErrors.LendingOrderRequired());
        }

        var item = await items.GetByNameAsync(request.OwnerUsername, request.ObjectName, cancellationToken);
        if (item is null)
        {
            return Result<UploadPrivateMediaResult>.Failure(MediaErrors.LendingOrderRequired());
        }

        var upload = await UploadAsync(
            request.Stream,
            request.Size,
            request.FileName,
            request.ContentType,
            cancellationToken);

        if (!upload.IsSuccess)
        {
            return Result<UploadPrivateMediaResult>.Failure(upload.Error);
        }

        var asset = await media.CreateLendingMediaAsync(
            orderId,
            item.ItemId,
            upload.Data!.Type,
            MediaStorageHelper.RewritePublicMediaHost(upload.Data.Stored.Url),
            request.Link,
            request.Description,
            cancellationToken);

        return Result<UploadPrivateMediaResult>.Success(new UploadPrivateMediaResult(
            asset.MediaId,
            asset.OrderId,
            asset.ObjectId,
            asset.Type,
            asset.Description,
            asset.Url,
            asset.Link,
            asset.CreatedAt));
    }

    private async Task<Result<UploadedMediaFile>> UploadAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
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

        return Result<UploadedMediaFile>.Failure(MediaErrors.UnsupportedFileType());
    }

    private sealed record UploadedMediaFile(string Type, StoredObject Stored);
}
