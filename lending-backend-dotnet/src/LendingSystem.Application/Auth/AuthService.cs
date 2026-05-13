using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth;
using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using Microsoft.Extensions.Configuration;

namespace LendingSystem.Application.Auth;

public sealed class AuthService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens, IConfiguration configuration)
{
    private const string GoogleProvider = "google";

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidEmail(request.Email))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Validation, "錯誤的 Email 格式");
        }

        var user = await users.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) || !passwords.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Unauthorized, "帳號或密碼錯誤");
        }

        var tokenPair = tokens.Generate(user);
        return Result<AuthResponse>.Success(new AuthResponse(tokenPair.AccessToken, tokenPair.RefreshToken));
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        if (!new EmailAddressAttribute().IsValid(email))
            return false;

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var domainParts = parts[1].Split('.');
        return domainParts.Length >= 2 &&
           domainParts.All(p => !string.IsNullOrWhiteSpace(p)) &&
           domainParts[^1].Length >= 2;
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

    public async Task<Result<AuthResponse>> GoogleLoginAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Validation, "Google token is required");
        }

        var googleClientId = configuration["GOOGLE_CLIENT_ID"] ?? configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(googleClientId))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.ServerError, "Google ClientId is not configured");
        }

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [googleClientId]
                });
        }
        catch (InvalidJwtException)
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Unauthorized, "Google 登入驗證失敗");
        }

        if (payload.EmailVerified != true || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
        {
            return Result<AuthResponse>.Failure(ErrorCodes.Unauthorized, "Google 帳號資料未驗證");
        }

        var user = await users.FindByProviderAsync(GoogleProvider, payload.Subject, cancellationToken);
        if (user is not null)
        {
            var existingTokenPair = tokens.Generate(user);
            return Result<AuthResponse>.Success(new AuthResponse(existingTokenPair.AccessToken, existingTokenPair.RefreshToken));
        }

        user = await users.FindByEmailAsync(payload.Email, cancellationToken);
        if (user is null)
        {
            var name = payload.Name ?? payload.Email.Split('@')[0];
            user = await users.CreateExternalAsync(
                name,
                payload.Email,
                GoogleProvider,
                payload.Subject,
                cancellationToken);
        }
        else if (user.AuthProvider != GoogleProvider || user.ProviderUserId != payload.Subject)
        {
            user = await users.LinkProviderAsync(user.Id, GoogleProvider, payload.Subject, cancellationToken);
            if (user is null)
            {
                return Result<AuthResponse>.Failure(ErrorCodes.Unauthorized, "Google 帳號連結失敗");
            }
        }

        var tokenPair = tokens.Generate(user);
        return Result<AuthResponse>.Success(new AuthResponse(tokenPair.AccessToken, tokenPair.RefreshToken));
    }
}
