using System.Security.Claims;
using LendingSystem.Auth.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace LendingSystem.WebApi.Configuration.Authorization;

public sealed class HasPermissionAuthorizationHandler(IHostEnvironment environment)
    : AuthorizationHandler<HasPermissionAuthorizationRequirement>
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionsByRole =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [UserRole.User.Value] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Permissions.ReadUsers,
                Permissions.CreateItems,
                Permissions.UpdateItems,
                Permissions.UploadItemMedia,
                Permissions.ReadBorrowings,
                Permissions.CreateBorrowings,
                Permissions.ReturnBorrowings,
                Permissions.ManageBorrowings
            },
            [UserRole.Admin.Value] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Permissions.ReadUsers,
                Permissions.DeleteUsers,
                Permissions.CreateItems,
                Permissions.UpdateItems,
                Permissions.UploadItemMedia,
                Permissions.ReadBorrowings,
                Permissions.CreateBorrowings,
                Permissions.ReturnBorrowings,
                Permissions.ManageBorrowings
            }
        };

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionAuthorizationRequirement requirement)
    {
        if (environment.IsDevelopment())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var attribute = GetPermissionAttribute(context);
        if (attribute is null || context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var roles = context.User.FindAll(ClaimTypes.Role)
            .Concat(context.User.FindAll("role"))
            .Select(x => x.Value);

        if (roles.Any(role =>
                PermissionsByRole.TryGetValue(role, out var permissions) &&
                permissions.Contains(attribute.Name)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private static HasPermissionAttribute? GetPermissionAttribute(AuthorizationHandlerContext context)
    {
        return context.Resource switch
        {
            HttpContext httpContext => httpContext.GetEndpoint()?.Metadata.GetMetadata<HasPermissionAttribute>(),
            Endpoint endpoint => endpoint.Metadata.GetMetadata<HasPermissionAttribute>(),
            _ => null
        };
    }
}
