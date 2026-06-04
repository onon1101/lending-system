using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUserCommandRepository users,
    IUserQueryRepository userQueries, 
    IPasswordHasher passwords) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    public async Task<Result<RegisterUserResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<RegisterUserResult>.Failure(AuthErrors.InvalidRequestBody());
        }

        var isExists = await userQueries.GetExistsAsync(request.Name, request.Email,cancellationToken);
        if (isExists)
        {
            return Result<RegisterUserResult>.Failure(AuthErrors.UserIsExists());
        }

        var passwordHash = request.PasswordHash.StartsWith("$2", StringComparison.Ordinal)
            ? request.PasswordHash
            : passwords.Hash(request.PasswordHash);

        var user = await users.CreateAsync(request.Name, request.Email, passwordHash, cancellationToken);
        return Result<RegisterUserResult>.Success(new RegisterUserResult(user.Name, user.Email));
    }
}
