using LendingSystem.WebApi.Options;
using System.Collections.Concurrent;
using System.Reflection;

namespace LendingSystem.WebApi.Modules.OptionsRegisteration;

public static class OptionsRegisterationExtension
{
    private delegate void ConfigureOptionsDelegate(
        IServiceCollection services,
        IConfiguration configuration);

    private static readonly ConcurrentDictionary<
        Type,
        ConfigureOptionsDelegate> ConfigureDelegates = new();

    private static readonly MethodInfo ConfigureMethod =
        typeof(OptionsRegisterationExtension)
            .GetMethod(
                nameof(ConfigureOption),
                BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException(
            $"Cannot find method {nameof(ConfigureOption)}.");

    public static IServiceCollection AddConfigurationOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var optionsTypes = GetTypes(assemblies);

        foreach (var optionsType in optionsTypes)
        {
            var configure = ConfigureDelegates.GetOrAdd(
                optionsType,
                CreateConfigureDelegate);

            configure(
                services,
                configuration);
        }

        return services;
    }

    private static IEnumerable<Type> GetTypes(params Assembly[] assemblies) =>
        assemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(IConfigurationOptions).IsAssignableFrom(type));

    private static ConfigureOptionsDelegate CreateConfigureDelegate(
        Type optionsType) =>
        ConfigureMethod
            .MakeGenericMethod(optionsType)
            .CreateDelegate<ConfigureOptionsDelegate>();

    private static void ConfigureOption<TOptions>(
        IServiceCollection services,
        IConfiguration configuration)
        where TOptions : class, IConfigurationOptions =>
        services.Configure<TOptions>(
            configuration.GetRequiredSection(TOptions.SettingsName));
}
