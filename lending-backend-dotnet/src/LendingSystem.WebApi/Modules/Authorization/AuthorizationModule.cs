using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.WebApi.Configuration.Authorization;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Authorization;

public sealed class AuthorizationModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddPermissionAuthorization();
        services.AddSingleton<IUserAccessChecker, UserAccessChecker>();
        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.UseAuthorization();
        return app;
    }
}
