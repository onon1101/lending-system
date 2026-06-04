using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.ValueObject;
using Microsoft.Extensions.Hosting;
using ExecutionContextModel = LendingSystem.SharedKernel.Application.Common.ExecutionContext;

namespace LendingSystem.WebApi.Configuration.ExecutionContext;

public sealed class ExecutionContextAccessor(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IHostEnvironment environment)
    : IExecutionContextAccessor
{
    private static readonly Guid DefaultDevelopmentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public ExecutionContextModel Current => new()
    {
        User = new CurrentUserContext
        {
            IsAuthenticated = GetCurrentUser()?.Identity?.IsAuthenticated == true,
            IdentityUserId = GetUserId(),
            UserId = GetCurrentUserId(),
            Username = GetClaimValue(ClaimTypes.Name, JwtRegisteredClaimNames.Name, "username", "name")
                ?? GetDevelopmentValue("Username", "dev.user"),
            Email = GetClaimValue(ClaimTypes.Email, JwtRegisteredClaimNames.Email, "email")
                ?? GetDevelopmentValue("Email", "dev.user@lending-system.local"),
            Roles = GetRoles()
        },
        Runtime = new RuntimeContext
        {
            EnvironmentName = environment.EnvironmentName,
            IsDevelopment = environment.IsDevelopment(),
            ApplicationName = environment.ApplicationName
        }
    };

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

    private long GetCurrentUserId()
    {
        var userKey = GetClaimValue("user_key", ClaimTypes.NameIdentifier);
        if (PublicResourceKey.TryGetInt("user", userKey, out var parsedUserId))
        {
            return parsedUserId;
        }

        var configuredUserId = configuration["Development:ExecutionContext:UserId"]
            ?? configuration["Dev:ExecutionContext:UserId"];

        return environment.IsDevelopment() && long.TryParse(configuredUserId, out var developmentUserId)
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

    private IReadOnlyCollection<string> GetRoles()
    {
        var user = GetCurrentUser();
        var roles = user
            ?.FindAll(ClaimTypes.Role)
            .Concat(user.FindAll("role"))
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (roles is { Length: > 0 })
        {
            return roles;
        }

        return environment.IsDevelopment()
            ? [GetDevelopmentValue("Role", UserRole.User.Value)]
            : [];
    }

    private ClaimsPrincipal? GetCurrentUser() => httpContextAccessor.HttpContext?.User;
}
