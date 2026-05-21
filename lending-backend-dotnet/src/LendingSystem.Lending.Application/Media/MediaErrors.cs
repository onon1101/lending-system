using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Media;

public static class MediaErrors
{
    public static ApplicationErrors UnsupportedFileType() =>
        new("MEDIA_UNSUPPORTED_FILE_TYPE", "Unsupported file type", "Unsupported file type", ErrorType.Validation);

    public static ApplicationErrors LendingOrderRequired() =>
        new("MEDIA_LENDING_ORDER_REQUIRED", "order_id is required for lending media", "Order id is required", ErrorType.Validation);
}
