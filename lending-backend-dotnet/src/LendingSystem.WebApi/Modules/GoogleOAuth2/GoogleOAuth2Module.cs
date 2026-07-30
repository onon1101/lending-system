using LendingSystem.Auth.ACL.Google;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.GoogleOAuth2;

public sealed class GoogleOAuth2Module : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IGoogleOAuth2Acl, GoogleOAuth2Acl>();
        return services;
    }
}
