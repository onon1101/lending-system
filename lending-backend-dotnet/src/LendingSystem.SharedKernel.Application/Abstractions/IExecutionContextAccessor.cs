namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }
    long CurrentUserId { get; }
    string Email { get; }
    string PasswordHash { get; }
    bool IsAdmin { get; }
    bool CanAccessUser(long userId);
}
