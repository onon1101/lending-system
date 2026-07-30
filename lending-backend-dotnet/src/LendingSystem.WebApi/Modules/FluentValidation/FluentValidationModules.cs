using FluentValidation;
using LendingSystem.Auth.Application;
using LendingSystem.Lending.Application;
using LendingSystem.WebApi.Modules.Definitions;
using System.Reflection;

namespace LendingSystem.WebApi.Modules.FluentValidation;

public sealed class FluentValidationModules : ModuleInstaller
{
    public override IServiceCollection InstallServices(IServiceCollection services,
                                                       IConfiguration configuration,
                                                       IWebHostEnvironment environment)
    {
        Assembly[] applicationAssemblies =
        [
            typeof(AuthApplicationAssemblyMarker).Assembly,
            typeof(LendingApplicationAssemblyMarker).Assembly
        ];

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(applicationAssemblies);
        });

        RegisterValidatorsFromAssemblies(
            services,
            applicationAssemblies);

        return services;
    }

    private static void RegisterValidatorsFromAssemblies(
        IServiceCollection services,
        params Assembly[] assemblies)
    {
        var validatorTypes = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type is
            {
                IsAbstract: false,
                IsInterface: false
            })
            .Select(type => new
            {
                ImplementationType = type.AsType(),
                ServiceTypes = type
                    .GetInterfaces()
                    .Where(interfaceType =>
                        interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() ==
                        typeof(IValidator<>))
                    .ToArray()
            })
            .Where(registration =>
                registration.ServiceTypes.Length > 0);

        foreach (var validatorType in validatorTypes)
        {
            foreach (var serviceType in validatorType.ServiceTypes)
            {
                services.AddScoped(
                    serviceType,
                    validatorType.ImplementationType);
            }
        }
    }
}
