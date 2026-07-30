using System.ComponentModel.DataAnnotations;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Validation;

public sealed class ValidationModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<EmailAddressAttribute>();
        return services;
    }
}
