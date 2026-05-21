using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Media;

public sealed record UploadPrivateMediaCommand(
    int? OrderId,
    int ObjectId,
    string Description,
    string Link,
    Stream Stream,
    long Size,
    string FileName,
    string ContentType) : ICommand<UploadPrivateMediaResult>;
