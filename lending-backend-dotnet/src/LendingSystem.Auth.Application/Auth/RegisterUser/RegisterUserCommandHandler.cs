using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwords) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResult>>
{
    public async Task<Result<RegisterUserResult>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<RegisterUserResult>.Failure(AuthErrors.InvalidRequestBody());
        }

        var passwordHash = request.PasswordHash.StartsWith("$2", StringComparison.Ordinal)
            ? request.PasswordHash
            : passwords.Hash(request.PasswordHash);

        var user = await users.CreateAsync(request.Name, request.Email, passwordHash, cancellationToken);
        return Result<RegisterUserResult>.Success(new RegisterUserResult(user.UserId, user.Name, user.Email));
    }
}
