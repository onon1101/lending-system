using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Application.Media;
using LendingSystem.Lending.Domain.Items;
using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

internal sealed class CreateItemCommandHandler(
    IItemRepository items,
    IObjectStorage storage) : IRequestHandler<CreateItemCommand, Result<CreateItemResult>>
{
    public async Task<Result<CreateItemResult>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectName))
        {
            return Result<CreateItemResult>.Failure(ItemErrors.ObjectNameRequired());
        }

        var imageUrl = "";
        if (request.FileFormat is not null)
        {
            var uploadedImage = await UploadItemImageFileAsync(request.FileFormat, cancellationToken);
            if (!uploadedImage.IsSuccess)
            {
                return Result<CreateItemResult>.Failure(uploadedImage.Error);
            }

            imageUrl = MediaStorageHelper.RewritePublicMediaHost(uploadedImage.Data!.Url);
        }

        var item = await items.CreateAsync(
            request.UserId,
            request.ObjectName.Trim(),
            request.Maker?.Trim() ?? "",
            request.Material?.Trim() ?? "",
            request.Description,
            imageUrl,
            cancellationToken);

        return Result<CreateItemResult>.Success(Map(item));
    }

    private static CreateItemResult Map(Item item) => new(
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
