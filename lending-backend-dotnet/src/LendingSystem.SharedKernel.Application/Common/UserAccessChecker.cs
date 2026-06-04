using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.SharedKernel.Application.Common;

public sealed class UserAccessChecker : IUserAccessChecker
{
    public bool CanAccessUser(bool isAdmin, long currentUserId, long userId) =>
        isAdmin || userId > 0 && currentUserId == userId;
}
