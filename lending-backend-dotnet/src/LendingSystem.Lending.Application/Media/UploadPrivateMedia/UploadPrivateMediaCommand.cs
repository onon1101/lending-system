using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Media.UploadPrivateMedia;

public sealed record UploadPrivateMediaCommand(
    string BorrowingKey,
    string OwnerUsername,
    string ObjectName,
    string Description,
    string Link,
    Stream Stream,
    long Size,
    string FileName,
    string ContentType) : ICommand<UploadPrivateMediaResult>;
