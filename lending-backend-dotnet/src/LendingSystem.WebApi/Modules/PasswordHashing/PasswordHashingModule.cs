using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Infrastructure.Auth;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.PasswordHashing;

public sealed class PasswordHashingModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        return services;
    }
}
