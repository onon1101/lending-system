using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.UploadItemImage;

public sealed record UploadItemImageCommand(
    string OwnerUsername,
    string ObjectName,
    FileFormat FileFormat,
    long CurrentUserId,
    bool IsAdmin) : ICommand<UploadItemImageResult>;
