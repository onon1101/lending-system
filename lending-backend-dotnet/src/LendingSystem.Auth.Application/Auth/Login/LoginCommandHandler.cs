using System.ComponentModel.DataAnnotations;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwords,
    ITokenService tokens,
    EmailAddressAttribute emailAddressAttribute) : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!IsValidEmail(request.Email, emailAddressAttribute))
        {
            return Result<LoginResult>.Failure(AuthErrors.InvalidEmail());
        }

        var user = await users.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) || !passwords.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResult>.Failure(AuthErrors.InvalidCredentials());
        }

        var tokenPair = tokens.Generate(user);
        return Result<LoginResult>.Success(new LoginResult(tokenPair.AccessToken, tokenPair.RefreshToken));
    }

    private static bool IsValidEmail(string email, EmailAddressAttribute emailAddressAttribute)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (!emailAddressAttribute.IsValid(email))
        {
            return false;
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        var domainParts = parts[1].Split('.');
        return domainParts.Length >= 2 &&
            domainParts.All(p => !string.IsNullOrWhiteSpace(p)) &&
            domainParts[^1].Length >= 2;
    }
}
