using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class SearchUserByNameQueryHandler(IUserRepository users) : IRequestHandler<SearchUserByNameQuery, Result<SearchUserByNameResult>>
{
    public async Task<Result<SearchUserByNameResult>> Handle(SearchUserByNameQuery request, CancellationToken cancellationToken)
    {
        var user = await users.SearchByNameAsync(request.Username, cancellationToken);
        return user is null
            ? Result<SearchUserByNameResult>.Failure(AuthErrors.UserNotFound())
            : Result<SearchUserByNameResult>.Success(new SearchUserByNameResult(user.UserId, user.Name, user.Email));
    }
}
