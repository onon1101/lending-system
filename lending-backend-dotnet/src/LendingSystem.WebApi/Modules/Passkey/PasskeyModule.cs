using LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Passkey;

public sealed class PasskeyModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.Configure<PasskeyOptions>(
            configuration.GetSection(PasskeyOptions.SectionName));

        return services;
    }
}
