using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Infrastructure.Storage;
using LendingSystem.WebApi.Modules.Definitions;
using LendingSystem.WebApi.Options;
using Microsoft.Extensions.Options;
using Minio;

namespace LendingSystem.WebApi.Modules.Minio;

public sealed class MinioModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IMinioClient>(CreateMinioClient);
        services.AddSingleton<IObjectStorage, MinioObjectStorage>();

        return services;
    }

    private static IMinioClient CreateMinioClient(IServiceProvider provider)
    {
        var options = provider
            .GetRequiredService<IOptions<MinioOptions>>()
            .Value;

        return new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.Ssl)
            .Build();
    }
}
