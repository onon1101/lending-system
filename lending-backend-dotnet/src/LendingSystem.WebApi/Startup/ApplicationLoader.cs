using System.Reflection;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Startup;

public static class ApplicationLoader
{
    public static WebApplicationBuilder ConfigureWebHost(this WebApplicationBuilder builder)
    {
        var appPort = builder.Configuration["APP_PORT"]
            ?? builder.Configuration["App:Port"]
            ?? "8000";

        builder.WebHost.UseUrls($"http://0.0.0.0:{appPort}");
        return builder;
    }

    public static IServiceCollection AddAllModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        Assembly assembly)
    {
        var modules = DiscoverModules(assembly);

        foreach (var module in modules)
        {
            module.InstallServices(services, configuration, environment);
        }

        services.AddSingleton<IReadOnlyCollection<IModuleInstaller>>(modules);
        return services;
    }

    public static WebApplication UseAllModules(this WebApplication app)
    {
        var modules = app.Services
            .GetRequiredService<IReadOnlyCollection<IModuleInstaller>>();

        foreach (var module in modules)
        {
            module.UseApplication(app);
        }

        return app;
    }

    private static IReadOnlyCollection<IModuleInstaller> DiscoverModules(Assembly assembly) =>
        assembly
            .GetTypes()
            .Where(type =>
                typeof(IModuleInstaller).IsAssignableFrom(type)
                && !type.IsInterface
                && !type.IsAbstract)
            .Select(type => (IModuleInstaller)(Activator.CreateInstance(type)
                ?? throw new InvalidOperationException($"Unable to create module {type.Name}.")))
            .ToArray();
}
