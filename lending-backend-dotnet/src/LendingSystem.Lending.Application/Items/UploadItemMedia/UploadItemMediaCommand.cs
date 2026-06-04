using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.UploadItemMedia;

public sealed record UploadItemMediaCommand(
    string BorrowingKey,
    string OwnerUsername,
    string ObjectName,
    string Description,
    string Link,
    Stream Stream,
    long Size,
    string FileName,
    string ContentType,
    long CurrentUserId,
    bool IsAdmin) : ICommand<UploadItemMediaResult>;
