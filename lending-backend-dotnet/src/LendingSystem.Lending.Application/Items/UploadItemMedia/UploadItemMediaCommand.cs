using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record UploadItemMediaCommand(
    int? OrderId,
    int ObjectId,
    string Description,
    string Link,
    Stream Stream,
    long Size,
    string FileName,
    string ContentType,
    int CurrentUserId,
    bool IsAdmin) : ICommand<UploadItemMediaResult>;
