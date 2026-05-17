using LendingSystem.Domain.Users;

namespace LendingSystem.Application.Abstractions;

public sealed record TokenPair(string AccessToken, string RefreshToken);

public interface ITokenService
{
    TokenPair Generate(UserEntity userEntity);
}
