using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.OptionsRegisteration;

public sealed class OptionsRegistrationModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) =>
        services.AddConfigurationOptions(
            configuration,
            typeof(Program).Assembly);
}
