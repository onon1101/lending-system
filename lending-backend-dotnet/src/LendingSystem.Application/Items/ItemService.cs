using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using LendingSystem.Application.Media;
using LendingSystem.Domain.Items;
using LendingSystem.Domain.Media;

namespace LendingSystem.Application.Items;

public sealed class ItemService(IItemRepository items, IMediaRepository media, IObjectStorage storage)
{
    public async Task<Result<ItemResponse>> CreateAsync(CreateItemRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectName))
        {
            return Result<ItemResponse>.Failure(ErrorCodes.Validation, "ObjectName is required");
        }

        return Result<ItemResponse>.Success(Map(await items.CreateAsync(request.ObjectName, request.Description, cancellationToken)));
    }

    public async Task<Result<ItemResponse>> GetByIdAsync(int objectId, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(objectId, cancellationToken);
        return item is null
            ? Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found")
            : Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<IReadOnlyCollection<ItemSummaryResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await items.GetAllAsync(cancellationToken);
        return Result<IReadOnlyCollection<ItemSummaryResponse>>.Success(result.Select(x => new ItemSummaryResponse(
            x.ObjectId,
            x.ObjectName,
            x.Description,
            x.CurrentStatus,
            x.OwnerName,
            x.OwnerEmail,
            x.ImageUrl)).ToArray());
    }

    public async Task<Result<ItemResponse>> UpdateAsync(int objectId, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var item = await items.UpdateAsync(objectId, request.ObjectName, request.Description, request.CurrentStatus, request.ImageUrl, cancellationToken);
        return item is null
            ? Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found")
            : Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<ItemResponse>> UploadImageAsync(int objectId, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ItemResponse>.Failure(ErrorCodes.UnsupportedFileType, "File must be an image type");
        }

        var stored = await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken);
        var item = await items.UpdateAsync(objectId, null, null, null, RewritePublicMediaHost(stored.Url), cancellationToken);
        if (item is null)
        {
            await storage.DeleteObjectAsync(stored.ObjectName, cancellationToken);
            return Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found");
        }

        return Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<MediaResponse>> UploadMediaAsync(int? orderId, int objectId, string description, string link, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var upload = await UploadMediaFileAsync(stream, size, fileName, contentType, cancellationToken);
        if (!upload.IsSuccess)
        {
            return Result<MediaResponse>.Failure(upload.Error.Code, upload.Error.Message);
        }

        var asset = await media.CreateAsync(orderId, objectId, upload.Data!.Type, RewritePublicMediaHost(upload.Data.Stored.Url), link, description, cancellationToken);
        return Result<MediaResponse>.Success(MediaResponse.From(asset));
    }

    public async Task<Result<IReadOnlyCollection<ItemMediaResponse>>> GetMediaAsync(int objectId, CancellationToken cancellationToken)
    {
        var result = await items.GetMediaByItemIdAsync(objectId, cancellationToken);
        return Result<IReadOnlyCollection<ItemMediaResponse>>.Success(result.Select(x => new ItemMediaResponse(x.Type, x.Creator, x.Description, x.OriginalLink, x.Media, x.CreatedAt)).ToArray());
    }

    private async Task<Result<UploadedMediaFile>> UploadMediaFileAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Video, await storage.UploadItemVideoAsync(stream, size, fileName, contentType, cancellationToken)));
        }

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<UploadedMediaFile>.Success(new UploadedMediaFile(MediaTypes.Image, await storage.UploadItemImageAsync(stream, size, fileName, contentType, cancellationToken)));
        }

        return Result<UploadedMediaFile>.Failure(ErrorCodes.UnsupportedFileType, "Unsupported file type");
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

    private sealed record UploadedMediaFile(string Type, StoredObject Stored);
}
