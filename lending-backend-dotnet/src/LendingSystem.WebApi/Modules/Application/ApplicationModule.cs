using LendingSystem.Infrastructure;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Application;

public sealed class ApplicationModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) =>
        services
            .AddApplication()
            .AddInfrastructure(configuration);
}
