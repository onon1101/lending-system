namespace LendingSystem.WebApi.Modules.Definitions;

/// <inheritdoc/>
public abstract class ModuleInstaller : IModuleInstaller
{
    /// <inheritdoc/>
    public virtual IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment) =>
        services;

    /// <inheritdoc/>
    public virtual IApplicationBuilder UseApplication(WebApplication app) =>
        app;
}
