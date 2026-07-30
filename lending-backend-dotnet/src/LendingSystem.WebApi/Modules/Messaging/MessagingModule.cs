using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Messaging;
using LendingSystem.WebApi.Modules.Definitions;
using MediatR;

namespace LendingSystem.WebApi.Modules.Messaging;

public sealed class MessagingModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(CommandMessageQueueBehavior<,>));

        return services;
    }
}
