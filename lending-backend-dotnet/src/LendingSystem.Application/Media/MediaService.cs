using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Common;
using LendingSystem.Domain.Media;

namespace LendingSystem.Application.Media;

public sealed class MediaService(IMediaRepository media, IObjectStorage storage)
{
    public async Task<MediaResponse> UploadPrivateAsync(int? orderId, int objectId, string description, string link, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var (type, stored) = await UploadAsync(stream, size, fileName, contentType, cancellationToken);
        try
        {
            var asset = await media.CreateAsync(orderId, objectId, type, RewritePublicMediaHost(stored.Url), link, description, cancellationToken);
            return MediaResponse.From(asset);
        }
        catch
        {
            await storage.DeleteObjectAsync(stored.ObjectName, cancellationToken);
            throw;
        }
    }

    private async Task<(string Type, StoredObject Stored)> UploadAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return (MediaTypes.Video, await storage.UploadItemVideoAsync(stream, size, fileName, contentType, cancellationToken));
        }

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return (MediaTypes.Image, await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken));
        }

        throw new DomainException("Unsupported file type");
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
}
