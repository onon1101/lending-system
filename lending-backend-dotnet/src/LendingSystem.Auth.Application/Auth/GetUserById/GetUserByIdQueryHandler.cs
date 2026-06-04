using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth.GetUserById;

internal sealed class GetUserByIdQueryHandler(IUserQueryRepository users) : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdResult>>
{
    public async Task<Result<GetUserByIdResult>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        return user is null
            ? Result<GetUserByIdResult>.Failure(AuthErrors.UserNotFound())
            : Result<GetUserByIdResult>.Success(new GetUserByIdResult(user.UserId, user.Name, user.Email));
    }
}
