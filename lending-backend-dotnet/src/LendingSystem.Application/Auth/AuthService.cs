using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Common;

namespace LendingSystem.Application.Auth;

public sealed class AuthService(IUserRepository users, IPasswordHasher passwords, ITokenService tokens)
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("帳號或密碼錯誤");
        }

        var tokenPair = tokens.Generate(user);
        return new AuthResponse(tokenPair.AccessToken, tokenPair.RefreshToken);
    }

    public async Task<UserResponse> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainException("Invalid request body");
        }

        var passwordHash = request.PasswordHash.StartsWith("$2", StringComparison.Ordinal)
            ? request.PasswordHash
            : passwords.Hash(request.PasswordHash);

        var user = await users.CreateAsync(request.Name, request.Email, passwordHash, cancellationToken);
        return new UserResponse(user.UserId, user.Name, user.Email);
    }

    public async Task<UserResponse> GetByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? throw new KeyNotFoundException("User not found")
            : new UserResponse(user.UserId, user.Name, user.Email);
    }

    public async Task<UserResponse> SearchByNameAsync(string username, CancellationToken cancellationToken)
    {
        var user = await users.SearchByNameAsync(username, cancellationToken);
        return user is null
            ? throw new KeyNotFoundException("User not found")
            : new UserResponse(user.UserId, user.Name, user.Email);
    }
}
