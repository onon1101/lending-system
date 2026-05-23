namespace LendingSystem.Auth.Application.Abstractions;

public sealed record TokenPair(string AccessToken, string RefreshToken);

public interface ITokenService
{
    TokenPair Generate(int userId, string username, string email, string role);
}
