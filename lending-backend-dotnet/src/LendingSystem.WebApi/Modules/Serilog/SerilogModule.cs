using LendingSystem.WebApi.Modules.Definitions;
using Serilog;

namespace LendingSystem.WebApi.Modules.Serilog;

public sealed class SerilogModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext());

        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
