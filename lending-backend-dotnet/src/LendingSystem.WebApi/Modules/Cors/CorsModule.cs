using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.Cors;

public sealed class CorsModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors();
        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.UseCors(policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());

        return app;
    }
}
