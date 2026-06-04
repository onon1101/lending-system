using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.DeleteUser;

public sealed record DeleteUserCommand(long UserId) : ICommand<DeleteUserResult>;
