using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Media;

internal sealed class UploadPrivateMediaCommandHandler(
    IMediaRepository media,
    IObjectStorage storage) : IRequestHandler<UploadPrivateMediaCommand, Result<UploadPrivateMediaResult>>
{
    public async Task<Result<UploadPrivateMediaResult>> Handle(UploadPrivateMediaCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId is null)
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
            request.OrderId.Value,
            request.ObjectId,
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
