using LendingSystem.Auth.Domain.Users;

namespace LendingSystem.Auth.Application.Abstractions;

public sealed record TokenPair(string AccessToken, string RefreshToken);

public interface ITokenService
{
    TokenPair Generate(UserEntity userEntity);
}
