using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.WebApi.Configuration.ExecutionContext;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.ExecutionContext;

public sealed class ExecutionContextModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();
        return services;
    }
}
