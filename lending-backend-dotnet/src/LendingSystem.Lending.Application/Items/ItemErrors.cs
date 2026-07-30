using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Items;

public static class ItemErrors
{
    public static Errors ObjectNameRequired() =>
        new("Item.ObjectNameRequired", "Object name is required", ErrorType.Validation);

    public static Errors ItemNameRequired() =>
        new("Item.NameRequired", "Item name is required", ErrorType.Validation);

    public static Errors UsernameRequired() =>
        new("Item.UsernameRequired", "Username is required", ErrorType.Validation);

    public static Errors ItemNotFound() =>
        new("Item.NotFound", "Item not found", ErrorType.NotFound);

    public static Errors UserNotFound() =>
        new("Item.UserNotFound", "User not found", ErrorType.NotFound);

    public static Errors UpdateOwnItemsOnly() =>
        new("Item.UpdateOwnItemsOnly", "You can only update your own items", ErrorType.Unauthorized);

    public static Errors UnsupportedFileType() =>
        new("Item.UnsupportedFileType", "Unsupported file type", ErrorType.Validation);

    public static Errors FileMustBeImageType() =>
        new("Item.FileMustBeImageType", "File must be an image type", ErrorType.Validation);
}
