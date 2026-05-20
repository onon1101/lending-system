using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Media;

public static class MediaErrors
{
    public static ApplicationErrors UnsupportedFileType() =>
        new("MEDIA_UNSUPPORTED_FILE_TYPE", "Unsupported file type", "Unsupported file type", ErrorType.Validation);
}
