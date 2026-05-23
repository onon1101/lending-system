using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Media;

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
