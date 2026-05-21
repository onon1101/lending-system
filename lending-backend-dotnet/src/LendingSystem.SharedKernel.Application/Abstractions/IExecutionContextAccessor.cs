namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }
    string Email { get; }
    string PasswordHash { get; }
}