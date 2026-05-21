using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class DeleteUserCommandHandler(IUserRepository users) : IRequestHandler<DeleteUserCommand, Result<DeleteUserResult>>
{
    public async Task<Result<DeleteUserResult>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await users.DeleteAsync(request.UserId, cancellationToken);

        return isSuccess
            ? Result<DeleteUserResult>.Success(new DeleteUserResult(true, $"Delete user from userid {request.UserId} is successful."))
            : Result<DeleteUserResult>.Failure(AuthErrors.DeleteUserFailed());
    }
}
