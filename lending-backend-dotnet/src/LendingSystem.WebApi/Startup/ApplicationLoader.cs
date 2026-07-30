using LendingSystem.WebApi.Modules.Definitions;
using System.Reflection;

namespace LendingSystem.WebApi.Startup;

/// <summary>
/// Builder service and Application's Loader 
/// </summary>
public static class ApplicationLoader
{
    /// <summary>
    /// using 0.0.0.0:8000 as default address
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder ConfigureWebHost(this WebApplicationBuilder builder)
    {
        var appPort = builder.Configuration["APP_PORT"]
            ?? builder.Configuration["App:Port"]
            ?? "8000";

        builder.WebHost.UseUrls($"http://0.0.0.0:{appPort}");
        return builder;
    }

    /// <summary>
    /// Register all of methdos that inherit 'I'
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddAllModules(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        Assembly assembly)
    {
        // scan all service 
        var modules = DiscoverModules(assembly);

        foreach (var module in modules)
        {
            module.InstallServices(services, configuration, environment);
        }

        // register all of third party service
        services.AddSingleton(modules);
        return services;
    }

    /// <summary>
    /// scan all of service 
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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


    /// <summary>
    /// engage application
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
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
}
