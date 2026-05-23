using Microsoft.AspNetCore.Authorization;

namespace LendingSystem.WebApi.Configuration.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                HasPermissionAttribute.PolicyName,
                policy => policy.Requirements.Add(new HasPermissionAuthorizationRequirement()));
        });

        services.AddSingleton<IAuthorizationHandler, HasPermissionAuthorizationHandler>();

        return services;
    }
}
