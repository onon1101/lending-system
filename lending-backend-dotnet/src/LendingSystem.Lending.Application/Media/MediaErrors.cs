using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Media;

public static class MediaErrors
{
    public static Errors UnsupportedFileType() =>
        new("Media.UnsupportedFileType", "Unsupported file type", ErrorType.Validation);

    public static Errors LendingOrderRequired() =>
        new("Media.LendingOrderRequired", "Borrowing key is required", ErrorType.Validation);
}
