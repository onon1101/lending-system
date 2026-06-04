namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IUserAccessChecker
{
    bool CanAccessUser(bool isAdmin, long currentUserId, long userId);
}
