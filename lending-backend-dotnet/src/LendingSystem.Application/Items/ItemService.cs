using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using LendingSystem.Application.Media;
using LendingSystem.Domain.Items;
using LendingSystem.Domain.Media;

namespace LendingSystem.Application.Items;

public sealed class ItemService(IItemRepository items, IMediaRepository media, IObjectStorage storage)
{
    public async Task<Result<ItemResponse>> CreateAsync(CreateItemRequest request, int userId, FileFormat? fileFormat, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectName))
        {
            return Result<ItemResponse>.Failure(ErrorCodes.Validation, "ObjectName is required");
        }

        var imageUrl = "";
        if (fileFormat is not null)
        {
            var uploadedImage = await UploadItemImageFileAsync(fileFormat, cancellationToken);
            if (!uploadedImage.IsSuccess)
            {
                return Result<ItemResponse>.Failure(uploadedImage.Error.Code, uploadedImage.Error.Message);
            }

            imageUrl = RewritePublicMediaHost(uploadedImage.Data!.Url);
        }

        return Result<ItemResponse>.Success(Map(await items.CreateAsync(
            userId,
            request.ObjectName.Trim(),
            request.Maker?.Trim() ?? "",
            request.Material?.Trim() ?? "",
            request.Description,
            imageUrl,
            cancellationToken)));
    }

    public async Task<Result<ItemResponse>> GetByIdAsync(int itemId, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(itemId, cancellationToken);
        return item is null
            ? Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found")
            : Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<ItemResponse>> GetByNameAsync(int userId, string itemName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return Result<ItemResponse>.Failure(ErrorCodes.Validation, "Item name is required");
        }

        var item = await items.GetByNameAsync(userId, Uri.UnescapeDataString(itemName.Trim()), cancellationToken);
        return item is null
            ? Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found")
            : Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<IReadOnlyCollection<ItemSummaryResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await items.GetAllAsync(cancellationToken);
        return Result<IReadOnlyCollection<ItemSummaryResponse>>.Success(result.Select(x => new ItemSummaryResponse(
            x.ItemId,
            x.OwnerId,
            x.ObjectName,
            x.Maker,
            x.Material,
            x.Description,
            x.CurrentStatus,
            x.OwnerUsername,
            x.OwnerName,
            x.OwnerEmail,
            x.ImageUrl)).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<ItemSummaryResponse>>> GetItemsByUserId(int userId,
        CancellationToken cancellationToken)
    {
        var result = await items.GetItemsByUserId(userId, cancellationToken);
        if (result is null)
        {
            return Result<IReadOnlyCollection<ItemSummaryResponse>>.Failure(ErrorCodes.NotFound, "User not found");
        }

        return Result<IReadOnlyCollection<ItemSummaryResponse>>.Success(MapSummary(result));
    }

    public async Task<Result<IReadOnlyCollection<ItemSummaryResponse>>> GetItemsByUserName(string username,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result<IReadOnlyCollection<ItemSummaryResponse>>.Failure(ErrorCodes.Validation, "Username is required");
        }

        var result = await items.GetItemsByUserName(Uri.UnescapeDataString(username.Trim()), cancellationToken);
        if (result is null)
        {
            return Result<IReadOnlyCollection<ItemSummaryResponse>>.Failure(ErrorCodes.NotFound, "User not found");
        }

        return Result<IReadOnlyCollection<ItemSummaryResponse>>.Success(MapSummary(result));
    }

    private static ItemSummaryResponse[] MapSummary(IReadOnlyCollection<ItemSummary> result) =>
        result.Select(x => new ItemSummaryResponse(
            x.ItemId,
            x.OwnerId,
            x.ObjectName,
            x.Maker,
            x.Material,
            x.Description,
            x.CurrentStatus,
            x.OwnerUsername,
            x.OwnerName,
            x.OwnerEmail,
            x.ImageUrl)).ToArray();

    public async Task<Result<ItemResponse>> UpdateAsync(int itemId, UpdateItemRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var existingItem = await items.GetByIdAsync(itemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found");
        }

        if (!isAdmin && existingItem.OwnerId != currentUserId)
        {
            return Result<ItemResponse>.Failure(ErrorCodes.Unauthorized, "You can only update your own items");
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
        return Result<ItemResponse>.Success(Map(item!));
    }

    public async Task<Result<ItemResponse>> UploadImageAsync(int itemId, FileFormat fileFormat, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var existingItem = await items.GetByIdAsync(itemId, cancellationToken);
        if (existingItem is null)
        {
            return Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found");
        }

        if (!isAdmin && existingItem.OwnerId != currentUserId)
        {
            return Result<ItemResponse>.Failure(ErrorCodes.Unauthorized, "You can only update your own items");
        }

        var uploadedImage = await UploadItemImageFileAsync(fileFormat, cancellationToken);
        if (!uploadedImage.IsSuccess)
        {
            return Result<ItemResponse>.Failure(uploadedImage.Error.Code, uploadedImage.Error.Message);
        }

        var item = await items.UpdateAsync(itemId, null, null, null, null, null, RewritePublicMediaHost(uploadedImage.Data!.Url), cancellationToken);
        if (item is null)
        {
            await storage.DeleteObjectAsync(uploadedImage.Data.ObjectName, cancellationToken);
            return Result<ItemResponse>.Failure(ErrorCodes.NotFound, "Item not found");
        }

        return Result<ItemResponse>.Success(Map(item));
    }

    public async Task<Result<MediaResponse>> UploadMediaAsync(int? orderId, int objectId, string description, string link, Stream stream, long size, string fileName, string contentType, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var existingItem = await items.GetByIdAsync(objectId, cancellationToken);
        if (existingItem is null)
        {
            return Result<MediaResponse>.Failure(ErrorCodes.NotFound, "Item not found");
        }

        if (!isAdmin && existingItem.OwnerId != currentUserId)
        {
            return Result<MediaResponse>.Failure(ErrorCodes.Unauthorized, "You can only update your own items");
        }

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

    private async Task<Result<StoredObject>> UploadItemImageFileAsync(FileFormat fileFormat, CancellationToken cancellationToken)
    {
        if (!fileFormat.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return Result<StoredObject>.Failure(ErrorCodes.UnsupportedFileType, "File must be an image type");
        }

        var stored = await storage.UploadItemImageAsync(fileFormat.Stream, fileFormat.Size, fileFormat.FileName, fileFormat.ContentType, cancellationToken);
        return Result<StoredObject>.Success(stored);
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

    private static ItemResponse Map(Item item) => new(item.ItemId, item.OwnerId, item.ObjectName, item.Maker, item.Material, item.Description, item.CurrentStatus, item.ImageUrl);

    private sealed record UploadedMediaFile(string Type, StoredObject Stored);
}
