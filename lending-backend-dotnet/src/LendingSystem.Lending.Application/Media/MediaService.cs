using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.Lending.Domain.Media;

namespace LendingSystem.Lending.Application.Media;

public sealed class MediaService(IMediaRepository media, IObjectStorage storage)
{
    public async Task<Result<MediaResponse>> UploadPrivateAsync(int? orderId, int objectId, string description, string link, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var upload = await UploadAsync(stream, size, fileName, contentType, cancellationToken);
        if (!upload.IsSuccess)
        {
            return Result<MediaResponse>.Failure(upload.Error);
        }

        var asset = await media.CreateAsync(orderId, objectId, upload.Data!.Type, RewritePublicMediaHost(upload.Data.Stored.Url), link, description, cancellationToken);
        return Result<MediaResponse>.Success(MediaResponse.From(asset));
    }

    private async Task<Result<UploadedMediaFile>> UploadAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Video, await storage.UploadItemVideoAsync(stream, size, fileName, contentType, cancellationToken)));
        }

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Image, await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken)));
        }

        return Result<UploadedMediaFile>.Failure(MediaErrors.UnsupportedFileType());
    }

    private static string RewritePublicMediaHost(string url)
    {
        var builder = new UriBuilder(url)
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "lending-minio.onon1101.org",
            Port = -1
        };
        return builder.Uri.ToString();
    }

    private sealed record UploadedMediaFile(string Type, StoredObject Stored);
}
