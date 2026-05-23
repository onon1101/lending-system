using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LendingSystem.Auth.Infrastructure.Auth;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public TokenPair Generate(long userId, string username, string email, string role)
    {
        var secret = configuration["SECRET_KEY"] ?? configuration["Jwt:SecretKey"] ?? "development-secret-key-change-before-production";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTimeOffset.UtcNow;

        var accessClaims = new[]
        {
            new Claim("user_key", PublicResourceKey.FromInt("user", userId)),
            new Claim("username", username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, PublicResourceKey.FromInt("user", userId)),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var access = new JwtSecurityToken(
            claims: accessClaims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(15).UtcDateTime,
            signingCredentials: credentials);

        var refresh = new JwtSecurityToken(
            claims: [new Claim(JwtRegisteredClaimNames.Sub, PublicResourceKey.FromInt("user", userId))],
            notBefore: now.UtcDateTime,
            expires: now.AddDays(7).UtcDateTime,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return new TokenPair(handler.WriteToken(access), handler.WriteToken(refresh));
    }
}
