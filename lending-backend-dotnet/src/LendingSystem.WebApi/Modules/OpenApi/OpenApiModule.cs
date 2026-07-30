using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.OpenApi;

public sealed class OpenApiModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
