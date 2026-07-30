using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Infrastructure.Video;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.VideoDownload;

public sealed class VideoDownloadModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddSingleton<IVideoDownloadClient, GrpcVideoDownloadClient>();
        return services;
    }
}
