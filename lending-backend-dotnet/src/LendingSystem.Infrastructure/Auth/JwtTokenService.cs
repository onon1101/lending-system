using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LendingSystem.Infrastructure.Auth;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public TokenPair Generate(User user)
    {
        var secret = configuration["SECRET_KEY"] ?? configuration["Jwt:SecretKey"] ?? "development-secret-key-change-before-production";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTimeOffset.UtcNow;

        var accessClaims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var access = new JwtSecurityToken(
            claims: accessClaims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(15).UtcDateTime,
            signingCredentials: credentials);

        var refresh = new JwtSecurityToken(
            claims: [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())],
            notBefore: now.UtcDateTime,
            expires: now.AddDays(7).UtcDateTime,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return new TokenPair(handler.WriteToken(access), handler.WriteToken(refresh));
    }
}
