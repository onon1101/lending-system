using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Items;

public static class ItemErrors
{
    public static ApplicationErrors ObjectNameRequired() =>
        new("ITEM_OBJECT_NAME_REQUIRED", "ObjectName is required", "Object name is required", ErrorType.Validation);

    public static ApplicationErrors ItemNameRequired() =>
        new("ITEM_NAME_REQUIRED", "Item name is required", "Item name is required", ErrorType.Validation);

    public static ApplicationErrors UsernameRequired() =>
        new("ITEM_USERNAME_REQUIRED", "Username is required", "Username is required", ErrorType.Validation);

    public static ApplicationErrors ItemNotFound() =>
        new("ITEM_NOT_FOUND", "Item not found", "Item not found", ErrorType.NotFound);

    public static ApplicationErrors UserNotFound() =>
        new("ITEM_USER_NOT_FOUND", "User not found", "User not found", ErrorType.NotFound);

    public static ApplicationErrors UpdateOwnItemsOnly() =>
        new("ITEM_UPDATE_OWN_ITEMS_ONLY", "You can only update your own items", "You can only update your own items", ErrorType.Unauthorized);

    public static ApplicationErrors UnsupportedFileType() =>
        new("ITEM_UNSUPPORTED_FILE_TYPE", "Unsupported file type", "Unsupported file type", ErrorType.Validation);

    public static ApplicationErrors FileMustBeImageType() =>
        new("ITEM_FILE_MUST_BE_IMAGE_TYPE", "File must be an image type", "File must be an image type", ErrorType.Validation);
}
