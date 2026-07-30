using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Time;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Time;

public sealed class TimeModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
