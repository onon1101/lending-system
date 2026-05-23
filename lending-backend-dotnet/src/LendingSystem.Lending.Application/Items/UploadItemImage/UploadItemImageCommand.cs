using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record UploadItemImageCommand(
    string OwnerUsername,
    string ObjectName,
    FileFormat FileFormat,
    int CurrentUserId,
    bool IsAdmin) : ICommand<UploadItemImageResult>;
