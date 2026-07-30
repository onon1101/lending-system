using LendingSystem.WebApi.Modules.Definitions;
using LendingSystem.WebApi.Options;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace LendingSystem.WebApi.Modules.Swagger;

public sealed class SwaggerModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "物品借閱系統 API",
                Version = "1.0"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
        });

        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "物品借閱系統 API v1"));

        var appOptions = app.Services
            .GetRequiredService<IOptions<AppOptions>>()
            .Value;

        var swaggerUrl = $"http://{appOptions.Ip}:{appOptions.Port}/swagger/index.html";

        app.Logger.LogInformation(
            "Swagger UI is available at {SwaggerUrl}",
            swaggerUrl);

        return app;
    }
}
