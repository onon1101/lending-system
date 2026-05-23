namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }
    int CurrentUserId { get; }
    string Email { get; }
    string PasswordHash { get; }
    bool IsAdmin { get; }
    bool CanAccessUser(int userId);
}
