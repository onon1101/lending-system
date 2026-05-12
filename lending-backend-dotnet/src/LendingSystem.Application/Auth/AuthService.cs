using System.IO.Pipelines;
using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;

namespace LendingSystem.Application.Auth;

public sealed class AuthService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens)
{
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Unauthorized, "帳號或密碼錯誤");
        }

        var tokenPair = tokens.Generate(user);
        return Result<AuthResponse>.Success(new AuthResponse(tokenPair.AccessToken, tokenPair.RefreshToken));
    }

    public async Task<Result<UserResponse>> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<UserResponse>.Failure(ErrorCodes.Validation, "Invalid request body");
        }

        var passwordHash = request.PasswordHash.StartsWith("$2", StringComparison.Ordinal)
            ? request.PasswordHash
            : passwords.Hash(request.PasswordHash);

        var user = await users.CreateAsync(request.Name, request.Email, passwordHash, cancellationToken);
        return Result<UserResponse>.Success(new UserResponse(user.UserId, user.Name, user.Email));
    }

    public async Task<Result<UserResponse>> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? Result<UserResponse>.Failure(ErrorCodes.NotFound, "User not found")
            : Result<UserResponse>.Success(new UserResponse(user.UserId, user.Name, user.Email));
    }

    public async Task<Result<UserResponse>> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        var user = await users.SearchByNameAsync(username, cancellationToken);
        return user is null
            ? Result<UserResponse>.Failure(ErrorCodes.NotFound, "User not found")
            : Result<UserResponse>.Success(new UserResponse(user.UserId, user.Name, user.Email));
    }

    public async Task<Result<DeleteResponse>> DeleteByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var IsSuccess = await users.DeleteAsync(userId, cancellationToken);

        return IsSuccess
            ? Result<DeleteResponse>.Success(new DeleteResponse(true, $"Delete user from userid {userId} is successful."))
            : Result<DeleteResponse>.Failure(ErrorCodes.ServerError, "Delete user is not successful.");
    }
}
