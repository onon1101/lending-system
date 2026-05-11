using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Media;
using LendingSystem.Domain.Common;
using LendingSystem.Domain.Items;
using LendingSystem.Domain.Media;

namespace LendingSystem.Application.Items;

public sealed class ItemService(IItemRepository items, IMediaRepository media, IObjectStorage storage)
{
    public async Task<ItemResponse> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectName))
        {
            throw new DomainException("ObjectName is required");
        }

        return Map(await items.CreateAsync(request.ObjectName, request.Description, cancellationToken));
    }

    public async Task<ItemResponse> GetByIdAsync(int objectId, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(objectId, cancellationToken);
        return item is null ? throw new KeyNotFoundException("Item not found") : Map(item);
    }

    public async Task<IReadOnlyCollection<ItemSummaryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await items.GetAllAsync(cancellationToken);
        return result.Select(x => new ItemSummaryResponse(
            x.ObjectId,
            x.ObjectName,
            x.Description,
            x.CurrentStatus,
            x.OwnerName,
            x.OwnerEmail,
            x.ImageUrl)).ToArray();
    }

    public async Task<ItemResponse> UpdateAsync(int objectId, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var item = await items.UpdateAsync(objectId, request.ObjectName, request.Description, request.CurrentStatus, request.ImageUrl, cancellationToken);
        return item is null ? throw new KeyNotFoundException("Item not found") : Map(item);
    }

    public async Task<ItemResponse> UploadImageAsync(int objectId, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("File must be an image type");
        }

        var stored = await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken);
        try
        {
            var item = await items.UpdateAsync(objectId, null, null, null, stored.Url, cancellationToken);
            return item is null ? throw new KeyNotFoundException("Item not found") : Map(item);
        }
        catch
        {
            await storage.DeleteObjectAsync(stored.ObjectName, cancellationToken);
            throw;
        }
    }

    public async Task<MediaResponse> UploadMediaAsync(int? orderId, int objectId, string description, string link, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var upload = await UploadMediaFileAsync(stream, size, fileName, contentType, cancellationToken);
        try
        {
            var asset = await media.CreateAsync(orderId, objectId, upload.Type, RewritePublicMediaHost(upload.Stored.Url), link, description, cancellationToken);
            return MediaResponse.From(asset);
        }
        catch
        {
            await storage.DeleteObjectAsync(upload.Stored.ObjectName, cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<ItemMediaResponse>> GetMediaAsync(int objectId, CancellationToken cancellationToken)
    {
        var result = await items.GetMediaByItemIdAsync(objectId, cancellationToken);
        return result.Select(x => new ItemMediaResponse(x.Type, x.Creator, x.Description, x.OriginalLink, x.Media, x.CreatedAt)).ToArray();
    }

    private async Task<(string Type, StoredObject Stored)> UploadMediaFileAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
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

    private static ItemResponse Map(Item item) => new(item.ObjectId, item.ObjectName, item.Description, item.CurrentStatus, item.ImageUrl);
}
