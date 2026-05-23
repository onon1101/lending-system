using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using Microsoft.Extensions.Hosting;

namespace LendingSystem.WebApi.Configuration.ExecutionContext;

public sealed class ExecutionContextAccessor(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IHostEnvironment environment) : IExecutionContextAccessor
{
    private static readonly Guid DefaultDevelopmentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid UserId => GetUserId();

    public int CurrentUserId => GetCurrentUserId();

    public string Email => GetClaimValue(ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email")
        ?? GetDevelopmentValue("Email", "dev.user@lending-system.local");

    public string PasswordHash => GetClaimValue("password_hash")
        ?? GetDevelopmentValue("PasswordHash", "dev-password-hash");

    public bool IsAdmin => GetCurrentUser()
        ?.FindAll(ClaimTypes.Role)
        .Concat(GetCurrentUser()?.FindAll("role") ?? [])
        .Any(x => string.Equals(x.Value, UserRole.Admin.Value, StringComparison.OrdinalIgnoreCase)) == true;

    public bool CanAccessUser(int userId) =>
        IsAdmin || userId > 0 && CurrentUserId == userId;

    private Guid GetUserId()
    {
        var userIdValue = GetClaimValue(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.Sub, "user_key");
        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return environment.IsDevelopment()
            ? GetDevelopmentUserId()
            : Guid.Empty;
    }

    private Guid GetDevelopmentUserId()
    {
        var configuredUserId = configuration["Development:ExecutionContext:UserId"]
            ?? configuration["Dev:ExecutionContext:UserId"];

        return Guid.TryParse(configuredUserId, out var userId)
            ? userId
            : DefaultDevelopmentUserId;
    }

    private int GetCurrentUserId()
    {
        var userKey = GetClaimValue("user_key", ClaimTypes.NameIdentifier);
        if (PublicResourceKey.TryGetInt("user", userKey, out var parsedUserId))
        {
            return parsedUserId;
        }

        var configuredUserId = configuration["Development:ExecutionContext:UserId"]
            ?? configuration["Dev:ExecutionContext:UserId"];

        return environment.IsDevelopment() && int.TryParse(configuredUserId, out var developmentUserId)
            ? developmentUserId
            : 0;
    }

    private string GetDevelopmentValue(string key, string fallback)
    {
        if (!environment.IsDevelopment())
        {
            return string.Empty;
        }

        return configuration[$"Development:ExecutionContext:{key}"]
            ?? configuration[$"Dev:ExecutionContext:{key}"]
            ?? fallback;
    }

    private string? GetClaimValue(params string[] claimTypes)
    {
        var user = GetCurrentUser();
        return claimTypes
            .Select(claimType => user?.FindFirstValue(claimType))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private ClaimsPrincipal? GetCurrentUser() => httpContextAccessor.HttpContext?.User;
}
